using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(BehaviourAI))]
public class BehaviourAIEditor : Editor {
	public override void OnInspectorGUI() {
		DrawDefaultInspector();

		//Set dirty, in order to save new data on game start
		BehaviourAI ai = (BehaviourAI)target;
		EditorUtility.SetDirty(ai);

		if(GUILayout.Button("CopyOldSchedule", GUILayout.Width(150))) {
			ai.schedules[0].items = ai.schedule;
		}

		if(GUILayout.Button("Next Activity", GUILayout.Width(150))) {
			ai.NextActivity ();
			ai.GetRunningActivity().Start();
		}
	}
}
