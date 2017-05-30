using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(SaveManager))]
public class SaveManagerEditor : Editor {
	public override void OnInspectorGUI() {
		DrawDefaultInspector();

		SaveManager myScript = (SaveManager)target;

		if(GUILayout.Button("Clear All Data", GUILayout.Width(100))) {
			myScript.ClearFile();
		}
	}
}