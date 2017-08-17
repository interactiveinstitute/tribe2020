using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class BattleView : MonoBehaviour {
	//Singleton features
	private static BattleView _instance;
	public static BattleView GetInstance() {
		return _instance as BattleView;
	}

	private BattleController _controller;

	public GameObject RisingNumberPrefab;

	public GameObject dialogueUI;
	public GameObject barsUI;
	public GameObject actionsUI;
	public GameObject levelUpUI;

	public Text foeName;
	public ImageBar opponentEnergy;
	public ImageBar playerEnergy;
	public RectTransform lowerUI;
	//public Text foeCPNumber;
	//public Image foeCPBar;

	//public Text allyCPNumber;
	//public Image allyCPBar;

	public Text levelUpName;
	public Slider avatarSatisfaction;
	public Slider avatarKnowledge;
	public Slider avatarAttitude;
	public Slider avatarNorm;

	public GameObject minigameFrame;

	public Text question;
	public Text[] answers;

	public GameObject congratsPanel;
	public Text congratsText;

	public GameObject FeedbackNumber;
	public ParticleSystem fireworks;

	private bool _answerVisibility = true;

	//Sort use instead of constructor
	void Awake() {
		_instance = this;
	}

	// Use this for initialization
	void Start () {
		_controller = BattleController.GetInstance();
	}
	
	// Update is called once per frame
	void Update () {
		if(_answerVisibility) {
			lowerUI.position = new Vector2(lowerUI.position.x, 180);
		} else {
			lowerUI.position = new Vector2(lowerUI.position.x, -80);
		}
	}

	//
	public void SetAnswerVisible(bool visible) {
		_answerVisibility = visible;
	}

	//
	public void SetMinigameVisible(bool visible) {
		minigameFrame.SetActive(visible);
	}

	//
	public GameObject CreateFeedback(Vector3 pos, string feedback) {
		GameObject fb = Instantiate(RisingNumberPrefab, pos, Quaternion.identity) as GameObject;
		fb.GetComponent<TextMesh>().text = feedback;
		return fb;
	}

	//
	public void ShowCongratulations(string text) {
		ShowFireworks();
		congratsPanel.SetActive(true);
		congratsText.text = text;
	}

	//
	public void ShowFireworks() {
		_instance.fireworks.Play();
	}

	public void BuildQuiz(Quiz quiz) {
		ClearMinigame();
		question.text = _controller.GetPhrase("Quizzes", quiz.name);
		answers[0].text = _controller.GetPhrase("Quizzes", quiz.name, 0);
		answers[1].text = _controller.GetPhrase("Quizzes", quiz.name, 1);
		answers[2].text = _controller.GetPhrase("Quizzes", quiz.name, 2);
		answers[3].text = _controller.GetPhrase("Quizzes", quiz.name, 3);
		SetAnswerVisible(true);
		SetMinigameVisible(false);
	}

	//
	public void BuildBuildGame(Quiz quiz) {
		ClearMinigame();
		AddBuildBlock(quiz.goodImages[0], new Vector2(0, 0), new Vector2(70, 70));
		AddBuildBlock(quiz.goodImages[0], new Vector2(0, 0), new Vector2(70, 70));
		AddBuildBlock(quiz.goodImages[0], new Vector2(0, 0), new Vector2(70, 70));
		AddBuildBlock(quiz.goodImages[0], new Vector2(0, 0), new Vector2(70, 70));
		AddBuildBlock(quiz.goodImages[0], new Vector2(0, 0), new Vector2(70, 70));
		AddBuildBlock(quiz.badImages[0], new Vector2(0, 0), 45);

		AddGoalBlock(quiz.goodImages[0], new Vector2(0, 0), new Vector2(45, 45));
		SetAnswerVisible(false);
		SetMinigameVisible(true);
	}

	//
	public void BuildGainingGame(Quiz quiz) {
		ClearMinigame();
		AddArgument(quiz.goodImages[0], new Vector2(0, 0), true);
		AddArgument(quiz.badImages[0], new Vector2(0, 0), false);
		SetAnswerVisible(false);
		SetMinigameVisible(true);
	}

	//
	public void BuildFramingGame(Quiz quiz) {
		//ClearMinigame();
		//AddFramingSubject(quiz.goodImages[0], new Vector2(0, 0), true);
		//AddFramingSubject(quiz.badImages[0], new Vector2(0, 0), false);
		//SetAnswerVisible(false);
		//SetMinigameVisible(true);
	}

	//
	public void AddBuildBlock(Sprite img, Vector2 pos, int radius) {
		GameObject newBlock = new GameObject();
		newBlock.name = "Build Circle";
		newBlock.transform.SetParent(minigameFrame.transform, false);
		Image newImg = newBlock.AddComponent<Image>();
		newImg.sprite = img;
		Rigidbody2D newRB = newBlock.AddComponent<Rigidbody2D>();
		newRB.gravityScale = 100;
		CircleCollider2D newColl = newBlock.AddComponent<CircleCollider2D>();
		newColl.radius = radius;
		newBlock.AddComponent<Draggable>();

		//Instantiate(newBlock, minigameFrame.transform);
	}

	//
	public void AddBuildBlock(Sprite img, Vector2 pos, Vector2 size) {
		GameObject newBlock = new GameObject();
		newBlock.name = "Build Block";
		newBlock.transform.SetParent(minigameFrame.transform, false);
		Image newImg = newBlock.AddComponent<Image>();
		newImg.sprite = img;
		Rigidbody2D newRB = newBlock.AddComponent<Rigidbody2D>();
		newRB.gravityScale = 100;
		BoxCollider2D newColl = newBlock.AddComponent<BoxCollider2D>();
		newColl.size = size;
		newBlock.AddComponent<Draggable>();
		

		//Instantiate(newBlock, minigameFrame.transform);
	}

	//
	public void AddGoalBlock(Sprite img, Vector2 pos, Vector2 size) {
		GameObject newBlock = new GameObject();
		newBlock.name = "Build Goal";
		newBlock.transform.SetParent(minigameFrame.transform, false);
		Image newImg = newBlock.AddComponent<Image>();
		newImg.sprite = img;
		Rigidbody2D newRB = newBlock.AddComponent<Rigidbody2D>();
		newRB.gravityScale = 0;
		BoxCollider2D newColl = newBlock.AddComponent<BoxCollider2D>();
		newColl.size = size;
		newColl.isTrigger = true;
		newBlock.AddComponent<BuildGoal>();

		//Instantiate(newBlock, minigameFrame.transform);
	}

	//
	public void AddArgument(Sprite img, Vector2 pos, bool isPlayers) {
		GameObject newBlock = new GameObject();
		Image newImg = newBlock.AddComponent<Image>();
		newImg.sprite = img;
		Rigidbody2D newRB = newBlock.AddComponent<Rigidbody2D>();
		newRB.gravityScale = 100;
		BoxCollider2D newColl = newBlock.AddComponent<BoxCollider2D>();
		newColl.size = new Vector2(45, 45);

		Instantiate(newBlock, minigameFrame.transform);
	}

	//
	public void AddFramingSubject(Sprite img, Vector2 pos, bool isRelevant) {
		GameObject newBlock = new GameObject();
		Image newImg = newBlock.AddComponent<Image>();
		newImg.sprite = img;
		Rigidbody2D newRB = newBlock.AddComponent<Rigidbody2D>();
		newRB.gravityScale = 100;
		BoxCollider2D newColl = newBlock.AddComponent<BoxCollider2D>();
		newColl.size = new Vector2(45, 45);

		Instantiate(newBlock, minigameFrame.transform);
	}

	//
	public void ClearMinigame() {
		foreach(Transform t in minigameFrame.transform) {
			if(t.gameObject.name != "Bounds") {
				Destroy(t.gameObject);
			}
		}
	}

	//
	public void ClearView() {
	}

	//
	public void ControlInterface(string id, string action) {
	}
}
