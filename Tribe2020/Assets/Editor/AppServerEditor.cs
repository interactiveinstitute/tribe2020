using System.Collections;
using System.Collections.Generic;
using UnityEngine;




using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(AppServer))]
public class AppServerEditor : Editor
{
	public override void OnInspectorGUI()
	{
		DrawDefaultInspector();

		AppServer myScript = (AppServer)target;


		if(GUILayout.Button("Connect",GUILayout.Width(100) ))
		{
			myScript.Start();
		}

		if(GUILayout.Button("Request",GUILayout.Width(100) ))
		{
			myScript.GetPeriod("test/signalA",double.NaN,double.NaN,null);
		}

		if(GUILayout.Button("Publish",GUILayout.Width(100) ))
		{
			myScript.Publish("~/test/publish","Hello!");
		}
			
	}
}