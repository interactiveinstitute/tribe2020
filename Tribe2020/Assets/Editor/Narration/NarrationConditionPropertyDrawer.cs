using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;

[CustomPropertyDrawer(typeof(NarrationConditionAttribute))]
public class NarrationConditionPropertyDrawer : PropertyDrawer {

	//
	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
		bool conditionalProgress = GetConditionalProgress(property);
		Quest.QuestEvent type = GetCondition(property);
		//List<Quest.QuestEvent> validTypes = GetValidConditions((NarrationConditionAttribute)attribute);

		if(conditionalProgress) {
			EditorGUI.PropertyField(position, property, label, true);
		}

		//if(conditionalProgress && validTypes.Contains(type)) {
		//	EditorGUI.PropertyField(position, property, label, true);
		//}
	}

	//
	public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
		bool conditionalProgress = GetConditionalProgress(property);

		if(conditionalProgress) {
			return EditorGUI.GetPropertyHeight(property, label);
		} else {
			return -EditorGUIUtility.standardVerticalSpacing;
		}
	}

	//
	public bool GetConditionalProgress(SerializedProperty prop) {
		int index = int.Parse(prop.propertyPath.Split('[')[1].Split(']')[0]);
		SerializedProperty conditionBool =
			prop.serializedObject.FindProperty("steps").GetArrayElementAtIndex(index).FindPropertyRelative("conditionalProgress");

		return conditionBool.boolValue;
	}

	//Extract current selected type from serialized object
	private Quest.QuestEvent GetCondition(SerializedProperty prop) {
		int index = int.Parse(prop.propertyPath.Split('[')[1].Split(']')[0]);
		SerializedProperty typeProp =
			prop.serializedObject.FindProperty("steps").GetArrayElementAtIndex(index).FindPropertyRelative("condition");

		Quest.QuestEvent type = (Quest.QuestEvent)typeProp.enumValueIndex;

		return type;
	}

	//Extract list of types that make the property show from attributes
	private List<Quest.QuestEvent> GetValidConditions(NarrationConditionAttribute attribute) {
		List<Quest.QuestEvent> conditions = new List<Quest.QuestEvent>();
		string[] parse = attribute.parameter.Split(',');
		foreach(string p in parse) {
			if(p != "") {
				conditions.Add((Quest.QuestEvent)Enum.Parse(typeof(Quest.QuestEvent), p));
			}
		}

		return conditions;
	}
}
