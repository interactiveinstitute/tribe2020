using UnityEngine;
using UnityEngine.UI;
using SimpleJSON;
using System;

public class BattleController : MonoBehaviour, CameraInterface {
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

	private bool _isTouching = false;

	public GameObject foeObject;
	public GameObject allyObject;

	private int foeCP = 100;
	private int allyCP = 100;

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

		LoadQuiz(quizzes[_curQuiz]);
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
		if(answerIndex == quizzes[_instance._curQuiz].rightChoice) {
			int damage = UnityEngine.Random.Range(10, 20);
			_instance._view.CreateFeedback(_instance.foeObject.transform.position, "" + damage);
			_instance.foeObject.GetComponent<AvatarMood>().SetMood(AvatarMood.Mood.tired);
			_instance.foeCP = Mathf.Max(_instance.foeCP - damage, 0);
			if(_instance.foeCP == 0) {
				_instance.OnWin();
			} else {
				_instance._curQuiz = (_instance._curQuiz + 1) % _instance.quizzes.Length;
				_instance.LoadQuiz(_instance.quizzes[_instance._curQuiz]);
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
	public void ClearView() {
		_view.ClearView();
	}

	//
	public void ControlInterface(string id, string action) {
		_view.ControlInterface(id, action);
	}

	//
	public void ShowCongratualations(string text) {
		_view.ShowCongratualations(text);
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
		_saveMgr.SetCurrentSlotClass("NarrationManager", _narrationMgr.SerializeAsJSON());
		_saveMgr.SaveCurrentSlot();
	}

	//
	public void LoadGameState() {
		_saveMgr.LoadCurrentSlot();
		_narrationMgr.DeserializeFromJSON(_saveMgr.GetCurrentSlotClass("NarrationManager"));
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
		_instance._sceneMgr.LoadScene(_instance._saveMgr.GetData(SaveManager.currentSlot, "curPilot"));
	}
}
