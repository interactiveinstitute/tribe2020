using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(Interactable))]
public class InteractableEditor : Editor {
	public override void OnInspectorGUI(){
		DrawDefaultInspector();
		
		Interactable script = target as Interactable;
		if(GUILayout.Button("Perform Measure")){
			script.enabled = true;
		}
	}
}
