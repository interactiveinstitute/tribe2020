using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Quest))]
public class QuestEditor : Editor {

	//
	public override void OnInspectorGUI() {
		Quest script = target as Quest;
	}
}
