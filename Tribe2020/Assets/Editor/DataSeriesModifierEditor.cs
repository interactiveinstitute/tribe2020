
using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(DataSeriesModifier))]
public class DataSeriesModifierEditor : Editor
{
	public override void OnInspectorGUI()
	{
		DrawDefaultInspector();

		DataSeriesModifier myScript = (DataSeriesModifier)target;


		if(GUILayout.Button("Test",GUILayout.Width(100) ))
		{
			myScript.Test();
		}

	}
}