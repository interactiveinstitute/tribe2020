using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(TimeSeries))]
public class TimeSeriesEditor : Editor
{
	public override void OnInspectorGUI()
	{
		DrawDefaultInspector();

		TimeSeries myScript = (TimeSeries)target;


		if(GUILayout.Button("Load all",GUILayout.Width(100) ))
		{
			myScript.LoadFromCVSFile();
		}
		if(GUILayout.Button("Load missing",GUILayout.Width(100) ))
		{
			myScript.LoadFromCVSFile();
		}
		if(GUILayout.Button("Clear data",GUILayout.Width(100) ))
		{
			myScript.Clear();
		}
			
	}
}