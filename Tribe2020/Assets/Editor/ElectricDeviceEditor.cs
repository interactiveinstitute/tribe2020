using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(ElectricDevice))]
public class ElectricDeviceEditor : Editor
{
	public override void OnInspectorGUI()
	{
		DrawDefaultInspector();

		ElectricDevice myScript = (ElectricDevice)target;


		if(GUILayout.Button("Turn on",GUILayout.Width(100) ))
		{
			myScript.TurnOn();
		}

		if(GUILayout.Button("Turn off",GUILayout.Width(100) ))
		{
			myScript.TurnOff();
		}
	}
}