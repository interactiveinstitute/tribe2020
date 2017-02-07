using UnityEngine;
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
	//private Controller _controller;
	private NarrationInterface _interface;

	[Header("Debug Mode")]
	public bool autoStart = true;
	public bool saveProgress = true;
	public bool debug = false;

	[Header("Narratives")]
	public Narrative curInFocus;
	public List<Narrative> starters;
	public List<Narrative> active;
	public List<Narrative> archive;

	[Header("Old Solution")]
	public Quest startQuest;
	public List<Quest> quests;
	public List<Quest> curQuests = new List<Quest>();

	public Quest activeNarrative;
	public List<Quest> listeningNarratives;

	public List<Quest> completedQuests = new List<Quest>();

	[Header("Narration Control")]
	public int selectIndex;
	#endregion

	//Sort use instead of constructor
	void Awake() {
		_instance = this;
	}

	// Use this for initialization
	void Start() {
	}

	// Update is called once per frame
	void Update() {
	}

	//
	public void SetInterface(NarrationInterface i) {
		_interface = i;
	}

	//
	public NarrationInterface GetInterface() {
		return _interface;
	}

	//
	public void Init() {
		foreach(Narrative n in starters) {
			ActivateNarrative(n);
		}

		OnNarrativeEvent();
	}

	//
	public void SetStartState() {
		AddQuest(startQuest, selectIndex);

		foreach(Narrative n in starters) {
			ActivateNarrative(n);
		}
	}

	//
	public void ActivateNarrative(Narrative narrative) {
		Narrative n = Object.Instantiate(narrative) as Narrative;
		active.Add(n);
	}

	//Callback for game event, progress narratives that are listening for the event
	public void OnNarrativeEvent(string eventType = "", string prop = "") {
		bool didProgress = false;
		//For every active narrative, going backwards
		for(int i = active.Count - 1; i >= 0; i--) {
			Narrative.Step curStep = active[i].GetCurrentStep();
			//Check if event fullfills conditions for current step
			if((curStep.conditionType == "" || curStep.conditionType == eventType) &&
				(curStep.conditionProp == "" || curStep.conditionProp == prop)) {
				//Invoke event function
				curStep.unityEvent.Invoke();
				//Progress narrative
				active[i].Progress();
				//Check if narrative was completed
				if(active[i].IsComplete()) {
					//Temporarily store narrative for following narratives
					Narrative n = active[i];
					//Archive and remove completed narrative
					archive.Add(active[i]);
					active.RemoveAt(i);
					//Start eventual following narratives
					foreach(Narrative fn in n.followingNarratives) {
						ActivateNarrative(fn);
					}
					//Save game due to progress
					_interface.SaveGameState();
				}
				//Flag progress
				didProgress = true;
			}
		}
		//Fire empty event to start eventual new steps
		if(didProgress) {
			OnNarrativeEvent();
		}
	}

	//
	public void NextStep() {
		if(curQuests[selectIndex].IsComplete()) {
		} else {
			OnQuestEvent(curQuests[selectIndex].GetCurrentStep().condition, 
				curQuests[0].GetCurrentStep().conditionField);
			StartQuestStep(curQuests[selectIndex]);
		}

		//if(curQuests.Count > 0) {
		//	if(curQuests[0].IsComplete()) {
		//	} else {
		//		OnQuestEvent(curQuests[0].GetCurrentStep().condition, curQuests[0].GetCurrentStep().conditionField);
		//		StartQuestStep(curQuests[0]);
		//	}
		//}
	}

	//
	public void PrevStep() {
		curQuests[selectIndex].PrevStep();

		if(curQuests[selectIndex].GetCurrentStepIndex() > 0 && 
			curQuests[0].GetCurrentStep().condition == Quest.QuestEvent.EMPTY) {
			PrevStep();
		}
		StartQuestStep(curQuests[selectIndex]);

		//if(curQuests.Count > 0) {
		//	curQuests[0].PrevStep();

		//	if(curQuests[0].GetCurrentStepIndex() > 0 && curQuests[0].GetCurrentStep().condition == Quest.QuestEvent.EMPTY) {
		//		PrevStep();
		//	}
		//	StartQuestStep(curQuests[0]);
		//}
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
		newQuest.date = _interface.GetCurrentDate();
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
		_interface.SetControlState(quest.GetCurrentInteractionLimits());
		string id, action, localKey;

		switch(quest.GetCurrentStepType()) {
			case Quest.QuestStepType.Popup:
				localKey = "Narrative." + quest.title + ":" + step.title;

				_interface.ShowMessage(localKey, step.message, step.portrait, false);
				break;
			case Quest.QuestStepType.Prompt:
				localKey = "Narrative." + quest.title + ":" + step.title;
				_interface.ShowMessage(localKey, step.message, step.portrait, true);
				break;
			case Quest.QuestStepType.PlayAnimation:
				_interface.ControlInterface("animation", "hide");
				_interface.ControlInterface("playAnimation", step.animation);
				break;
			case Quest.QuestStepType.PlaySound:
				_interface.PlaySound(step.sound);
				break;
			case Quest.QuestStepType.CreateHarvest:
				_interface.CreateHarvest(step.commandJSON);
				break;
			case Quest.QuestStepType.ControlAvatar:
				id = quest.ParseAsString("id");
				action = quest.ParseAsString("action");
				Vector3 pos = quest.ParseAsVector3("pos");
				if(pos != Vector3.back) {
					_interface.ControlAvatar(id, action, pos);
				} else {
					_interface.ControlAvatar(id, step.activity);
				}
				break;
			case Quest.QuestStepType.ControlInterface:
				id = quest.ParseAsString("id");
				action = quest.ParseAsString("action");
				_interface.ControlInterface(id, action);
				break;
			case Quest.QuestStepType.ChangeTimeScale:
				_interface.SetTimeScale(step.timeScale);
				//_controller.SetTimeScale(step.timeScale);
				break;
			case Quest.QuestStepType.UnlockView:
				Vector2 coord = quest.ParseAsVector2("coord");
				_interface.UnlockView((int)coord.x, (int)coord.y);
				break;
			case Quest.QuestStepType.QuestComplete:
				_interface.ShowCongratualations("Narrative." + quest.title + ":Quest Complete");
				break;
			case Quest.QuestStepType.PilotComplete:
				break;
			case Quest.QuestStepType.MoveCamera:
				JSONNode json = JSON.Parse(step.commandJSON);
				_interface.MoveCamera(json["animation"]);
				break;
			case Quest.QuestStepType.StopCamera:
				_interface.StopCamera();
				break;
		}

		//If there are no conditions, just step to the next quest step
		if(!quest.GetCurrentStep().conditionalProgress) {
			OnQuestEvent(Quest.QuestEvent.EMPTY);
		} else if(quest.GetCurrentStep().condition == Quest.QuestEvent.FindView) {
			_interface.RequestCurrentView();
		}
	}

	//
	public void DoAction(string method, string prop) {
	}

	//
	public void OnEvent(string e, string prop) {
	}

	// Called to send quest related event to all active quest. Progresses related quests
	// and starts next quest if quest fully progressed
	public void OnQuestEvent(Quest.QuestEvent questEvent, string argument = "") {
		//if(debug) { Debug.Log(name + ": Received event " + questEvent + "(" + argument + ")"); }
		//for(int i = curQuests.Count - 1; i >= 0; i--) {
		//	Quest curQuest = curQuests[i];
		//	if(curQuest.GetCurrentStep().condition == questEvent) {
		//		if(curQuest.GetCurrentStep().conditionField == "" ||
		//			curQuest.GetCurrentStep().conditionField == argument) {
		//			if(debug) { Debug.Log(name + ":" + curQuest.name + " progressed"); }

		//			_interface.ClearView();
		//			curQuest.NextStep();
		//			if(curQuest.IsComplete()) {
		//				completedQuests.Add(curQuest);
		//				curQuests.Remove(curQuest);

		//				if(curQuest.nextQuest != null) {
		//					AddQuest(curQuest.nextQuest, 0);
		//				}

		//				_interface.SetControlState(Controller.InputState.ALL);
		//				_interface.SaveGameState();
		//			} else {
		//				StartQuestStep(curQuests[i]);
		//			}
		//		}
		//	}
		//}
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
