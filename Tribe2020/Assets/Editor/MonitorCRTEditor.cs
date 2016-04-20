using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(MonitorCRT))]
public class MonitorCRTEditor : Editor
{
	public override void OnInspectorGUI()
	{
		DrawDefaultInspector();

		MonitorCRT myScript = (MonitorCRT)target;


		if(GUILayout.Button("Turn on",GUILayout.Width(100) ))
		{
			myScript.On();
		}

		if(GUILayout.Button("Turn off",GUILayout.Width(100) ))
		{
			myScript.Off();
		}

		if(GUILayout.Button("Enable power saving",GUILayout.Width(100) ))
		{
			myScript.SetPowerSave(true);
		}

		if(GUILayout.Button("Disable power saving",GUILayout.Width(100) ))
		{
			myScript.SetPowerSave(false);
		}
	}
}