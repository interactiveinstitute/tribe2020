using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(MainMeter))]
public class MainMeterEditor : Editor
{
	public override void OnInspectorGUI()
	{
		DrawDefaultInspector();

		MainMeter myScript = (MainMeter)target;


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

		if(GUILayout.Button("Reset",GUILayout.Width(100) ))
		{
			myScript.reset_energy();

		}


		if(GUILayout.Button("Connect",GUILayout.Width(100) ))
		{
			myScript.Connect(myScript.PowerSource);
		}



	}
}