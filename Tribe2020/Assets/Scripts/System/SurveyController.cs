using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SimpleJSON;

public class SurveyController : MonoBehaviour {
	private static SurveyController _instance;
	public static SurveyController GetInstance() {
		return _instance;
	}

	private List<SurveyQuestion> _questions;
	private List<string> _answers;
	private int _curQuestion;

	private SaveManager _saveMgr;
	private CustomSceneManager _sceneMgr;
	private MonitorManager _monitorMgr;
	private LocalisationManager _localMgr;
	private SurveyCanvas _canvas;

	//Used before Start, initializes singleton instance. Dependencies cannot call GetInstance earlier than Start
	public void Awake() {
		_instance = this;
	}

	// Use this for initialization
	void Start() {
		_saveMgr = SaveManager.GetInstance();
		_sceneMgr = CustomSceneManager.GetInstance();
		_monitorMgr = MonitorManager.GetInstance();
		_localMgr = LocalisationManager.GetInstance();
		_canvas = SurveyCanvas.GetInstance();

		_monitorMgr.LoadSurvey(_canvas.GetQuestions());
		_questions = new List<SurveyQuestion>(_canvas.GetQuestions());

		_saveMgr.Load();
		_monitorMgr.DeserializeFromJSON(_saveMgr.GetClass("Monitor"));
		_localMgr.SetLanguage(_saveMgr.GetData("language"));
	}
	
	// Update is called once per frame
	void Update() {
		
	}

	//Get translation for phrase group and key
	public string GetPhrase(string group, string key) {
		string trans = _localMgr.GetPhrase(group, key);
		return _localMgr.GetPhrase(group, key);
	}

	//Get translateion for phrase group, key and index of value
	public string GetPhrase(string group, string key, int index) {
		return _localMgr.GetPhrase(group, key, index);
	}

	//
	public void OnAnswer(string answer) {
		_monitorMgr.AddAnswer(_curQuestion, _questions[_curQuestion].name, answer);
		Next();
	}	

	//
	public void OnAnswer(Text answer) {
		_monitorMgr.AddAnswer(_curQuestion, _questions[_curQuestion].name, answer.text);
		Next();
	}

	//
	public void OnAnswer(int answer) {
		_monitorMgr.AddAnswer(_curQuestion, _questions[_curQuestion].name, "" + answer);
		Next();
	}

	//
	public void Skip() {
		if(_questions[_curQuestion].type == SurveyQuestion.Type.Dropdown) {
			string answer = _questions[_curQuestion].GetComponentsInChildren<Text>()[1].text;
			_monitorMgr.AddAnswer(_curQuestion, _curQuestion + "", answer);
		} else {
			_monitorMgr.AddAnswer(_curQuestion, _curQuestion + "", "n/a");
		}
		
		Next();
	}

	//
	public void Next() {
		if(_curQuestion >= _questions.Count) {
			return;
		}

		_curQuestion++;
		_canvas.SetCurrentQuestion(_curQuestion);

		if(_curQuestion == _questions.Count - 1) {
			_canvas.ShowSummary(_monitorMgr.GetSurvey());
			_monitorMgr.CheckSurveyComplete();
		}
	}

	//
	public void Prev() {
		Debug.Log("Prev");
		if(_curQuestion == 0) {
			return;
		}

		_curQuestion--;
		_canvas.SetCurrentQuestion(_curQuestion);
	}

	//
	public void LoadScene(string scene) {
		_saveMgr.SetClass("Monitor", _monitorMgr.SerializeAsJSON());
		_saveMgr.Save();
		_sceneMgr.LoadScene(scene);
	}
}
