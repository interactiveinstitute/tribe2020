using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(NarrationManager))]
public class NarrationManagerEditor : Editor {
	public override void OnInspectorGUI() {
		DrawDefaultInspector();

		NarrationManager myScript = (NarrationManager)target;


		if(GUILayout.Button("NextStep", GUILayout.Width(100))) {
			myScript.NextStep();
		}

		if(GUILayout.Button("PrevStep",GUILayout.Width(100))) {
			myScript.PrevStep();
		}

		if(GUILayout.Button("Restart Selected", GUILayout.Width(100))) {
			myScript.ResetSelected();
		}

		if(GUILayout.Button("Finish Selected", GUILayout.Width(100))) {
			myScript.FinishSelected();
		}
	}
}