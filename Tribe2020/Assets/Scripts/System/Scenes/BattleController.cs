using UnityEngine;
using UnityEngine.UI;
using SimpleJSON;

public class BattleController : Controller {
	//Singleton features
	public static BattleController GetInstance() {
		return _instance as BattleController;
	}

	private BattleView _view;
	private GameTime _timeMgr;
	private AudioManager _audioMgr;
	private NarrationManager _narrationMgr;
	private CustomSceneManager _sceneMgr;
	private SaveManager _saveMgr;
	private LocalisationManager _localMgr;

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
		_timeMgr = GameTime.GetInstance();
		_audioMgr = AudioManager.GetInstance();
		_narrationMgr = NarrationManager.GetInstance();
		_sceneMgr = CustomSceneManager.GetInstance();
		_saveMgr = SaveManager.GetInstance();
		_localMgr = LocalisationManager.GetInstance();

		LoadQuiz(quizzes[_curQuiz]);
	}
	
	// Update is called once per frame
	void Update () {
		if(!_isLoaded) {
			_isLoaded = true;
			LoadGameState();

			Debug.Log("app: "   + _saveMgr.GetData("pendingChallenge")["appliance"]);
			Debug.Log("title: " + _saveMgr.GetData("pendingChallenge")["appliance"]["title"]);

			LoadOpponent(_saveMgr.GetData("pendingChallenge"));
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
		//_saveMgr.Save(SaveManager.currentSlot);
	}

	//
	private void OnTouchStart(Vector3 pos) {
		_isTouching = true;
	}

	//
	private void OnTouchEnded(Vector3 pos) {
		if(_isTouching && _hasWon) {
			_sceneMgr.LoadScene(_saveMgr.GetData(SaveManager.currentSlot, "curPilot"));
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
	public void LoadOpponent(JSONNode json) {
		Appliance foeAppliance = foeObject.GetComponent<Appliance>();
		AvatarModel foeModel = foeObject.GetComponent<AvatarModel>();
		AvatarStats foeStats = foeObject.GetComponent<AvatarStats>();

		foeStats.DeserializeFromJSON(json);
		foeAppliance.title = json["appliance"]["title"];
		foeModel.DeserializeFromJSON(json["avatarModel"]);

		_view.foeName.text = foeAppliance.title;
	}

	//
	public void OnArguePressed(int answerIndex) {
		if(answerIndex == quizzes[_curQuiz].rightChoice) {
			int damage = Random.Range(10, 20);
			_view.CreateFeedback(foeObject.transform.position, "" + damage);
			foeObject.GetComponent<AvatarMood>().SetMood(AvatarMood.Mood.tired);
			foeCP = Mathf.Max(foeCP - damage, 0);
			if(foeCP == 0) {
				OnWin();
			} else {
				_curQuiz = (_curQuiz + 1) % quizzes.Length;
				LoadQuiz(quizzes[_curQuiz]);
			}
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

		_narrationMgr.OnNarrativeEvent("BattleOver");
		//_narrationMgr.OnQuestEvent(Quest.QuestEvent.BattleOver);
		SaveGameState();
	}

	//
	public void ShowMessage(string key, string message, bool showButton) {
	}

	//
	public override void ClearView() {
		_view.ClearView();
	}

	//
	public override void ControlInterface(string id, string action) {
		_view.ControlInterface(id, action);
	}

	//
	public void ShowCongratualations(string text) {
		_view.ShowCongratualations(text);
	}

	//
	public override void PlaySound(string sound) {
		_audioMgr.PlaySound(sound);
	}

	//
	public override void SetControlState(InputState state) {
		//_curState = state;
	}

	//
	public string GetPhrase(string groupKey) {
		return _localMgr.GetPhrase(groupKey);
	}

	//
	public string GetPhrase(string groupKey, string key) {
		return _localMgr.GetPhrase(groupKey, key);
	}

	//
	public override void SetTimeScale(int timeScale) {
		_timeMgr.VisualTimeScale = timeScale;
	}

	//
	public override string GetCurrentDate() {
		return _timeMgr.CurrentDate;
	}

	//
	public override void SaveGameState() {
		_saveMgr.SetCurrentSlotClass("NarrationManager", _narrationMgr.SerializeAsJSON());
		_saveMgr.SaveCurrentSlot();
	}

	//
	public override void LoadGameState() {
		_saveMgr.LoadCurrentSlot();
		_narrationMgr.DeserializeFromJSON(_saveMgr.GetCurrentSlotClass("NarrationManager"));
	}
}
