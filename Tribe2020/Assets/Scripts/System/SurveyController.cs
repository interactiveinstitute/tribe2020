using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SurveyController : MonoBehaviour {
	public List<SurveyQuestion> questions;
	private int _curQuestion;
	private Vector3 _inPos, _outPos;

	// Use this for initialization
	void Start () {
		_inPos = new Vector3(640, 400, 0);
		_outPos = new Vector3(1920, 400, 0);
	}
	
	// Update is called once per frame
	void Update () {
		for(int i = 0; i < questions.Count; i++) {
			if(i == _curQuestion) {
				questions[i].transform.position = _inPos + (questions[i].transform.position - _inPos) * 0.95f;
			} else {
				questions[i].transform.position = _outPos + (questions[i].transform.position - _outPos) * 0.95f;
			}
		}
	}

	//
	public void OnAnswer(string answer) {
		Debug.Log(answer);
		_curQuestion++;
	}

	//
	public void OnAnswer(Text answer) {
		Debug.Log(answer.text);
		_curQuestion++;
	}

	//
	public void OnAnswer(int answer) {
		Debug.Log(answer);
		_curQuestion++;
	}
}
