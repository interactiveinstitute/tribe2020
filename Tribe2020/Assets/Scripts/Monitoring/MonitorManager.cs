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

	public bool live = false;
	public bool debug = false;
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

	bool isSent = false;
	float dTimer = 0;

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
	}

	// Update is called once per frame
	void Update() {
		_playTime += Time.unscaledDeltaTime;

		if(!live) { return; }

		if(Application.internetReachability == NetworkReachability.NotReachable) {
			// 
		} else {
			if(_surveyCompleted && !_surveyPublished) {
				PublishSurvey();
			}

			if(_appServer != null && _appServer.IsConnected && pendingData.Count > 0) {
				string[] data = pendingData[0].Split('&');
				if(debug) { Debug.Log("topic: " + data[0] + ", payload: " + data[1]); }
				_appServer.Publish(data[0], data[1]);

				pendingData.RemoveAt(0);
			}
		}

		//Timed app started
		dTimer += Time.deltaTime;
		if(dTimer > 1 && !isSent) {
			Publish("event", "event", "app started");
			isSent = true;
		}

		if(dTimer > 30) {
			Publish("playtime", "playtime", "" + _playTime);
			dTimer = 0;
		}
	}

	//
	void OnApplicationQuit() {
		PublishInstant("event", "event", "app closed");
	}

	//
	public string GetMACAddress() {
#if UNITY_IOS
		return SystemInfo.deviceUniqueIdentifier;
#endif
#if UNITY_EDITOR_OSX
		return SystemInfo.deviceUniqueIdentifier;
#endif
#if UNITY_STANDALONE_OSX
		return SystemInfo.deviceUniqueIdentifier;
#endif
		//IPGlobalProperties computerProperties = IPGlobalProperties.GetIPGlobalProperties();
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
		string data = json.ToString().Replace("\"", "\\\"");
		
		pendingData.Add(topic + "&" + data);
	}

	//
	public void Publish(string topTopic, string key, string value, string key2, string value2) {
		JSONClass json = new JSONClass();
		json.Add("time", GetEpoch().ToString());
		json.Add(key, value);
		json.Add(key2, value2);

		string topic = "gamedata/" + _macAddress + "/" + topTopic;
		string data = json.ToString().Replace("\"", "\\\"");

		pendingData.Add(topic + "&" + data);
	}

	//
	public void PublishInstant(string topTopic, string key, string value) {
		JSONClass json = new JSONClass();
		json.Add("time", GetEpoch().ToString());
		json.Add(key, value);

		string topic = "gamedata/" + _macAddress + "/" + topTopic;
		string data = json.ToString().Replace("\"", "\\\"");

		string[] sendData = (topic + "&" + data).Split('&');
		if(debug) { Debug.Log("topic: " + sendData[0] + ", payload: " + sendData[1]); }
		if(live) { _appServer.Publish(sendData[0], sendData[1]); }
	}

	//
	public void TestPublish() {
		string topic = "gamedata/" + _macAddress + "/event";
		JSONClass json = new JSONClass();

		Dictionary<string, string> data = new Dictionary<string, string>();
		data["timestamp"] = "" + GetEpoch();
		data["event"] = "\"app started\"";
		
		_appServer.Publish(topic, "{\\\"timestamp\\\":0,\\\"playtime\\\":1337}");
		Debug.Log("Test sent: " + "{\\\"timestamp\\\":0,\\\"playtime\\\":1337}");
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
