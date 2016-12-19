using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;

[CustomPropertyDrawer(typeof(NarrationStepAttribute))]
public class NarrationStepPropertyDrawer : PropertyDrawer {

	//
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
		Quest.QuestStepType type = GetStepType(property);
		List<Quest.QuestStepType> validTypes = GetValidTypes((NarrationStepAttribute)attribute);
		bool containsType = validTypes.Contains(type);

		if(containsType) {
			EditorGUI.PropertyField(position, property, label, true);
			UpdateTitle(containsType, property);
		}

		//UpdateConditionVisibility(property);
    }

	//
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
		Quest.QuestStepType type = GetStepType(property);
		List<Quest.QuestStepType> validTypes = GetValidTypes((NarrationStepAttribute)attribute);

		if(validTypes.Contains(type)) {
			return EditorGUI.GetPropertyHeight(property, label);
		} else {
			return -EditorGUIUtility.standardVerticalSpacing;
		}
    }

	//Update title shown in list, descriptive of set properties
	private void UpdateTitle(bool containsType, SerializedProperty prop) {
		int index = int.Parse(prop.propertyPath.Split('[')[1].Split(']')[0]);
		SerializedProperty titleProp =
			prop.serializedObject.FindProperty("steps").GetArrayElementAtIndex(index).FindPropertyRelative("title");

		if( titleProp.stringValue == "" ||
			titleProp.stringValue.Split('.')[0] != GetStepType(prop).ToString() ||
			(prop.propertyType == SerializedPropertyType.String &&
			titleProp.stringValue.Split('.')[1] != prop.stringValue.Substring(0, Math.Min(prop.stringValue.Length, 50)))) {
			if(containsType && prop.type == "string") {
				titleProp.stringValue = GetStepType(prop) + "." + prop.stringValue.Substring(0, Math.Min(prop.stringValue.Length, 25));
			} else if(containsType && prop.type == "float") {
				titleProp.stringValue = GetStepType(prop) + "." + prop.floatValue;
			} else if(GetStepType(prop).ToString() == "Wait") {
				titleProp.stringValue = GetStepType(prop) + "." + prop.enumNames[prop.enumValueIndex];
			} else {
				titleProp.stringValue = GetStepType(prop) + "." + index;
			}
		}
	}

	//Extract current selected type from serialized object
	private Quest.QuestStepType GetStepType(SerializedProperty prop) {
		int index = int.Parse(prop.propertyPath.Split('[')[1].Split(']')[0]);
		SerializedProperty typeProp = 
			prop.serializedObject.FindProperty("steps").GetArrayElementAtIndex(index).FindPropertyRelative("type");

		Quest.QuestStepType type = (Quest.QuestStepType)typeProp.enumValueIndex;

		return type;
	}

	//Extract list of types that make the property show from attributes
	private List<Quest.QuestStepType> GetValidTypes(NarrationStepAttribute attribute) {
		List<Quest.QuestStepType> types = new List<Quest.QuestStepType>();
		string[] parse = attribute.parameter.Split(',');
		foreach(string p in parse) {
			if(p != "") {
				types.Add((Quest.QuestStepType)Enum.Parse(typeof(Quest.QuestStepType), p));
			}
		}

		return types;
	}

	//
	//public void UpdateConditionVisibility(SerializedProperty prop) {
	//	int index = int.Parse(prop.propertyPath.Split('[')[1].Split(']')[0]);
	//	SerializedProperty conditionBool =
	//		prop.serializedObject.FindProperty("steps").GetArrayElementAtIndex(index).FindPropertyRelative("conditionalProgress");
	//	bool conditionalProgress = conditionBool.boolValue;

	//}
}
