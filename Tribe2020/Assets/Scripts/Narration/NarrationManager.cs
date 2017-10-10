using UnityEngine;
using System.Collections.Generic;
using SimpleJSON;

/* 
Manages game narration that goes outside the base game behaviour, for instance
showing a dialogue box, playing an overlay animation or injecting behavior into
some aspect of the game in order to tell the story
*/
public class NarrationManager : MonoBehaviour {
	//Singleton features
	private static NarrationManager _instance;
	public static NarrationManager GetInstance() {
		return _instance;
	}

	#region Fields
	private NarrationInterface _interface;

	[Header("Debug Mode")]
	public bool autoStart = true;
	public bool saveProgress = true;
	public bool debug = false;

	[Header("Narratives")]
	public Narrative curInFocus;

	private Narrative[] _allNarratives;

	public List<Narrative> starters;
	public List<Narrative> active;
	public List<Narrative> archive;
	[SerializeField]
	public List<PerformedStep> _performedSteps = new List<PerformedStep>();

	public string[] eventTypes;

	[Header("References")]
	public GameObject interactionPoint;

	[Header("Narration Control")]
	public int selectIndex;
    #endregion

    //Sort use instead of constructor
    void Awake() {
        _instance = this;
    }

	// Use this for initialization
	void Start() {
		_allNarratives = Resources.LoadAll<Narrative>("Narratives");
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

		//OnNarrativeEvent();
	}

	//
	public void ActiveNarratives() {
		foreach(Narrative n in active) {
			//Call callbacks
			foreach(Narrative.Action action in n.GetCurrentStep().actions) {
				_interface.OnNarrativeAction(n, n.GetCurrentStep(), action.callback, action.GetParameters());
			}
		}
	}

	//TODO
	public void ResetSelected() {
		//TODO
	}

	//
	public void FinishSelected() {
		CompleteNarrative(active[selectIndex]);
	}

	//Clone narrative as own instance and create text related callback if step contains such values
	public Narrative ActivateNarrative(Narrative narrative) {
		Narrative n = Object.Instantiate(narrative) as Narrative;
		foreach(Narrative.Step s in n.steps) {
			n.SetCurrentStepIndex(narrative.GetCurrentStepIndex());
		}
		active.Add(n);
		return n;
	}

	//Callback for game event, progress narratives that are listening for the event
	public void OnNarrativeEvent(string eventType = "", string prop = "", bool storeEvent = false) {
		if(debug) { Debug.Log("Narrative: " + eventType + "(" + prop + ")"); }
		if(!autoStart) { return; }

		bool didProgress = false;
		//For every active narrative, going backwards
		for(int i = active.Count - 1; i >= 0; i--) {
			Narrative.Step curStep = active[i].GetCurrentStep();

			if(curStep.conditionType == "Check") {
				bool result = _interface.NarrativeCheck(curStep.conditionProp);
				if(result) {
					GotoStep(active[i], curStep.actions[0].callback);
				} else {
					GotoStep(active[i], curStep.actions[1].callback);
				}
				return;
			}

            //Check if event fullfills conditions for current step
            bool isCompleted =
				curStep.IsCompletedBy(eventType, prop) ||
				_interface.HasEventFired(active[i], curStep);

			//if (curStep.IsCompletedBy(eventType, prop)) {
			if(isCompleted) {
				//Call callbacks
				foreach(Narrative.Action action in curStep.actions) {
					_interface.OnNarrativeAction(active[i], curStep, action.callback, action.GetParameters());
				}
				//Progress narrative
				active[i].Progress();
				//Check if narrative was completed
				if(active[i].IsComplete()) {
					CompleteNarrative(active[i]);
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
    public bool IsPerformed(string eventType, string prop) {
        foreach(PerformedStep ps in _performedSteps) {
            if(ps.IsCompletedBy(eventType, prop)) {
                return true;
            }
        }
        return false;
    }

    //
    public void CompleteNarrative(Narrative n) {
		//Archive and remove completed narrative
		archive.Add(n);
		active.Remove(n);
		//Start eventual following narratives
		foreach(Narrative fn in n.followingNarratives) {
			ActivateNarrative(fn);
		}
		//Send callback as narrative is over
		_interface.OnNarrativeCompleted(n);
	}

	//Send event matching the condition of selected, active narrative
	public void NextStep() {
		OnNarrativeEvent(active[selectIndex].GetCurrentStep().conditionType,
			active[selectIndex].GetCurrentStep().conditionProp);
	}

	//TODO
	public void PrevStep() {
		//TODO
	}

	//
	public void GotoStep(Narrative narrative, string stepTitle) {
		for(int i = 0; i < narrative.steps.Count; i++) {
			if(narrative.steps[i].description == stepTitle) {
				narrative.SetCurrentStepIndex(i);
			}
		}
		OnNarrativeEvent();
	}

	//
	public int GetDBIndexForNarrative(Narrative narrative) {
		int count = 0;
		foreach(Narrative n in _allNarratives) {
			if(n.title == narrative.title) {
				break;
			}
			count++;
		}
		return count;
	}

	//
	public int GetNumberOfActiveChecklists() {
		int result = 0;
		foreach(Narrative n in active) {
			if(n.HasChecklist()) {
				result++;
			}
		}
		return result;
	}

	//
	public JSONClass SerializeNarrative(Narrative narrative) {
		JSONClass narrativeJSON = new JSONClass();
		narrativeJSON.Add("index", GetDBIndexForNarrative(narrative).ToString());
		narrativeJSON.Add("step", narrative.GetLatestCheckpoint().ToString());

		return narrativeJSON;
	}

	//
	public Narrative DeserializeNarrative(JSONClass narrativeJSON) {
		int index = narrativeJSON["index"].AsInt;
		int step = narrativeJSON["step"].AsInt;

		Narrative narrative = Object.Instantiate(_allNarratives[index]) as Narrative;
		//Narrative narrative = ActivateNarrative(allNarratives[index]);
		narrative.SetCurrentStepIndex(step);

		return narrative;
	}

	// Help function to parse string with format "x,y,z" to Vector3
	public Vector3 StringToVector3(string vector3) {
		string[] coord = vector3.Split(',');
		Vector3 result = new Vector3(float.Parse(coord[0]), float.Parse(coord[1]), float.Parse(coord[2]));
		return result;
	}

	//
	public Narrative[] GetAllNarratives() {
		_allNarratives = Resources.LoadAll<Narrative>("Narratives");
		return _allNarratives;
	}

	//Serialize narration manager state to json
	public JSONClass SerializeAsJSON() {
		JSONClass json = new JSONClass();
		if(saveProgress) {
			//Store active narratives
			JSONArray activeJSON = new JSONArray();
			foreach(Narrative n in active) {
				activeJSON.Add(SerializeNarrative(n));
			}
			json.Add("active", activeJSON);

			//Store archive of completed narratives
			JSONArray archiveJSON = new JSONArray();
			foreach(Narrative n in archive) {
				archiveJSON.Add(SerializeNarrative(n));
			}
			json.Add("archive", archiveJSON);

			//Store archive of stored events
			//JSONArray storedEventsJSON = new JSONArray();
			//foreach(PerformedStep ps in _performedSteps) {
			//	storedEventsJSON.Add(ps.Serialize());
			//	Debug.Log(storedEventsJSON.ToString());
			//}
			//json.Add("storedEvents", storedEventsJSON);
		}
		return json;
	}

	//Deserialize narration manager state from json and activate or init if empty
	public void DeserializeFromJSON(JSONClass json) {
		if(json != null) {
			active.Clear();
			archive.Clear();

			//Recover active narratives
			JSONArray activeJSON = json["active"].AsArray;
			foreach(JSONClass narrativeJSON in activeJSON) {
				ActivateNarrative(DeserializeNarrative(narrativeJSON));
			}

			//Recover archive of completed narratives
			JSONArray archiveJSON = json["archive"].AsArray;
			foreach(JSONClass narrativeJSON in archiveJSON) {
				archive.Add(DeserializeNarrative(narrativeJSON));
			}

			//Recover archive of stored events
			//JSONArray storedEventsJSON = json["storedEvents"].AsArray;
			//foreach(JSONClass storedEvent in storedEventsJSON) {
			//	_performedSteps.Add(new PerformedStep(storedEvent["event"], storedEvent["prop"]));
			//}
		} else {
			Init();
		}
	}

	//
	public struct PerformedStep {
		public PerformedStep(string t, string p) {
			conditionType = t;
			conditionProp = p;
		}

		public string conditionType;
		public string conditionProp;

		//
		public bool IsCompletedBy(string eventType, string prop) {
			return (conditionType == "" || conditionType == eventType) &&
					(conditionProp == "" || conditionProp == prop);
		}

		//
		public JSONClass Serialize() {
			JSONClass stepJSON = new JSONClass();
			stepJSON.Add("type", conditionType);
			stepJSON.Add("prop", conditionProp);
			return stepJSON;
		}
	}
}
