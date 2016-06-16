using UnityEngine;

public class BattleController : Controller {
	//Singleton features
	private static BattleController _instance;
	public static BattleController GetInstance() {
		return _instance;
	}

	private BattleView _view;
	private CustomSceneManager _sceneMgr;
	private NarrationManager _narrationMgr;
	private SaveManager _saveMgr;

	private bool _isTouching = false;

	public GameObject foeObject;
	public GameObject allyObject;

	private int foeCP = 100;
	private int foeEP = 100;

	private int allyCP = 100;
	private int allyEP = 100;

	public Quiz[] quizzes;
	private int _curQuiz = 0;
	private bool _hasWon = false;

	private bool _isLoaded = false;

	//Sort use instead of constructor
	void Awake() {
		_instance = this;
	}

	// Use this for initialization
	void Start () {
		_view = BattleView.GetInstance();
		_sceneMgr = CustomSceneManager.GetInstance();
		_narrationMgr = NarrationManager.GetInstance();
		_saveMgr = SaveManager.GetInstance();

		LoadQuiz(quizzes[_curQuiz]);
	}
	
	// Update is called once per frame
	void Update () {
		if(!_isLoaded) {
			_isLoaded = true;
			_saveMgr.Load();
		}

		_view.foeCPNumber.text = foeCP + "/100";
		_view.foeEPNumber.text = foeEP + "/100";
		_view.foeCPBar.fillAmount = foeCP / 100f;
		_view.foeEPBar.fillAmount = foeEP / 100f;

		_view.allyCPNumber.text = allyCP + "/100";
		_view.allyEPNumber.text = allyEP + "/100";
		_view.allyCPBar.fillAmount = allyCP / 100f;
		_view.allyEPBar.fillAmount = allyEP / 100f;

		//Touch Events
		if(Input.GetMouseButtonDown(0)) { OnTouchStart(Input.mousePosition); }
		if(Input.GetMouseButtonUp(0)) { OnTouchEnded(Input.mousePosition); }
	}

	//
	void OnDestroy() {
		_saveMgr.Save();
	}

	//
	private void OnTouchStart(Vector3 pos) {
		_isTouching = true;
	}

	//
	private void OnTouchEnded(Vector3 pos) {
		if(_isTouching && _hasWon) {
			_sceneMgr.LoadScene("ga_madrid_erik");
		}
	}

	//
	public void LoadQuiz(Quiz quiz) {
		_view.question.text = quiz.question;
		_view.answers[0].text = quiz.options[0];
		_view.answers[1].text = quiz.options[1];
		_view.answers[2].text = quiz.options[2];
		_view.answers[3].text = quiz.options[3];
	}

	//
	public void OnArguePressed(int answerIndex) {
		if(answerIndex == quizzes[_curQuiz].rightChoice) {
			int damage = Random.Range(10, 20);
			_view.CreateFeedback(foeObject.transform.position, "" + damage);
			foeCP = Mathf.Max(foeCP - damage, 0);
			if(foeCP == 0) {
				OnWin();
			}

			_curQuiz = (_curQuiz + 1) % quizzes.Length;
			LoadQuiz(quizzes[_curQuiz]);
		}
	}

	//
	public void OnItemPressed() {
	}

	//
	public void OnSurrenderPressed() {
	}

	//
	public void OnWin() {
		_view.ShowCongratualations("You have won the battle!");
		_isTouching = false;
		_hasWon = true;

		_narrationMgr.OnQuestEvent(Quest.QuestEvent.BattleOver);
		//_sceneMgr.LoadScene("ga_madrid_erik");
	}
}
