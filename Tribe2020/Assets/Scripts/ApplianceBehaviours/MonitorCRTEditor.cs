﻿using UnityEngine;
using System.Collections;

using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(MonitorCRT))]
public class ElectricDeviceEditor : Editor
{
	public override void OnInspectorGUI()
	{
		DrawDefaultInspector();

		MonitorCRT myScript = (MonitorCRT)target;


		if(GUILayout.Button("Turn on",GUILayout.Width(100) ))
		{
			myScript.TurnOn();
		}

		if(GUILayout.Button("Turn off",GUILayout.Width(100) ))
		{
			myScript.TurnOff();
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