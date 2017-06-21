using UnityEngine;
using UnityEngine.UI;
using SimpleJSON;
using System;

public class BattleController : MonoBehaviour, NarrationInterface, CameraInterface {
	//Singleton features
	private static BattleController _instance;
	public static BattleController GetInstance() {
		return _instance as BattleController;
	}

	//
	public enum InputState {
		ALL, ONLY_PROMPT, ONLY_SWIPE, ONLY_TAP, ONLY_APPLIANCE_SELECT, ONLY_APPLIANCE_DESELECT,
		ONLY_OPEN_INBOX, ONLY_CLOSE_INBOX, ONLY_ENERGY, ONLY_COMFORT, ONLY_SWITCH_LIGHT, ONLY_APPLY_EEM, ONLY_HARVEST, NOTHING,
		ONLY_CLOSE_MAIL, ONLY_SELECT_OVERVIEW, ONLY_SELECT_GRIDVIEW
	};

	private BattleView _view;
	private GameTime _timeMgr;
	private AudioManager _audioMgr;
	private NarrationManager _narrationMgr;
	private CustomSceneManager _sceneMgr;
	private SaveManager _saveMgr;
	private LocalisationManager _localMgr;
	private CameraManager _camMgr;
	private QuizManager _quizMgr;

	private bool _isTouching = false;

	public GameObject foeObject;
	public GameObject allyObject;

	private int foeCP = 100;
	private int allyCP = 100;

	public Quiz[] quizzes;
	private int _curQuizIndex = 0;
	private Quiz _curQuiz;
	private bool _hasWon = false;
	private bool _isLeveling = false;
	private bool _firstUpdate = false;

	[Header("Save between sessions")]
	public bool syncTime = true;
	public bool syncPilot = true;
	public bool syncCamera = true;
	public bool syncResources = true;
	public bool syncNarrative = true;
	public bool syncLocalization = true;
	public bool syncAvatars = true;
	public bool syncAppliances = true;

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
		_narrationMgr.SetInterface(this);

		_sceneMgr = CustomSceneManager.GetInstance();
		_saveMgr = SaveManager.GetInstance();
		_localMgr = LocalisationManager.GetInstance();

		_camMgr = CameraManager.GetInstance();
		_camMgr.SetInterface(this);

		_quizMgr = QuizManager.GetInstance();

		//LoadQuiz(quizzes[_curQuiz]);
	}
	
	// Update is called once per frame
	void Update () {
		if(!_firstUpdate) {
			_firstUpdate = true;
			_instance.LoadGameState();

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
	public void LoadOpponent(JSONNode json) {
		Appliance foeAppliance = foeObject.GetComponent<Appliance>();
		AvatarModel foeModel = foeObject.GetComponent<AvatarModel>();
		AvatarStats foeStats = foeObject.GetComponent<AvatarStats>();

		if(json != null) {
			foeAppliance.title = json["appliance"]["title"];
			foeModel.DeserializeFromJSON(json["avatarModel"]);
			foeStats.DeserializeFromJSON(json);

			_view.foeName.text = foeAppliance.title;
		}

		LoadQuiz(foeAppliance.title, _curQuizIndex);
	}

	//
	public void LoadQuiz(string avatarTitle, int quizIndex) {
		QuizManager.AvatarQuizzes aq = _quizMgr.GetAvatarQuizzes(avatarTitle);
		if(aq.avatarName != null) {
			_curQuiz = aq.quizzes[quizIndex];

			_view.question.text = _localMgr.GetPhrase("Quizzes", _curQuiz.name);
			_view.answers[0].text = _localMgr.GetPhrase("Quizzes", _curQuiz.name, 0);
			_view.answers[1].text = _localMgr.GetPhrase("Quizzes", _curQuiz.name, 1);
			_view.answers[2].text = _localMgr.GetPhrase("Quizzes", _curQuiz.name, 2);
			_view.answers[3].text = _localMgr.GetPhrase("Quizzes", _curQuiz.name, 3);
		} else {
			Debug.Log("no quizzes found for avatar " + avatarTitle);
		}
	}

	//
	public void OnArguePressed(int answerIndex) {
		if(answerIndex == _instance._curQuiz.rightChoice) {
			int damage = UnityEngine.Random.Range(10, 20);
			_instance._view.CreateFeedback(_instance.foeObject.transform.position, "" + damage);
			_instance.foeObject.GetComponent<AvatarMood>().SetMood(AvatarMood.Mood.tired);
			_instance.foeCP = Mathf.Max(_instance.foeCP - damage, 0);
			if(_instance.foeCP == 0) {
				_instance.OnWin();
			} else {
				_instance._curQuizIndex = (_instance._curQuizIndex + 1) % _instance.quizzes.Length;
				_instance.LoadQuiz(_instance.foeObject.GetComponent<Appliance>().title, _instance._curQuizIndex);
				//_instance.LoadQuiz(_instance.quizzes[_instance._curQuiz]);
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
		_view.ShowCongratulations("You have won the battle!");
		_isTouching = false;
		_hasWon = true;

		_camMgr.SetViewpoint("Victory");
		allyObject.GetComponent<AvatarMood>().SetMood(AvatarMood.Mood.euphoric);
		//allyObject.GetComponent<Animator>().Play("Sit");

		_view.dialogueUI.SetActive(false);
		_view.barsUI.SetActive(false);
		_view.actionsUI.SetActive(false);

		_narrationMgr.OnNarrativeEvent("BattleOver");
		//_narrationMgr.OnQuestEvent(Quest.QuestEvent.BattleOver);
		//_instance.SaveGameState();
	}

	//
	public void ShowMessage(string key, string message, bool showButton) {
	}

	//
	public void ClearView() {
		_view.ClearView();
	}

	//
	public void ControlInterface(string id, string action) {
		_view.ControlInterface(id, action);
	}

	//
	public void ShowCongratulations(string text) {
		_view.ShowCongratulations(text);
	}

	//
	public void PlaySound(string sound) {
		_audioMgr.PlaySound(sound);
	}

	//
	public void SetControlState(InputState state) {
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
	public void SetTimeScale(int timeScale) {
		_timeMgr.VisualTimeScale = timeScale;
	}

	//
	public string GetCurrentDate() {
		return _timeMgr.CurrentDate;
	}

	//
	public void SaveGameState() {
		if(syncNarrative) _saveMgr.SetCurrentSlotClass("NarrationManager", _narrationMgr.SerializeAsJSON());

		_saveMgr.SaveCurrentSlot();
	}

	//
	public void LoadGameState() {
		if(syncPilot) _saveMgr.LoadCurrentSlot();

		if(syncLocalization) _localMgr.SetLanguage(_saveMgr.GetData("language"));
		if(syncNarrative) _narrationMgr.DeserializeFromJSON(_saveMgr.GetCurrentSlotClass("NarrationManager"));
	}

	//
	public void OnAnimationEvent(string animationEvent) {
	}

	//
	public void OnNewViewpoint(Viewpoint curView, Viewpoint[][] viewMatrix, bool overview) {
	}

	//
	public void OnCameraArrived(Viewpoint viewpoint) {
	}

	//
	public void OnLevelUpOK() {
		JSONClass challengeData = new JSONClass();
		challengeData.Add("avatar", _instance.foeObject.GetComponent<Appliance>().title);
		_instance._saveMgr.SetClass("battleReport", challengeData);

		_instance.SaveGameState();
		_instance._sceneMgr.LoadScene(_instance._saveMgr.GetData(SaveManager.currentSlot, "curPilot"));
	}

	public void LimitInteraction(string limitation) {
		throw new NotImplementedException();
	}

	public void ShowMessage(string cmdJSON) {
		throw new NotImplementedException();
	}

	public void ShowPrompt(string cmdJSON) {
		throw new NotImplementedException();
	}

	public void ShowMessage(string key, string message, Sprite portrait, bool showButton) {
		throw new NotImplementedException();
	}

	public void CreateHarvest(string commandJSON) {
		throw new NotImplementedException();
	}

	public void ControlAvatar(string id, string action, Vector3 targetPosition) {
		throw new NotImplementedException();
	}

	public void ControlAvatar(string id, UnityEngine.Object action) {
		throw new NotImplementedException();
	}

	public void UnlockView(int x, int y) {
		throw new NotImplementedException();
	}

	public void SetSimulationTimeScale(float timeScale) {
		throw new NotImplementedException();
	}

	public void RequestCurrentView() {
		throw new NotImplementedException();
	}

	public void MoveCamera(string animation) {
		throw new NotImplementedException();
	}

	public void StopCamera() {
		throw new NotImplementedException();
	}

	public void OnNarrativeAction(Narrative narrative, Narrative.Step step, string callback, string[] parameters) {
		throw new NotImplementedException();
	}

	public void OnNarrativeCompleted(Narrative narrative) {
		throw new NotImplementedException();
	}

	public void OnNarrativeActivated(Narrative narrative) {
		throw new NotImplementedException();
	}

	public bool HasEventFired(Narrative narrative, Narrative.Step step) {
		throw new NotImplementedException();
	}

	public bool NarrativeCheck(string callback) {
		throw new NotImplementedException();
	}
}
