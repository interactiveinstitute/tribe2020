using UnityEngine;
using System.Collections.Generic;
using UnityEditor;

[CustomEditor(typeof(ApplianceManager))]
public class ApplianceManagerEditor : Editor {

	public override void OnInspectorGUI() {
		DrawDefaultInspector();

		ApplianceManager script = (ApplianceManager)target;
		if(GUILayout.Button("Refresh All Slots", GUILayout.Width(150))) {
			script.RefreshAllSlots();
		}
	}
}
