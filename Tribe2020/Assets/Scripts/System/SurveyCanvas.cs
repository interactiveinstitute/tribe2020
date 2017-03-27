using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SurveyCanvas : MonoBehaviour {
	private static SurveyCanvas _instance;
	public static SurveyCanvas GetInstance() {
		return _instance;
	}

	private SurveyController _controller;

	private List<SurveyQuestion> _questions;
	private int _curQuestion;

	public Button prevButton;
	public Button nextButton;
	public Button OKButton;
	public Color curColor;
	private Vector3 _inPos, _outPos;

	public Transform progressContainer;
	public SurveyProgress progress;
	private List<Image> _progressDots;

	public Transform summaryListContainer;
	public GameObject summaryRowPrefab;

	private bool _firstUpdate = true;

	//
	public void Awake() {
		_questions = new List<SurveyQuestion>(GetQuestions());
		_instance = this;
	}

	// Use this for initialization
	void Start() {
		_controller = SurveyController.GetInstance();

		progress.Init(_questions.Count);
		progress.SetCurrent(0);

		_inPos = new Vector3(0, 0, 0);
		_outPos = new Vector3(1920, 0, 0);
	}
	
	// Update is called once per frame
	void Update() {
		if(_firstUpdate) {
			Translate();
			_firstUpdate = false;
		}

		for(int i = 0; i < _questions.Count; i++) {
			if(i == _curQuestion) {
				_questions[i].transform.position = _inPos + (_questions[i].transform.position - _inPos) * 0.95f;
			} else {
				_questions[i].transform.position = _outPos + (_questions[i].transform.position - _outPos) * 0.95f;
			}
		}
	}

	//Translate all survey questions
	public void Translate() {
		foreach(SurveyQuestion sq in _questions) {
			//Translate main text
			if(sq.GetComponentInChildren<Text>()) {
				sq.GetComponentInChildren<Text>().text = _controller.GetPhrase("Survey", sq.name);
			}

			//Translate components depending on survey slide type
			switch(sq.type) {
				case SurveyQuestion.Type.Buttons:
					Button[] buttons = sq.GetComponentsInChildren<Button>();
					for(int i = 0; i < buttons.Length; i++) {
						buttons[i].GetComponentInChildren<Text>().text = _controller.GetPhrase("Survey", sq.name, i);
					}
					break;
				case SurveyQuestion.Type.Dropdown:
					Dropdown dropdown = sq.GetComponentInChildren<Dropdown>();
					dropdown.GetComponentInChildren<Text>().text = _controller.GetPhrase("Survey", sq.name, 0);
					for(int i = 0; i < dropdown.options.Count; i++) {
						dropdown.options[i].text = _controller.GetPhrase("Survey", sq.name, i);
					}
					break;
				case SurveyQuestion.Type.Grade:
					sq.GetComponentsInChildren<Text>()[1].text = _controller.GetPhrase("Survey", sq.name, 0);
					sq.GetComponentsInChildren<Text>()[3].text = _controller.GetPhrase("Survey", sq.name, 1);
					break;
				case SurveyQuestion.Type.Info:
					break;
				case SurveyQuestion.Type.Write:
					break;
				case SurveyQuestion.Type.Summary:
					break;
			}
		}
	}

	//
	public void ShowSummary(List<SurveyQuestion> answers) {
		for(int i = 0; i < answers.Count; i++) {
			if(answers[i].include) {
				GameObject row = Instantiate(summaryRowPrefab);
				row.transform.SetParent(summaryListContainer);
				row.GetComponentsInChildren<Text>()[0].text = _controller.GetPhrase("Survey", answers[i].name);
				//answers[i].question;
				switch(answers[i].type) {
					case SurveyQuestion.Type.Buttons:
						Button[] buttons = answers[i].GetComponentsInChildren<Button>();
						for(int b = 0; b < buttons.Length; b++) {
							if(buttons[b].GetComponentInChildren<Text>().text == answers[i].answer) {
								row.GetComponentsInChildren<Text>()[1].text = _controller.GetPhrase("Survey", answers[i].name, b);
							}
						}
						break;
					case SurveyQuestion.Type.Dropdown:
						row.GetComponentsInChildren<Text>()[1].text = answers[i].answer;
						break;
					default:
						row.GetComponentsInChildren<Text>()[1].text = answers[i].answer;
						break;
				}
			}
		}
	}

	//
	public SurveyQuestion[] GetQuestions() {
		return GetComponentsInChildren<SurveyQuestion>();
	}

	//
	public void SetCurrentQuestion(int index) {
		_curQuestion = index;
		progress.SetCurrent(index);

		if(_curQuestion == 0) {
			prevButton.interactable = false;
		} else {
			prevButton.interactable = true;
		}

		if(_curQuestion < _questions.Count - 1) {
			nextButton.gameObject.SetActive(true);
			OKButton.gameObject.SetActive(false);
		} else {
			nextButton.gameObject.SetActive(false);
			OKButton.gameObject.SetActive(true);
		}
	}
}
