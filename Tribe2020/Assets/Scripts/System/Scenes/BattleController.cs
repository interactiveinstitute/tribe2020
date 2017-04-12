using UnityEngine;
using UnityEngine.UI;
using SimpleJSON;
using System;

public class BattleController : Controller, CameraInterface {
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
	private CameraManager _camMgr;

	private bool _isTouching = false;

	public GameObject foeObject;
	public GameObject allyObject;

	public Transform thirdPersonTransform;
	public Transform victoryTransform;
	private Transform _targetTransform;

	private int foeCP = 100;
	private int foeEP = 100;

	private int allyCP = 100;
	private int allyEP = 100;

	public Quiz[] quizzes;
	private int _curQuiz = 0;
	private bool _hasWon = false;
	private bool _isLeveling = false;

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
		_camMgr = CameraManager.GetInstance();
		_camMgr.SetInterface(this);

		//_camMgr.SetViewpoint(0, 0, Vector2.zero);

		LoadQuiz(quizzes[_curQuiz]);

		_targetTransform = thirdPersonTransform;
	}
	
	// Update is called once per frame
	void Update () {
		if(!_isLoaded) {
			_isLoaded = true;
			LoadGameState();
			LoadOpponent(_saveMgr.GetData("pendingChallenge"));
		}

		_view.foeCPNumber.text = foeCP + "/100";
		_view.foeCPBar.fillAmount = foeCP / 100f;

		_view.allyCPNumber.text = allyCP + "/100";
		_view.allyCPBar.fillAmount = allyCP / 100f;

		_view.levelUpName.text = foeObject.GetComponent<Appliance>().title;
		_view.avatarKnowledge.value = foeObject.GetComponent<AvatarStats>().knowledge * 100;
		_view.avatarAttitude.value = foeObject.GetComponent<AvatarStats>().attitude * 100;
		_view.avatarNorm.value = foeObject.GetComponent<AvatarStats>().normSensititvity * 100;

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
			_camMgr.SetViewpoint("LevelUp");
			_view.levelUpUI.SetActive(true);
			_view.congratsPanel.SetActive(false);
			_isLeveling = true;
			//_sceneMgr.LoadScene(_saveMgr.GetData(SaveManager.currentSlot, "curPilot"));
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

		foeAppliance.title = json["appliance"]["title"];
		foeModel.DeserializeFromJSON(json["avatarModel"]);
		foeStats.DeserializeFromJSON(json);

		_view.foeName.text = foeAppliance.title;
	}

	//
	public void OnArguePressed(int answerIndex) {
		if(answerIndex == quizzes[_curQuiz].rightChoice) {
			int damage = UnityEngine.Random.Range(10, 20);
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

		_camMgr.SetViewpoint("Victory");
		allyObject.GetComponent<AvatarMood>().SetMood(AvatarMood.Mood.euphoric);
		allyObject.GetComponent<Animator>().Play("Sit");

		_view.dialogueUI.SetActive(false);
		_view.barsUI.SetActive(false);
		_view.actionsUI.SetActive(false);

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

	//
	public void OnAnimationEvent(string animationEvent) {
	}

	//
	public void OnCameraArrived(Viewpoint viewpoint) {
	}

	//
	public void OnLevelUpOK() {
		_sceneMgr.LoadScene(_saveMgr.GetData(SaveManager.currentSlot, "curPilot"));
	}
}
