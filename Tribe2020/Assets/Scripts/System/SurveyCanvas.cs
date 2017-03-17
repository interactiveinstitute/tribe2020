using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SurveyCanvas : MonoBehaviour {
	private static SurveyCanvas _instance;
	public static SurveyCanvas GetInstance() {
		return _instance;
	}

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

	//
	public void Awake() {
		_questions = new List<SurveyQuestion>(GetQuestions());
		_instance = this;
	}

	// Use this for initialization
	void Start () {
		progress.Init(_questions.Count);
		progress.SetCurrent(0);

		_inPos = new Vector3(0, 0, 0);
		_outPos = new Vector3(1920, 0, 0);
	}
	
	// Update is called once per frame
	void Update () {
		for(int i = 0; i < _questions.Count; i++) {
			if(i == _curQuestion) {
				_questions[i].transform.position = _inPos + (_questions[i].transform.position - _inPos) * 0.95f;
			} else {
				_questions[i].transform.position = _outPos + (_questions[i].transform.position - _outPos) * 0.95f;
			}
		}
	}

	//
	public void ShowSummary(List<SurveyQuestion> answers) {
		for(int i = 0; i < answers.Count; i++) {
			if(answers[i].include) {
				GameObject row = Instantiate(summaryRowPrefab);
				row.transform.SetParent(summaryListContainer);
				row.GetComponentsInChildren<Text>()[0].text = answers[i].question;
				row.GetComponentsInChildren<Text>()[1].text = answers[i].answer;
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
