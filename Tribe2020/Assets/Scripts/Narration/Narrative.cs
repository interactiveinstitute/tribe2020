using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(fileName = "Narrative", menuName = "Narration/Narrative", order = 1)]
public class Narrative : ScriptableObject {
	public List<Step> steps = new List<Step>();

	[Serializable]
	public struct Step {
		public string description;

		public UnityEvent unityEvent;

		//public string progressType;
		//public string progressProp;
		//public Type type;
		//public string json;
		public List<NarrativeCondition> conditions;
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
