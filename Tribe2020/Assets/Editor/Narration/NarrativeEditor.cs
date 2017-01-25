using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System.Reflection;
using System.Linq;

[CustomEditor(typeof(Narrative))]
public class NarrativeEditor : Editor {
	private ReorderableList list;

	private void OnEnable() {
		list = new ReorderableList(serializedObject, serializedObject.FindProperty("steps"), true, true, true, true);
		list.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) => {
			//var element = list.serializedProperty.GetArrayElementAtIndex(index);
			//ReorderableList cList = 
			//	new ReorderableList(element, element.FindPropertyRelative("conditions"), true, true, true, true);

			//var element = list.serializedProperty.GetArrayElementAtIndex(index);
			//rect.y += 2;
			//rect.height += 120;

			//EditorGUI.PropertyField(
			//	new Rect(rect.x, rect.y, 120, 60),
			//	element.FindPropertyRelative("callback"), GUIContent.none);

			//EditorGUI.PropertyField(
			//	new Rect(rect.x + 120, rect.y, rect.width - 120, EditorGUIUtility.singleLineHeight),
			//	element.FindPropertyRelative("json"), GUIContent.none);

			//EditorGUI.PropertyField(
			//	new Rect(rect.x + rect.width - 30, rect.y + 30, 60, EditorGUIUtility.singleLineHeight),
			//	element.FindPropertyRelative("conditions"), GUIContent.none);

			//EditorGUI.PropertyField(
			//	new Rect(rect.x + rect.width - 30, rect.y, 30, EditorGUIUtility.singleLineHeight),
			//	element.FindPropertyRelative("Count"), GUIContent.none);
		};
	}

	public override void OnInspectorGUI() {
		base.OnInspectorGUI();

		//Narrative obj = target as Narrative;
		//if(obj != null) {
		//	int index;
		//	try {
		//		index = obj.methods
		//			.Select((v, i) => new { Name = v, Index = i })
		//			.First(x => x.Name == obj.MethodToCall)
		//			.Index;
		//	} catch {
		//		index = 0;
		//	}

		//	obj.MethodToCall = obj.methods[EditorGUILayout.Popup(index, obj.methods)];

		//	//string some = obj.methods[EditorGUILayout.Popup(index, obj.methods)];
		//}

		//serializedObject.Update();
		//list.DoLayoutList();
		//serializedObject.ApplyModifiedProperties();
	}
}
