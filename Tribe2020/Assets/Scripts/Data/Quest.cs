using UnityEngine;
using System.Collections.Generic;
using SimpleJSON;

[CreateAssetMenu(fileName = "Quest", menuName = "Gameplay/Quest", order = 1)]
public class Quest : ScriptableObject {
	//Quest step types
	public enum QuestStepType {
		Prompt, Popup, SendMail, PlayAnimation, PlaySound, ControlAvatar, StartAvatarActivity,
		ChangeTimeScale, QuestComplete, ControlInterface, Wait
	};

	//Quest events
	public enum QuestEvent {
		EMPTY, OKPressed, Swiped, Tapped, ApplianceSelected, ApplianceDeselected, QuestListOpened,
		QuestListClosed, QuestOpened, MeasurePerformed, FindView, AvatarArrived, AvatarSessionOver, AvatarActivityOver,
		ResourceHarvested, BattleOver, InspectorOpened, InspectorClosed, InboxOpened, InboxClosed, MailOpened, MailClosed,
		OpenEnergyPanel, CloseEnergyPanel, OpenComfortPanel, CloseComfortPanel, LightSwitchedOff, LightSwitchedOn
	};

	public string title;
	[TextArea(3, 10)]
	public string description;
	public string date;
	public Quest nextQuest;

	public List<Quest.QuestStep> questSteps;

	private int _curStep = 0;

	//Definition of a quest step
	[System.Serializable]
	public class QuestStep {
		public string title;
		public QuestStepType type;
		public Controller.InputState inputState;

		[TextArea(2, 10)]
		public string valueField;
		public Object objectField;
		public bool showAtBottom;

		public QuestEvent condition;
		public string conditionField;

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
	public QuestStep GetCurrentStep() {
		return questSteps[_curStep];
	}

	//
	public int GetCurrentStepIndex() {
		return _curStep;
	}

	//
	public QuestStepType GetCurrentStepType() {
		return questSteps[_curStep].type;
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
		return questSteps[_curStep].inputState;
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

		return _curStep >= questSteps.Count;

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
		if(_curStep < questSteps.Count) {
			_curStep++;
		}
	}

	//
	public bool IsComplete() {
		return (_curStep >= questSteps.Count);
	}

	//
	public int ParseAsInt(string key) {
		return JSON.Parse(GetCurrentStep().valueField)[key].AsInt;
		//return 0;
	}

	//
	public string ParseAsString(string key) {
		return JSON.Parse(GetCurrentStep().valueField)[key].Value;
		//return "";
	}

	//
	public Vector3 ParseAsVector3(string key) {
		if(JSON.Parse(GetCurrentStep().valueField)[key] != null) {
			float x = JSON.Parse(GetCurrentStep().valueField)[key][0].AsFloat;
			float y = JSON.Parse(GetCurrentStep().valueField)[key][1].AsFloat;
			float z = JSON.Parse(GetCurrentStep().valueField)[key][2].AsFloat;
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
