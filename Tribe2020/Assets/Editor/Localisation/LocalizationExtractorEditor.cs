using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(LocalisationExtractor))]
public class LocalizationExtractorEditor : Editor {

	//Add elements while drawing the inspector
	public override void OnInspectorGUI() {
		DrawDefaultInspector();

		LocalisationExtractor script = (LocalisationExtractor)target;

		if(GUILayout.Button("Extract Text", GUILayout.Width(100))) {
			script.ExtractText();
		}
	}
}
