using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "Quest", menuName = "Gameplay/Quest", order = 1)]
public class Quest : ScriptableObject {
	//Quest step types
	public enum QuestStepType {
		PromptMessage, PopUpMessage, SendMail, PlayAnimation, PlaySound, ControlAvatar, StartAvatarActivity, ChangeTimeScale,
		QuestComplete
	};

	//Quest events
	public enum QuestEvent { EMPTY, OKPressed, Swiped, Tapped, ApplianceSelected, ApplianceDeselected, QuestListOpened,
		QuestListClosed, QuestOpened, MeasurePerformed, FindView, AvatarArrived, AvatarSessionOver, AvatarActivityOver,
		ResourceHarvested
	};

	public string title;
	public Quest nextQuest;

	public List<QuestStep> questSteps;

	private int _curStep = 0;

	//Definition of a quest step
	[System.Serializable]
	public class QuestStep {
		public string title;
		//public string message;
		public QuestStepType questType;
		//public string animation;
		public PilotController.InputState inputState;
		public Argument arguments;
		//public PilotController.InputState inputState;
		//public QuestCondition condition;
		public List<QuestCondition> conditions;
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
	public Argument GetArguments() {
		return questSteps[_curStep].arguments;
	}

	//
	public QuestStep GetCurrentStep() {
		return questSteps[_curStep];
	}

	//
	public QuestStepType GetCurrentStepType() {
		return questSteps[_curStep].questType;
	}

	////
	//public string GetCurrentStepMessage() {
	//	return questSteps[_curStep].message;
	//}

	////
	//public string GetCurrentStepAnimation() {
	//	return questSteps[_curStep].animation;
	//}

	//
	public PilotController.InputState GetCurrentInteractionLimits() {
		return questSteps[_curStep].inputState;
	}

	//
	public void AttemptCompletion(QuestEvent questEvent, string argument) {
		List<QuestCondition> conditions = questSteps[_curStep].conditions;

		//Debug.Log("AttemptCompletion: " + questEvent + ", " + argument);
		foreach(QuestCondition condition in conditions) {
			if(condition.stepCondition == questEvent && (argument == condition.argument || condition.argument == "")) {
				condition.SetComplete(true);
			}
		}
	}

	//
	public bool IsCurrentStepComplete() {
		if(_curStep >= questSteps.Count) {
			return true;
		}

		bool result = true;

		List<QuestCondition> conditions = questSteps[_curStep].conditions;
		foreach(QuestCondition condition in conditions) {
			if(!condition.GetComplete()) {
				result = false;
			}
		}
		return result;
	}

	//
	public void NextStep() {
		_curStep++;
	}

	//
	public bool IsComplete() {
		return (_curStep >= questSteps.Count);
	}
}
