using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(fileName = "Narrative", menuName = "Narration/Narrative", order = 1)]
public class Narrative : ScriptableObject {
	public string title;
	[TextArea(3, 10)]
	public string description;
	public List<Step> steps;
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

	//
	[Serializable]
	public struct Step {
		public string description;

		public string conditionType;
		public string conditionProp;

		public List<Narrative.Action> actions;
		public bool inChecklist;

		//
		public bool IsCompletedBy(string eventType, string prop) {
			return (conditionType == "" || conditionType == eventType) &&
					(conditionProp == "" || conditionProp == prop);
		}
	}

	//
	[Serializable]
	public struct Action {
		public string callback;
		public string parameter1;
		[TextArea(3, 10)]
		public string parameter2;

		public string[] GetParameters() {
			string[] parameters = { parameter1, parameter2 };
			return parameters;
		}
	}
}
