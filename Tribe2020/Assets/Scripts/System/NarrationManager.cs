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

	#region Fields
	private Controller _controller;

	//public Transform tutorialAnimation;
	public bool autoStart = true;
	public bool saveProgress = true;
	public bool debug = false;

	public Quest startQuest;
	public List<Quest> quests;
	public List<Quest> curQuests = new List<Quest>();
	public List<Quest> completedQuests = new List<Quest>();
	#endregion

	//Sort use instead of constructor
	void Awake() {
		_instance = this;
	}

	// Use this for initialization
	void Start() {
		_controller = Controller.GetInstance();
	}

	// Update is called once per frame
	void Update() {
	}

	//
	public void SetStartState() {
		_controller.ClearView();
		AddQuest(startQuest, 0);
	}

	//
	public void NextStep() {
		if(curQuests.Count > 0) {
			if(curQuests[0].IsComplete()) {
			} else {
				OnQuestEvent(curQuests[0].GetCurrentStep().condition, curQuests[0].GetCurrentStep().conditionField);
				StartQuestStep(curQuests[0]);
			}
		}
	}

	//
	public void PrevStep() {
		if(curQuests.Count > 0) {
			curQuests[0].PrevStep();

			if(curQuests[0].GetCurrentStepIndex() > 0 && curQuests[0].GetCurrentStep().condition == Quest.QuestEvent.EMPTY) {
				PrevStep();
			}
			StartQuestStep(curQuests[0]);
		}
	}

	//
	public void FinishNarrative() {
		while(curQuests.Count > 0 && curQuests[0].GetCurrentStep().type != Quest.QuestStepType.QuestComplete) {
			OnQuestEvent(curQuests[0].GetCurrentStep().condition, curQuests[0].GetCurrentStep().conditionField);
		}
	}

	//
	public List<Quest> GetQuests() {
		return curQuests;
	}

	//
	public List<Quest> GetCompletedQuests() {
		return completedQuests;
	}

	//
	public void AddQuest(int questIndex, int questStep) {
		AddQuest(quests[questIndex], questStep);
	}

	//
	public void AddQuest(Quest quest, int questStep) {
		if(debug) { Debug.Log(name + ":AddQuest " + quest.name); }

		Quest newQuest = Object.Instantiate(quest) as Quest;
		newQuest.SetCurrentStep(questStep);
		newQuest.date = _controller.GetCurrentDate();
		curQuests.Add(newQuest);

		if(autoStart) {
			StartQuestStep(newQuest);
		}
	}

	// Initializes a step of a quest depending on its type
	public void StartQuestStep(Quest quest) {
		Quest.Step step = quest.GetCurrentStep();
		if(debug) { Debug.Log(name + ": " + quest.title + " -> " + quest.GetCurrentStepType()); }

		// Limit user interaction
		_controller.SetControlState(quest.GetCurrentInteractionLimits());
		string id, action;

		switch(quest.GetCurrentStepType()) {
			case Quest.QuestStepType.Popup:
				//if(debug) { Debug.Log(name + ":Popup " + "Narrative." + quest.title + ":" + step.title); }
				_controller.ShowMessage("Narrative." + quest.title + ":" + step.title, step.message, false);
				break;
			case Quest.QuestStepType.Prompt:
				//if(debug) { Debug.Log(name + ":Prompt " + "Narrative." + quest.title + ":" + step.title); }
				_controller.ShowMessage("Narrative." + quest.title + ":" + step.title, step.message, true);
				break;
			case Quest.QuestStepType.PlayAnimation:
				_controller.ControlInterface("animation", "hide");
				_controller.ControlInterface("playAnimation", step.animation);
				break;
			case Quest.QuestStepType.PlaySound:
				_controller.PlaySound(step.sound);
				break;
			case Quest.QuestStepType.CreateHarvest:
				_controller.SendMessage(step.type.ToString(), step.commandJSON);
				break;
			case Quest.QuestStepType.ControlAvatar:
				id = quest.ParseAsString("id");
				action = quest.ParseAsString("action");
				Vector3 pos = quest.ParseAsVector3("pos");
				if(pos != Vector3.back) {
					_controller.ControlAvatar(id, action, pos);
				} else {
					_controller.ControlAvatar(id, step.activity);
				}
				break;
			case Quest.QuestStepType.ControlInterface:
				id = quest.ParseAsString("id");
				action = quest.ParseAsString("action");
				_controller.ControlInterface(id, action);
				break;
			case Quest.QuestStepType.ChangeTimeScale:
				_controller.SendMessage("SetTimeScale", step.timeScale);
				//_controller.SetTimeScale(step.timeScale);
				break;
			case Quest.QuestStepType.UnlockView:
				Vector2 coord = quest.ParseAsVector2("coord");
				_controller.UnlockView((int)coord.x, (int)coord.y);
				break;
			case Quest.QuestStepType.QuestComplete:
				_controller.ShowCongratualations("Narrative." + quest.title + ":Quest Complete");
				break;
			case Quest.QuestStepType.PilotComplete:
				break;
		}

		//If there are no conditions, just step to the next quest step
		if(!quest.GetCurrentStep().conditionalProgress) {
			OnQuestEvent(Quest.QuestEvent.EMPTY);
		} else if(quest.GetCurrentStep().condition == Quest.QuestEvent.FindView) {
			_controller.SendMessage("RequestCurrentView");
		}
	}

	// Called to send quest related event to all active quest. Progresses related quests
	// and starts next quest if quest fully progressed
	public void OnQuestEvent(Quest.QuestEvent questEvent, string argument = "") {
		if(debug) { Debug.Log(name + ": Received event " + questEvent + "(" + argument + ")"); }
		for(int i = curQuests.Count - 1; i >= 0; i--) {
			Quest curQuest = curQuests[i];
			if(curQuest.GetCurrentStep().condition == questEvent) {
				if(curQuest.GetCurrentStep().conditionField == "" ||
					curQuest.GetCurrentStep().conditionField == argument) {
					if(debug) { Debug.Log(name + ":" + curQuest.name + " progressed"); }

					_controller.ClearView();
					curQuest.NextStep();
					if(curQuest.IsComplete()) {
						completedQuests.Add(curQuest);
						curQuests.Remove(curQuest);

						if(curQuest.nextQuest != null) {
							AddQuest(curQuest.nextQuest, 0);
						}

						_controller.SetControlState(Controller.InputState.ALL);
						_controller.SaveGameState();
					} else {
						StartQuestStep(curQuests[i]);
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

	//
	public JSONClass SerializeAsJSON() {
		JSONClass json = new JSONClass();

		JSONArray questsJSON = new JSONArray();
		if(saveProgress) {
			foreach(Quest quest in curQuests) {
				JSONClass questJSON = new JSONClass();
				questJSON.Add("index", GetQuestIndex(quest).ToString());
				questJSON.Add("step", quest.GetCurrentStepIndex().ToString());
				questsJSON.Add(questJSON);
			}
		}
		json.Add("activeQuests", questsJSON);

		return json;
	}

	//
	public void DeserializeFromJSON(JSONClass json) {
		if(json != null) {
			curQuests.Clear();

			JSONArray questsJSON = json["activeQuests"].AsArray;
			foreach(JSONClass quest in questsJSON) {
				AddQuest(quest["index"].AsInt, quest["step"].AsInt);
			}
		} else {
			SetStartState();
		}
	}
}
