using UnityEngine;
using System.Collections.Generic;

public class QuestController : MonoBehaviour {
	//Singleton features
	private static QuestController _instance;
	public static QuestController GetInstance() {
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

	public List<Quest> quests;
	private List<Quest> _curQuests;

	//Sort use instead of constructor
	void Awake() {
		_instance = this;
	}

	// Use this for initialization
	void Start () {
		_controller = PilotController.GetInstance();
		_view = PilotView.GetInstance();
		_avatarMgr = AvatarManager.GetInstance();
		_timeMgr = GameTime.GetInstance();

		_curQuests = new List<Quest>();
		_curQuests.Add(Object.Instantiate(quests[0]) as Quest);

		StartQuestStep(_curQuests[0]);
	}
	
	// Update is called once per frame
	void Update () {
	}

	//
	public List<Quest> GetQuests() {
		return _curQuests;
	}
	 
	//
	public void StartQuestStep(Quest quest) {
		//Debug.Log("StartQuestStep: " + quest.GetCurrentStepType().ToString());
		//Show animation
		if(quest.GetArguments().animation != "") {
			tutorialAnimation.gameObject.SetActive(true);
			tutorialAnimation.GetComponent<Animation>().Play(quest.GetArguments().animation);
		} else {
			tutorialAnimation.gameObject.SetActive(false);
		}

		//Limit user interaction
		_controller.SetControlState(quest.GetCurrentInteractionLimits());

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
			case Quest.QuestStepType.StartAvatarActivity:
				_avatarMgr.MakeAvatarPerformActivity(quest.GetArguments().activity);
				break;
			case Quest.QuestStepType.ChangeTimeScale:
				_timeMgr.TimeScale = quest.GetArguments().value;
				break;
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
	}

	//
	public Vector3 StringToVector3(string vector3) {
		string[] coord = vector3.Split(',');
		Vector3 result = new Vector3(float.Parse(coord[0]), float.Parse(coord[1]), float.Parse(coord[2]));
		return result;
	}
}
