using UnityEngine;
using System.Collections.Generic;
using SimpleJSON;
using System.Net;
using System.Text;
using WebSocketSharp.Net;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;

public class LocalisationManager : MonoBehaviour {
	//Singleton features
	private static LocalisationManager _instance;
	public static LocalisationManager GetInstance() {
		return _instance;
	}

	public bool debug = false;

	//
	public List<Language> languages;
	public Language curLanguage;

	public Language template;

	public string APIKey;

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
	public void SetLanguage(string language) {
		foreach(Language lang in languages) {
			if(lang.name.Equals(language)) {
				curLanguage = lang;
			}
		}
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

	//
	public string GetPhrase(string group, string key, int index) {
		foreach(Language.ValueGroup g in curLanguage.groups) {
			if(g.title == group) {
				foreach(Language.KeyValue keyValue in g.values) {
					if(key == keyValue.key) {
						return keyValue.values[index];
					}
				}
			}
		}
		return "";
	}

	//
	public JSONClass SerializeAsJSON() {
		JSONClass json = new JSONClass();

		json.Add("language", curLanguage.ToString());

		return json;
	}

	//
	public void DeserializeFromJSON(JSONClass json) {
		if(json != null) {
			string langString = json["language"];
			foreach(Language language in languages) {
				if(language.ToString() == langString) {
					curLanguage = language;
				}
			}
		}
	}

	//
	public string Translate(string text, string from, string to) {
		string url = 
			"https://www.googleapis.com/language/translate/v2?key=" + APIKey +
			"&q=" + HttpUtility.UrlEncode(text) +
			"&source=" + from.ToLower() +
			"&target=" + to.ToLower();

		string html = null;
		WebClient web = new WebClient();

		// MUST add a known browser user agent or else response encoding doen't return UTF-8 (WTF Google?)
		web.Headers.Add(HttpRequestHeader.UserAgent, "Mozilla/5.0");
		web.Headers.Add(HttpRequestHeader.AcceptCharset, "UTF-8");

		// Make sure we have response encoding to UTF-8
		web.Encoding = Encoding.UTF8;

		Instate();

		html = web.DownloadString(url);
		html = HttpUtility.HtmlDecode(html);

		JSONNode json = JSON.Parse(html);
		string result = json["data"]["translations"][0]["translatedText"];

		Debug.Log(from + ": " + text + " -> " + to + ": " + result);

		return result;
	}

	//Just accept and move on
	public static bool Validator(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors policyErrors) {
		return true;
	}

	//Call before doing a web call
	public static void Instate() {
		ServicePointManager.ServerCertificateValidationCallback = Validator;
	}
}
