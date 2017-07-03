using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuizManager : MonoBehaviour {
	private static QuizManager _instance;
	public static QuizManager GetInstance() {
		return _instance;
	}

	//public Quiz[] quizzes;
	private AvatarQuizzes _curQuizGroup; 
	private int _curIndex = 0;
	private Quiz _curQuiz;

	//
	[System.Serializable]
	public struct AvatarQuizzes {
		public string avatarName;
		public List<Quiz> quizzes;
		public Minigame minigame;
	}

	[System.Serializable]
	public struct Minigame {
		public string name;
	}

	public List<AvatarQuizzes> avatarQuizzes;

	//
	void Awake() {
		_instance = this;
	}

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	//
	public AvatarQuizzes GetAvatarQuizzes(string avatarName) {
		foreach(AvatarQuizzes aq in avatarQuizzes) {
			if(aq.avatarName == avatarName) {
				return aq;
			}
		}
		return new AvatarQuizzes();
	}

	//
	public List<Quiz> GetAllQuizzez() {
		List<Quiz> allQuizzes = new List<Quiz>();

		foreach(AvatarQuizzes aq in avatarQuizzes) {
			foreach(Quiz q in aq.quizzes) {
				if(!allQuizzes.Contains(q)) {
					allQuizzes.Add(q);
				}
			}
		}

		return allQuizzes;
	}

	//
	public void InitQuizzes(string avatarName) {
		_curIndex = 0;
		foreach(AvatarQuizzes aq in avatarQuizzes) {
			if(aq.avatarName == avatarName) {
				_curQuizGroup = aq;
			}
		}
		_curQuiz = _curQuizGroup.quizzes[_curIndex];
	}

	//
	public Quiz GetCurrentQuiz() {
		return _curQuiz;
	}

	//
	public void Next() {
		_curIndex++;
		_curQuiz = _curQuizGroup.quizzes[_curIndex];
	}

	//
	public bool IsRightAnswer(int answerIndex) {
		return _curQuiz.rightChoice == answerIndex;
	}
}
