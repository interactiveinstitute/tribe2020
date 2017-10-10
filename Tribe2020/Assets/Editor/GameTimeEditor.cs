using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(GameTime))]
public class GameTimeEditor : Editor
{
	public override void OnInspectorGUI()
	{
		DrawDefaultInspector();

		GameTime myScript = (GameTime)target;

		if(GUILayout.Button("Minute +",GUILayout.Width(100) ))
		{
			myScript.Offset(60);
		}

		if(GUILayout.Button("Minute -",GUILayout.Width(100) ))
		{
			myScript.Offset(-60);
		}

		if(GUILayout.Button("Hour +",GUILayout.Width(100) ))
		{
			myScript.Offset(3600);
		}

		if(GUILayout.Button("Hour -",GUILayout.Width(100) ))
		{
			myScript.Offset(-3600);
		}

		if(GUILayout.Button("Day +",GUILayout.Width(100) ))
		{
			myScript.Offset(86400);
		}

		if(GUILayout.Button("Day -",GUILayout.Width(100) ))
		{
			myScript.Offset(-86400);	
		}

		if(GUILayout.Button("Jump to realtime",GUILayout.Width(100) ))
		{
			myScript.JumpToRealtime();	
		}

		if(GUILayout.Button("Sort keypoints",GUILayout.Width(200) ))
		{
			myScript.KeyActions.Sort(new GameTime.CompareKeyAction() );	
		}
	}
}