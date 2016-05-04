


using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(ElectricMeter))]
public class ElectricMeterEditor : Editor
{
	public override void OnInspectorGUI()
	{
		DrawDefaultInspector();

		ElectricMeter myScript = (ElectricMeter)target;


		if(GUILayout.Button("Switch On",GUILayout.Width(100) ))
		{
			myScript.On();
		}

		if(GUILayout.Button("Switch Off",GUILayout.Width(100) ))
		{
			myScript.Off();
		}

		if(GUILayout.Button("Toggle",GUILayout.Width(100) ))
		{
			myScript.Toggle();
		}

		if(GUILayout.Button("Connect",GUILayout.Width(100) ))
		{
			myScript.Connect(myScript.PowerSource);
		}
			
	}
}