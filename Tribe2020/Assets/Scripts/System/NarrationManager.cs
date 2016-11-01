﻿using UnityEngine;
using System.Collections.Generic;
using SimpleJSON;


// Manages game narration that goes outside the base game behaviour, for instance
// showing a dialogue box, playing an overlay animation or injecting behavior into
// some aspect of the game in order to tell the story
public class NarrationManager : MonoBehaviour {
	//Singleton features
	private static NarrationManager _instance;
	public static NarrationManager GetInstance() {
		return _instance;
	}

	private Controller _controller;
	private View _view;
	private AudioManager _audioMgr;
	private GameTime _timeMgr;

	//public Transform tutorialAnimation;
	public bool autoStart;

	public Quest startQuest;
	public List<Quest> quests;
	public List<Quest> _curQuests = new List<Quest>();

	//Sort use instead of constructor
	void Awake() {
		_instance = this;
	}

	// Use this for initialization
	void Start() {
		_controller = Controller.GetInstance();
		_view = View.GetInstance();
		_audioMgr = AudioManager.GetInstance();
		_timeMgr = GameTime.GetInstance();
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
	public void AddQuest(int questIndex, int questStep) {
		Quest questInstance = Object.Instantiate(quests[questIndex]) as Quest;
		questInstance.SetCurrentStep(questStep);
		_curQuests.Add(questInstance);

		if(autoStart) {
			StartQuestStep(questInstance);
		}
	}

	// Initializes a step of a quest depending on its type
	public void StartQuestStep(Quest quest) {
		Quest.QuestStep step = quest.GetCurrentStep();

		// Limit user interaction
		_controller.SetControlState(quest.GetCurrentInteractionLimits());
		string id, action;
		Vector3 pos;

		if(_view != null) {
			switch(quest.GetCurrentStepType()) {
				case Quest.QuestStepType.Popup:
					_view.ShowMessage("Narrative." + quest.title + ":" + step.title, step.showAtBottom, false);
					break;
				case Quest.QuestStepType.Prompt:
					_view.ShowMessage("Narrative." + quest.title + ":" + step.title, step.showAtBottom);
					break;
				case Quest.QuestStepType.SendMail:
					//_view.ShowMessage(quest.GetArguments().text, quest.GetArguments().showAtBottom);
					break;
				case Quest.QuestStepType.PlayAnimation:
					_view.ControlInterface("animation", "show");
					_view.ControlInterface("playAnimation", step.valueField);
					//tutorialAnimation.gameObject.SetActive(true);
					//tutorialAnimation.GetComponent<Animation>().Play(step.valueField);
					break;
				case Quest.QuestStepType.PlaySound:
					_audioMgr.PlaySound(step.valueField);
					break;
				case Quest.QuestStepType.ControlAvatar:
					id = quest.ParseAsString("id");
					action = quest.ParseAsString("action");
					pos = quest.ParseAsVector3("pos");
					if(pos != Vector3.back) {
						_controller.ControlAvatar(id, action, pos);
					} else {
						_controller.ControlAvatar(id, step.objectField);
					}
					break;
				case Quest.QuestStepType.ControlInterface:
					id = quest.ParseAsString("id");
					action = quest.ParseAsString("action");
					_view.ControlInterface(id, action);
					break;
				case Quest.QuestStepType.ChangeTimeScale:
					_timeMgr.TimeScale = int.Parse(step.valueField);
					break;
				case Quest.QuestStepType.QuestComplete:
					_view.ShowCongratualations(step.valueField);
					break;
			}
		}

		//If there are no conditions, just step to the next quest step
		if(quest.GetCurrentStep().condition == Quest.QuestEvent.EMPTY) {
			OnQuestEvent(Quest.QuestEvent.EMPTY);
		}
	}

	// Called to send quest related event to all active quest. Progresses related quests
	// and starts next quest if quest fully progressed
	public void OnQuestEvent(Quest.QuestEvent questEvent, string argument = "") {
		for(int i = _curQuests.Count - 1; i >= 0; i--) {
			Quest curQuest = _curQuests[i];
			if(curQuest.GetCurrentStep().condition == questEvent) {
				if(curQuest.GetCurrentStep().conditionField == "" ||
					curQuest.GetCurrentStep().conditionField == argument) {
					Debug.Log("Narration: " + curQuest.name + " was progressed by event " + questEvent);

					if(_view != null) {
						_view.ClearView();
					}

					curQuest.NextStep();
					if(curQuest.IsComplete()) {
						_curQuests.Remove(curQuest);

						if(curQuest.nextQuest != null) {
							Quest nextQuest = Object.Instantiate(curQuest.nextQuest) as Quest;
							_curQuests.Add(nextQuest);
							StartQuestStep(nextQuest);
						}

						_controller.SaveGameState();
					} else {
						StartQuestStep(_curQuests[i]);
					}
				}
			}
		}
	}

	// Returns index for given quest
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

	// Help function to parse string with format "x,y,z" to Vector3
	public Vector3 StringToVector3(string vector3) {
		string[] coord = vector3.Split(',');
		Vector3 result = new Vector3(float.Parse(coord[0]), float.Parse(coord[1]), float.Parse(coord[2]));
		return result;
	}

	// Save function
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

	// Load function
	public void Decode(JSONClass questStateJSON) {
		JSONArray quests = questStateJSON["activeQuests"].AsArray;
		foreach(JSONClass quest in quests) {
			AddQuest(quest["index"].AsInt, quest["step"]["step"].AsInt);
		}

		//if(quests.Count == 0) {
		//	_curQuests.Add(Object.Instantiate(startQuest) as Quest);
		//	StartQuestStep(_curQuests[0]);
		//}
	}
}
