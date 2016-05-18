using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(Lamp))]
public class LampEditor : Editor
{
	public override void OnInspectorGUI()
	{
		DrawDefaultInspector();

		Lamp myScript = (Lamp)target;


		if(GUILayout.Button("Turn on",GUILayout.Width(100) ))
		{
			myScript.On();
		}

		if(GUILayout.Button("Turn off",GUILayout.Width(100) ))
		{
			myScript.Off();
		}

		if(GUILayout.Button("Update",GUILayout.Width(100) ))
		{
			myScript.update_energy();
		}

		if(GUILayout.Button("Reset meter",GUILayout.Width(100) ))
		{
			myScript.reset_energy();	
		}
	}
}