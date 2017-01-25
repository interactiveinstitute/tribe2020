using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "Language", menuName = "Localisation/Language", order = 1)]
public class Language : ScriptableObject {
	public Sprite flagSprite;
	public string code;

	//
	public bool isTestLanguage;
	
	//
	[System.Serializable]
	public struct KeyValue {
		public string key;
		[TextArea(3, 3)]
		public string value;
		public List<string> values;
	}

	//Definition of a quest step
	[System.Serializable]
	public class ValueGroup {
		public string title;
		public List<KeyValue> values;
	}

	public List<Language.ValueGroup> groups; 
}
