using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(Graph))]
public class GraphEditor : Editor
{
	public override void OnInspectorGUI()
	{
		DrawDefaultInspector();

		Graph myScript = (Graph)target;


		if(GUILayout.Button("Plot",GUILayout.Width(100) ))
		{
			myScript.Plot();
		}

	
	}
}