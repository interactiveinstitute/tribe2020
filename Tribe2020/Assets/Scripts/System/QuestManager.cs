using UnityEngine;
using System.Collections.Generic;
using SimpleJSON;

public class QuestManager : MonoBehaviour {
	//Singleton features
	private static QuestManager _instance;
	public static QuestManager GetInstance() {
		if(_instance != null) {
			return _instance;
		}
		return null;
	}

	private PilotController _controller;
	private PilotView _view;
	private AvatarManager _avatarMgr;
	private GameTime _timeMgr;

	public Transform tutorialAnimation;

	public Quest startQuest;
	public List<Quest> quests;
	private List<Quest> _curQuests = new List<Quest>();

	//Sort use instead of constructor
	void Awake() {
		_instance = this;
	}

	// Use this for initialization
	void Start() {
		_controller = PilotController.GetInstance();
		_view = PilotView.GetInstance();
		_avatarMgr = AvatarManager.GetInstance();
		_timeMgr = GameTime.GetInstance();

		//_curQuests = new List<Quest>();
		//if(quests.Count > 0) {
		//	_curQuests.Add(Object.Instantiate(quests[0]) as Quest);

		//	StartQuestStep(_curQuests[0]);
		//}
	}

	// Update is called once per frame
	void Update() {
	}

	//
	public void SetStartState() {
		AddQuest(0, 0);

	}

	//
	public List<Quest> GetQuests() {
		return _curQuests;
	}

	//
	public void SetQuests() {
	}

	//
	public void AddQuest(int questIndex, int questStep) {
		Quest questInstance = Object.Instantiate(quests[questIndex]) as Quest;
		questInstance.SetCurrentStep(questStep);
		_curQuests.Add(questInstance);
		StartQuestStep(questInstance);
	}

	//
	public void StartQuestStep(Quest quest) {
		//Show animation
		if(tutorialAnimation != null) {
			if(quest.GetArguments().animation != "") {
				tutorialAnimation.gameObject.SetActive(true);
				tutorialAnimation.GetComponent<Animation>().Play(quest.GetArguments().animation);
			} else {
				tutorialAnimation.gameObject.SetActive(false);
			}
		}

		//Limit user interaction
		if(_controller != null) {
			_controller.SetControlState(quest.GetCurrentInteractionLimits());
		}

		if(_view != null) {
			switch(quest.GetCurrentStepType()) {
				case Quest.QuestStepType.PopUpMessage:
					_view.ShowMessage(quest.GetArguments().text, quest.GetArguments().showAtBottom, false);
					break;
				case Quest.QuestStepType.PromptMessage:
					_view.ShowMessage(quest.GetArguments().text, quest.GetArguments().showAtBottom);
					break;
				case Quest.QuestStepType.SendMail:
					_view.ShowMessage(quest.GetArguments().text, quest.GetArguments().showAtBottom);
					break;
				case Quest.QuestStepType.PlaySound:
					_view.ShowMessage(quest.GetArguments().text, quest.GetArguments().showAtBottom);
					break;
				case Quest.QuestStepType.ControlAvatar:
					_avatarMgr.MakeAvatarWalkTo(quest.GetArguments().position);
					break;
				case Quest.QuestStepType.ControlInterface:
					_view.HideAppliance();
					break;
				case Quest.QuestStepType.StartAvatarActivity:
					_avatarMgr.MakeAvatarPerformActivity(quest.GetArguments().activity);
					break;
				case Quest.QuestStepType.ChangeTimeScale:
					_timeMgr.TimeScale = quest.GetArguments().value;
					break;
				case Quest.QuestStepType.QuestComplete:
					_view.ShowCongratualations(quest.GetArguments().text);
					break;
			}
		}

		//If there are no conditions, just step to the next quest step
		if(quest.IsCurrentStepComplete()) {
			OnQuestEvent(Quest.QuestEvent.EMPTY);
		}
	}

	//
	public void OnQuestEvent(Quest.QuestEvent questEvent) {
		OnQuestEvent(questEvent, "");
	}

	//
	public void OnQuestEvent(Quest.QuestEvent questEvent, string argument) {
		for(int i = _curQuests.Count - 1; i >= 0; i--) {
			_curQuests[i].AttemptCompletion(questEvent, argument);
			if(_curQuests[i].IsCurrentStepComplete()) {
				if(_view != null) {
					_view.messageUI.SetActive(false);
					_view.congratsPanel.SetActive(false);
				}
				_curQuests[i].NextStep();
				if(_curQuests[i].IsComplete()) {
					if(_curQuests[i].nextQuest != null) {
						Quest nextQuest = Object.Instantiate(_curQuests[i].nextQuest) as Quest;
						_curQuests.Add(nextQuest);
						StartQuestStep(nextQuest);
					}

					_curQuests.Remove(_curQuests[i]);
				} else {
					StartQuestStep(_curQuests[i]);
				}
			}
		}

		Debug.Log("OnQuestEvent:" + questEvent);
	}

	//
	public int GetQuestIndex(Quest quest) {
		int count = 0;
		foreach(Quest q in quests) {
			if(q.title == quest.title) {
				break;
			}
			count++;
		}
		return count;
	}

	//
	public Vector3 StringToVector3(string vector3) {
		string[] coord = vector3.Split(',');
		Vector3 result = new Vector3(float.Parse(coord[0]), float.Parse(coord[1]), float.Parse(coord[2]));
		return result;
	}

	//
	public JSONClass Encode() {
		JSONClass questStateJSON = new JSONClass();

		JSONArray questsJSON = new JSONArray();
		foreach(Quest quest in _curQuests) {
			JSONClass questJSON = new JSONClass();
			questJSON.Add("index", GetQuestIndex(quest).ToString());
			questJSON.Add("step", quest.Encode());

			questsJSON.Add(questJSON);
		}
		questStateJSON.Add("activeQuests", questsJSON);

		return questStateJSON;
	}

	//
	public void Decode(JSONClass questStateJSON) {
		JSONArray quests = questStateJSON["activeQuests"].AsArray;
		foreach(JSONClass quest in quests) {
			//Debug.Log("A quest: " + quest.ToString());
			AddQuest(quest["index"].AsInt, quest["step"]["step"].AsInt);
		}

		if(quests.Count == 0) {
			_curQuests.Add(Object.Instantiate(startQuest) as Quest);
			StartQuestStep(_curQuests[0]);
		}

		//Debug.Log("DecodeQuestState: " + questStateJSON.ToString());
	}
}
