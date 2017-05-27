using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuizManager : MonoBehaviour {
	private static QuizManager _instance;
	public static QuizManager GetInstance() {
		return _instance;
	}

	//
	[System.Serializable]
	public struct AvatarQuizzes {
		public string avatarName;
		public List<Quiz> quizzes;
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
}
