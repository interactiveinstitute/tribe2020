﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SimpleJSON;
using System.Net.NetworkInformation;

public class MonitorManager : MonoBehaviour {
	private static MonitorManager _instance;
	public static MonitorManager GetInstance() {
		return _instance;
	}

	[SerializeField]
	private string _macAddress;
	[SerializeField]
	private List<SurveyQuestion> _questions;
	[SerializeField]
	private bool _surveyCompleted = false;
	[SerializeField]
	private double playTime = 0;

	//
	void Awake() {
		_instance = this;
	}

	// Use this for initialization
	void Start() {
		if(_macAddress == "") {
			_macAddress = GetMACAddress();
		}
	}
	
	// Update is called once per frame
	void Update() {
		playTime += Time.unscaledDeltaTime;
	}

	//
	public string GetMACAddress() {
		IPGlobalProperties computerProperties = IPGlobalProperties.GetIPGlobalProperties();
		NetworkInterface[] nics = NetworkInterface.GetAllNetworkInterfaces();
		string mac = null;

		foreach(NetworkInterface adapter in nics) {
			PhysicalAddress address = adapter.GetPhysicalAddress();
			byte[] bytes = address.GetAddressBytes();
			
			for(int i = 0; i < bytes.Length; i++) {
				mac = string.Concat(mac + (string.Format("{0}", bytes[i].ToString("X2"))));
				if(i != bytes.Length - 1) {
					mac = string.Concat(mac + "-");
				}
			}
		}
		return mac;
	}

	//
	public void AddAnswer(int index, string question, string answer) {
		_questions[index].answer = answer;
	}

	//
	public List<SurveyQuestion> GetSurvey() {
		return _questions;
	}

	//
	public void CheckSurveyComplete() {
		bool result = true;

		foreach(SurveyQuestion question in _questions) {
			if(question.required && question.answer == SurveyQuestion.NO_ANSWER) {
				result = false;
			}
		}

		_surveyCompleted = result;
	}

	//
	public void LoadSurvey(SurveyQuestion[] questions) {
		_questions = new List<SurveyQuestion>(questions);
	}

	//Serialize narration manager state to json
	public JSONClass SerializeAsJSON() {
		JSONClass json = new JSONClass();

		JSONArray surveyJSON = new JSONArray();
		foreach(SurveyQuestion question in _questions) {
			surveyJSON.Add(question.answer);
		}
		json.Add("survey", surveyJSON);

		json.Add("surveyComplete", "" + _surveyCompleted);
		json.Add("playTime", "" + playTime);

		return json;
	}

	//Deserialize narration manager state from json and activate or init if empty
	public void DeserializeFromJSON(JSONClass json) {
		if(json != null) {
			JSONArray surveyJSON = json["survey"].AsArray;
			for(int i = 0; i < surveyJSON.AsArray.Count; i++) {
				_questions[i].answer = surveyJSON.AsArray[i];
			}

			_surveyCompleted = json["surveyComplete"].AsBool;
			playTime = json["playTime"].AsDouble;
		}
	}
}
