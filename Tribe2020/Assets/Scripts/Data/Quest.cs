using UnityEngine;
using System.Collections.Generic;
using SimpleJSON;

[CreateAssetMenu(fileName = "Quest", menuName = "Gameplay/Quest", order = 1)]
public class Quest : ScriptableObject {
	//Quest step types
	public enum QuestStepType {
		Prompt, Popup, SendMail, PlayAnimation, PlaySound, ControlAvatar, StartAvatarActivity,
		ChangeTimeScale, QuestComplete, ControlInterface, Wait, UnlockView, PilotComplete,
		PilotFailed, CreateHarvest
	};

	//Quest events
	public enum QuestEvent {
		EMPTY, OKPressed, Swiped, Tapped, ApplianceSelected, ApplianceDeselected, QuestListOpened,
		QuestListClosed, QuestOpened, MeasurePerformed, FindView, AvatarArrived, AvatarSessionOver, AvatarActivityOver,
		ResourceHarvested, BattleOver, InspectorOpened, InspectorClosed, InboxOpened, InboxClosed, MailOpened, MailClosed,
		OpenEnergyPanel, CloseEnergyPanel, OpenComfortPanel, CloseComfortPanel, LightSwitchedOff, LightSwitchedOn, AvatarSelected,
		SelectedOverview, SelectedGridView
	};

	

	public string title;
	[TextArea(3, 10)]
	public string description;
	public string date;
	public List<Quest.NarrativeCheck> checkList;
	public Quest nextQuest;

	//public List<Quest.QuestStep> questSteps;

	public List<Quest.Step> steps;

	//public List<NarrationStep> nSteps;

	private int _curStep = 0;

	//
	[System.Serializable]
	public struct NarrativeCheck {
		public string description;
		public bool done;
	}

	//Definition of a quest step
	[System.Serializable]
	public class QuestStep {
		public string title;
		public Controller.InputState inputState;
		public QuestStepType type;

		[TextArea(2, 10)]
		public string valueField;
		public Object objectField;
		public bool showAtBottom;

		public QuestEvent condition;
		public string conditionField;

		//public Argument arguments;
		//public List<QuestCondition> conditions;
	}

	//Definition of a quest step
	[System.Serializable]
	public class Step {
		public string title;
		public QuestStepType type;

		//[TextArea(2, 10)]
		[NarrationStep("Prompt,Popup,QuestComplete", true)]
		public string message;
		[NarrationStep("Prompt,Popup")]
		public Sprite portrait;

		//[TextArea(2, 10)]
		[HideInInspector]
		public string valueField;

		[NarrationStep("UnlockView", true)]
		public Viewpoint viewpoint;

		[NarrationStep("PlayAnimation", true)]
		public string animation;

		[NarrationStep("ControlAvatar,ControlInterface,CreateHarvest", true)]
		public string commandJSON;

		[NarrationStep("ControlAvatar")]
		public AvatarActivity activity;

		[NarrationStep("ChangeTimeScale", true)]
		public float timeScale;

		[NarrationStep("PlaySound", true)]
		public string sound;

		[HideInInspector]
		public Object objectField;
		[HideInInspector]
		public bool showAtBottom;

		public bool conditionalProgress;
		[NarrationCondition]
		public QuestEvent condition;
		[NarrationCondition("ApplianceSelected")]
		public string conditionField;

		public Controller.InputState inputState;

		//public Argument arguments;
		//public List<QuestCondition> conditions;
	}

	//
	[System.Serializable]
	public class Argument {
		public string text;
		public float value;
		public Vector3 position;
		public AvatarActivity activity;
		public string animation;
		public bool showAtBottom;
	}

	//
	[System.Serializable]
	public class QuestCondition {
		public QuestEvent stepCondition;
		public string argument;
		private bool isCompleted;

		public void SetComplete(bool isComplete) {
			this.isCompleted = isComplete;
		}

		public bool GetComplete() {
			return isCompleted;
		}
	}

	//
	//public Argument GetArguments() {
	//	return questSteps[_currSession].arguments;
	//}

	//
	public void SetCurrentStep(int step) {
		_curStep = step;
	}

	//
	public Step GetCurrentStep() {
		return steps[_curStep];
	}

	//
	public int GetCurrentStepIndex() {
		return _curStep;
	}

	//
	public QuestStepType GetCurrentStepType() {
		return steps[_curStep].type;
	}

	////
	//public string GetCurrentStepMessage() {
	//	return questSteps[_currSession].message;
	//}

	////
	//public string GetCurrentStepAnimation() {
	//	return questSteps[_currSession].animation;
	//}

	//
	public Controller.InputState GetCurrentInteractionLimits() {
		return steps[_curStep].inputState;
	}

	//
	public void AttemptCompletion(QuestEvent questEvent, string argument) {
		//List<QuestCondition> conditions = questSteps[_currSession].conditions;

		//Debug.Log("AttemptCompletion: " + questEvent + ", " + argument);
		//foreach(QuestCondition condition in conditions) {
		//	if(condition.stepCondition == questEvent && (argument == condition.argument || condition.argument == "")) {
		//		condition.SetComplete(true);
		//	}
		//}
	}

	//
	public bool IsCurrentStepComplete() {
		//if(_currSession >= questSteps.Count) {
		//	return true;
		//}

		return _curStep >= steps.Count;

		//bool result = true;

		//List<QuestCondition> conditions = questSteps[_currSession].conditions;
		//foreach(QuestCondition condition in conditions) {
		//	if(!condition.GetComplete()) {
		//		result = false;
		//	}
		//}
		//return result;
	}

	//
	public void NextStep() {
		if(_curStep < steps.Count) {
			_curStep++;
		}
	}

	//
	public void PrevStep() {
		if(_curStep > 0) {
			_curStep--;
		}
	}

	//
	public bool IsComplete() {
		return (_curStep >= steps.Count);
	}

	//
	public int ParseAsInt(string key) {
		return JSON.Parse(GetCurrentStep().commandJSON)[key].AsInt;
		//return 0;
	}

	//
	public string ParseAsString(string key) {
		return JSON.Parse(GetCurrentStep().commandJSON)[key].Value;
		//return "";
	}

	//
	public Vector2 ParseAsVector2(string key) {
		if(JSON.Parse(GetCurrentStep().commandJSON)[key] != null) {
			float x = JSON.Parse(GetCurrentStep().commandJSON)[key][0].AsFloat;
			float y = JSON.Parse(GetCurrentStep().commandJSON)[key][1].AsFloat;

			return new Vector2(x, y);
		}

		return Vector2.zero;
	}

	//
	public Vector3 ParseAsVector3(string key) {
		if(JSON.Parse(GetCurrentStep().commandJSON)[key] != null) {
			float x = JSON.Parse(GetCurrentStep().commandJSON)[key][0].AsFloat;
			float y = JSON.Parse(GetCurrentStep().commandJSON)[key][1].AsFloat;
			float z = JSON.Parse(GetCurrentStep().commandJSON)[key][2].AsFloat;
			return new Vector3(x, y, z);
		}

		return Vector3.back;
		//return Vector3.zero;
	}

	//
	public JSONClass Encode() {
		JSONClass questStepJSON = new JSONClass();

		questStepJSON.Add("step", _curStep.ToString());

		return questStepJSON;
	}

	//
	public void Decode(JSONClass questStepJSON) {
		//Debug.Log("DecodeQuestStep: " + questStepJSON.ToString());
		_curStep = questStepJSON[_curStep].AsInt;
	}
}
