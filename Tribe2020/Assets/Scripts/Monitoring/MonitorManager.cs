using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SimpleJSON;
using System.Net.NetworkInformation;
using WebSocketSharp;

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
	private bool _surveyPublished = false;
	[SerializeField]
	private double _playTime = 0;

	public string url = "wss://op-en.se";
	private WebSocket _webSocket;
	public List<string> pendingData;

	private AppServer _appServer;

	//
	void Awake() {
		_instance = this;
	}

	// Use this for initialization
	void Start() {
		if(_macAddress == "") {
			_macAddress = GetMACAddress();
		}
		_webSocket = new WebSocket(url + "/socket.io/?EIO=4&transport=websocket");

		_appServer = GetComponent<AppServer>();

		Publish("event", "event", "app started");
	}

	//
	void FixedUpdate() {
		_playTime += Time.unscaledDeltaTime;

		if(Application.internetReachability == NetworkReachability.NotReachable) {
			// 
		} else {
			if(_surveyCompleted && !_surveyPublished) {
				PublishSurvey();
			}

			if(pendingData.Count > 0) {
				//Debug.Log("sent: " + pendingData[0]);
				string[] data = pendingData[0].Split('&');
				_appServer.Publish(data[0], data[1]);
				//_webSocket.Send(pendingData[0]);
				pendingData.RemoveAt(0);
			}
		}
	}

	// Update is called once per frame
	void Update() {
	}

	//
	void OnDestroy() {
		Publish("event", "event", "app closed");
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

	//
	public void Publish(string topTopic, string key, string value) {
		JSONClass json = new JSONClass();
		json.Add("time", GetEpoch().ToString());
		json.Add(key, value);

		string topic = "gamedata/" + _macAddress + "/" + topTopic;
		pendingData.Add(topic + "&" + json.ToString());

		//gamedata/[mac]/survey
		//gamedata/[mac]/playtime {"time":[unix epoc time], "playtime":[playtime]}
		//
		//gamedata/[mac]/event {"time":[uet], "event":"app started"}
		//...
		//session started/ended
		//event
	}

	//
	public void PublishSurvey() {
		for(int i = 0; i < _questions.Count; i++) {
			Publish("surey", i + "", _questions[i].answer);
		}
		_surveyPublished = true;
	}

	public double GetEpoch() {
		System.DateTime epochStart = new System.DateTime(1970, 1, 1, 0, 0, 0, System.DateTimeKind.Utc);
		return (int)(System.DateTime.UtcNow - epochStart).TotalSeconds;
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
		json.Add("surveyPublished", "" + _surveyPublished);
		json.Add("playTime", "" + _playTime);

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
			_surveyPublished = json["surveyPublished"].AsBool;
			_playTime = json["playTime"].AsDouble;
		}
	}
}
