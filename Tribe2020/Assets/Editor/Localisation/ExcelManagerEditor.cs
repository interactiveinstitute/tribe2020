using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ExcelManager))]
public class ExcelManagerEditor : Editor {

	//Add elements while drawing the inspector
	public override void OnInspectorGUI() {
		DrawDefaultInspector();

		ExcelManager script = (ExcelManager)target;

		if(GUILayout.Button("Export to .xls", GUILayout.Width(150))) {
			script.ExportAllExcels();
		}
	}
}
