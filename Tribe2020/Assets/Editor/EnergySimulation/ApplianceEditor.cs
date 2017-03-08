using UnityEngine;
using System.Collections.Generic;
using UnityEditor;

[CustomEditor(typeof(Appliance))]
public class ApplianceEditor : Editor {

	public override void OnInspectorGUI() {
		DrawDefaultInspector();

		Appliance script = (Appliance)target;
		if(GUILayout.Button("Refresh Slots", GUILayout.Width(100))) {
			script.RefreshSlots();
		}
	}
}