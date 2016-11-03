using UnityEngine;
using System.Collections.Generic;

public class LocalisationManager : MonoBehaviour {
	//Singleton features
	private static LocalisationManager _instance;
	public static LocalisationManager GetInstance() {
		return _instance;
	}

	//
	public List<Language> languages;
	public Language curLanguage;

	//Sort use instead of constructor
	void Awake() {
		_instance = this;

		if(languages.Count > 0) {
			curLanguage = languages[0];
		}
	}

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	//
	public string GetPhrase(string groupKey) {
		string[] parse = groupKey.Split(':');
		if(parse.Length == 2) {
			return GetPhrase(parse[0], parse[1]);
		}
		return "";
	}

	//
	public string GetPhrase(string group, string key) {
		foreach(Language.ValueGroup g in curLanguage.groups) {
			if(g.title == group) {
				foreach(Language.KeyValue keyValue in g.values) {
					if(key == keyValue.key) {
						if(curLanguage.isTestLanguage) {
							return "DEBUG:" + keyValue.value;
						} else {
							return keyValue.value;
						}
					}
				}
			}
		}
		return "";
	}
}
