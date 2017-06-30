using UnityEngine;
using UnityEngine.UI;
using SimpleJSON;
using System;

public class BattleController : MonoBehaviour, NarrationInterface, CameraInterface, InteractionListener {
	//Singleton features
	private static BattleController _instance;
	public static BattleController GetInstance() {
		return _instance as BattleController;
	}

	public enum GameState { Quizzing, Damage, Framing, Building, Gaining, Presenting, Defeat, Victory, Leveling };
	private GameState state = GameState.Quizzing;
	private float _stateTimer = 0;

	private BattleView _view;
	private GameTime _timeMgr;
	private AudioManager _audioMgr;
	private NarrationManager _narrationMgr;
	private CustomSceneManager _sceneMgr;
	private SaveManager _saveMgr;
	private LocalisationManager _localMgr;
	private CameraManager _camMgr;
	private QuizManager _quizMgr;
	private InteractionManager _interMgr;

	//private bool _isTouching = false;

	public GameObject foeObject;
	public GameObject allyObject;

	private int opponentEnergy = 4;
	private int playerEnergy = 3;

	private bool _firstUpdate = false;
	private int _pendingAnswer;

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

		_interMgr = InteractionManager.GetInstance();
		_interMgr.SetListener(this);

		_view.opponentEnergy.maxValue = opponentEnergy;
		_view.playerEnergy.maxValue = playerEnergy;
	}
	
	// Update is called once per frame
	void Update () {
		if(!_firstUpdate) {
			_firstUpdate = true;
			_instance.LoadGameState();

			LoadOpponent(_saveMgr.GetData("pendingChallenge"));
			SetState(GameState.Quizzing);
		}

		_view.opponentEnergy.value = opponentEnergy;
		_view.playerEnergy.value = playerEnergy;

		_view.levelUpName.text = foeObject.GetComponent<Appliance>().title;
		_view.avatarKnowledge.value = foeObject.GetComponent<AvatarStats>().knowledge * 100;
		_view.avatarAttitude.value = foeObject.GetComponent<AvatarStats>().attitude * 100;
		_view.avatarNorm.value = foeObject.GetComponent<AvatarStats>().normSensititvity * 100;

		if(state == GameState.Damage){
			if(_stateTimer >= 1.5f && _stateTimer < 2) {
				ResolveDamage();
				_stateTimer = 2;
			} else if(_stateTimer >= 3) {
				SetState(GameState.Quizzing);
			}
		}
		_stateTimer += Time.deltaTime;
	}

	//
	public void OnTap(Vector3 position) {
		if(state == GameState.Victory) {
			SetState(GameState.Leveling);
		}
	}

	//
	public void OnSwipe(Vector2 direction) {
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

		_quizMgr.InitQuizzes(foeAppliance.title);
		RefreshQuiz();
	}

	//
	public void SetState(GameState nextState) {
		switch(nextState) {
			case GameState.Quizzing:
				_camMgr.SetViewpoint("Battle");
				_view.SetAnswerVisible(true);
				break;
			case GameState.Damage:
				_camMgr.SetViewpoint("LevelUp");
				_view.SetAnswerVisible(false);
				break;
			case GameState.Building:
				_view.SetAnswerVisible(false);
				break;
			case GameState.Framing:
				_view.SetAnswerVisible(false);
				break;
			case GameState.Gaining:
				_view.SetAnswerVisible(false);
				break;
			case GameState.Presenting:
				_view.SetAnswerVisible(false);
				break;
			case GameState.Defeat:
				foeObject.GetComponent<AvatarMood>().SetMood(AvatarMood.Mood.happy);

				_view.dialogueUI.SetActive(false);
				_view.barsUI.SetActive(false);
				_view.actionsUI.SetActive(false);

				_sceneMgr.LoadScene(_saveMgr.GetData(SaveManager.currentSlot, "curPilot"));
				break;
			case GameState.Victory:
				_view.ShowCongratulations(_localMgr.GetPhrase("Interface", "You have won the battle!"));
				_camMgr.SetViewpoint("Victory");
				allyObject.GetComponent<AvatarMood>().SetMood(AvatarMood.Mood.euphoric);

				_view.dialogueUI.SetActive(false);
				_view.barsUI.SetActive(false);
				_view.actionsUI.SetActive(false);

				_narrationMgr.OnNarrativeEvent("BattleOver");
				_interMgr.ResetTouch();
				break;
			case GameState.Leveling:
				_camMgr.SetViewpoint("LevelUp");
				_view.levelUpUI.SetActive(true);
				_view.congratsPanel.SetActive(false);
				foeObject.GetComponent<AvatarMood>().SetMood(AvatarMood.Mood.happy);
				break;
		}
		state = nextState;
		_stateTimer = 0;
	}

	//
	public void RefreshQuiz() {
		Quiz curQuiz = _quizMgr.GetCurrentQuiz();
		_view.question.text = _localMgr.GetPhrase("Quizzes", curQuiz.name);
		_view.answers[0].text = _localMgr.GetPhrase("Quizzes", curQuiz.name, 0);
		_view.answers[1].text = _localMgr.GetPhrase("Quizzes", curQuiz.name, 1);
		_view.answers[2].text = _localMgr.GetPhrase("Quizzes", curQuiz.name, 2);
		_view.answers[3].text = _localMgr.GetPhrase("Quizzes", curQuiz.name, 3);
	}

	//
	public void OnArguePressed(int answerIndex) {
		_instance._pendingAnswer = answerIndex;
		_instance.SetState(GameState.Damage);
	}

	//
	public void OnLevelUpOK() {
		JSONClass challengeData = new JSONClass();
		challengeData.Add("avatar", _instance.foeObject.GetComponent<Appliance>().title);
		_instance._saveMgr.SetClass("battleReport", challengeData);

		_instance.SaveGameState();
		_instance._sceneMgr.LoadScene(_instance._saveMgr.GetData(SaveManager.currentSlot, "curPilot"));
	}

	//
	public void ResolveDamage() {
		//Right answer -> deal damage to opponent or wrong answer -> receive damage
		if(_quizMgr.IsRightAnswer(_pendingAnswer)) {
			int damage = 1;
			_view.CreateFeedback(foeObject.transform.position, "" + damage);
			int moodShuffle = UnityEngine.Random.Range(0, 3);
			if(moodShuffle == 0) {
				foeObject.GetComponent<AvatarMood>().SetMood(AvatarMood.Mood.tired);
			} else if(moodShuffle == 1) {
				foeObject.GetComponent<AvatarMood>().SetMood(AvatarMood.Mood.angry);
			} else {
				foeObject.GetComponent<AvatarMood>().SetMood(AvatarMood.Mood.surprised);
			}
			opponentEnergy--;
			if(opponentEnergy == 0) {
				SetState(GameState.Victory);
			} else {
				_quizMgr.Next();
				RefreshQuiz();
			}
		} else {
			playerEnergy--;
			if(playerEnergy == 0) {
				SetState(GameState.Defeat);
			}
		}
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
	public string GetPhrase(string groupKey) {
		return _localMgr.GetPhrase(groupKey);
	}

	//
	public string GetPhrase(string groupKey, string key) {
		return _localMgr.GetPhrase(groupKey, key);
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
