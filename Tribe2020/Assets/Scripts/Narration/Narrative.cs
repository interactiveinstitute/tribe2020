using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(fileName = "Narrative", menuName = "Narration/Narrative", order = 1)]
public class Narrative : ScriptableObject {
	public string title;
	public List<Step> steps = new List<Step>();
	private int _curStep = 0;

	public List<Narrative> followingNarratives;

	//
	public Step GetCurrentStep() {
		return steps[_curStep];
	}

	//
	public int GetCurrentStepIndex() {
		return _curStep;
	}

	//
	public void SetCurrentStepIndex(int i) {
		_curStep = i;
	}

	//
	public void Progress() {
		_curStep++;
	}

	//
	public bool IsComplete() {
		return _curStep >= steps.Count;
	}

	[Serializable]
	public struct Step {
		public string description;

		public string conditionType;
		public string conditionProp;

		public UnityEvent unityEvent;

		//
		public bool IsCompletedBy(string eventType, string prop) {
			return (conditionType == "" || conditionType == eventType) &&
					(conditionProp == "" || conditionProp == prop);
		}
	}

	

	[Serializable]
	public struct NarrativeCondition {
		public enum Condition {
			EMPTY, OKPressed, Swiped, Tapped, ApplianceSelected, ApplianceDeselected, QuestListOpened,
			QuestListClosed, QuestOpened, MeasurePerformed, FindView, AvatarArrived, AvatarSessionOver, AvatarActivityOver,
			ResourceHarvested, BattleOver, InspectorOpened, InspectorClosed, InboxOpened, InboxClosed, MailOpened, MailClosed,
			OpenEnergyPanel, CloseEnergyPanel, OpenComfortPanel, CloseComfortPanel, LightSwitchedOff, LightSwitchedOn, AvatarSelected,
			SelectedOverview, SelectedGridView, CameraMoveOver, CameraAnimationEvent
		}

		public string type;
		public string prop;
	}
}
