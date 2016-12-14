using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(DataSeriesBuffer))]
public class DataSeriesBufferEditor : Editor
{
	public override void OnInspectorGUI()
	{
		DrawDefaultInspector();

		DataSeriesBuffer myScript = (DataSeriesBuffer)target;


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