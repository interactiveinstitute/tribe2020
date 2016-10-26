using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(InjectNode))]
public class InjectNodeEditor : Editor
{
	public override void OnInspectorGUI()
	{
		DrawDefaultInspector();

		InjectNode myScript = (InjectNode)target;


		if(GUILayout.Button("Inject",GUILayout.Width(100) ))
		{
			myScript.Inject();
		}


	}
}