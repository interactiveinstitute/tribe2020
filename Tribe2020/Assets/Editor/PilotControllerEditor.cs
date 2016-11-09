using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(PilotController))]
public class PilotControllerEditor : Editor {
	public override void OnInspectorGUI() {
		DrawDefaultInspector();

		PilotController myScript = (PilotController)target;


		if(GUILayout.Button("Save State", GUILayout.Width(100))) {
			myScript.SaveGameState();
		}

		if(GUILayout.Button("Load State",GUILayout.Width(100))) {
			myScript.LoadGameState();
		}

		if(GUILayout.Button("Generate Unique IDs", GUILayout.Width(200))) {
			myScript.GenerateUniqueIDs();
		}
	}
}