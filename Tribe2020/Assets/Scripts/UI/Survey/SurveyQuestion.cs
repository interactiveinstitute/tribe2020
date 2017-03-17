using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SurveyQuestion : MonoBehaviour {
	public const string NO_ANSWER = "n/a";
	public enum Type { Buttons, Write, Dropdown, Grade, Info };

	public Type type;
	public bool required = true;
	public bool include = true;
	public string question;
	public string answer = NO_ANSWER;

	// Use this for initialization
	void Start () {
		if(GetComponentInChildren<Text>()) {
			question = GetComponentInChildren<Text>().text;
		}
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
