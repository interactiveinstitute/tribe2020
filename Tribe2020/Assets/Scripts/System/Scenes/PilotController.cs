using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using SimpleJSON;

public class PilotController : Controller {
	//Singleton features
	public static PilotController GetInstance(){
		return _instance as PilotController;
	}
	private InputState _curState = InputState.ALL;

	#region Fields
	public bool debug = false;
    public bool enableSaveLoad = true;

    //Access all singleton systemss
    private PilotView _view;
	private GameTime _timeMgr;
	private CameraManager _camMgr;
	private AudioManager _audioMgr;
	private MainMeter _mainMeter;
	private ResourceManager _resourceMgr;
	private NarrationManager _narrationMgr;
	private CustomSceneManager _sceneMgr;
	private SaveManager _saveMgr;
	private LocalisationManager _localMgr;

	//Interaction props
	private string _touchState = IDLE;
	private float _touchTimer = 0;
	private float _doubleTimer = 0;
	private Vector3 _startPos;
	private bool _isPinching = false;
	private bool _touchReset = false;

	//Interaction consts
	private const string IDLE = "idle";
	private const string TAP = "tap";
	public const float TAP_TIMEOUT = 0.1f;
	public const float D_TAP_TIMEOUT = 0.2f;
	public const float SWIPE_THRESH = 50;

	private List<BehaviourAI> _avatars;
	private List<Appliance> _appliances;

	private bool _isLoaded = false;
    #endregion

    //Sort use instead of constructor
    void Awake(){
		_instance = this;
	}

	//
	public void SetToInstance(PilotController oldInstance) {

	}

	// Use this for initialization
	void Start(){
		_view = PilotView.GetInstance();
		_timeMgr = GameTime.GetInstance();
		_camMgr = CameraManager.GetInstance();
		_audioMgr = AudioManager.GetInstance();
		_mainMeter = MainMeter.GetInstance();
		_resourceMgr = ResourceManager.GetInstance();
		_narrationMgr = NarrationManager.GetInstance();
		_sceneMgr = CustomSceneManager.GetInstance();
		_saveMgr = SaveManager.GetInstance();
		_localMgr = LocalisationManager.GetInstance();

		_avatars = new List<BehaviourAI>(Object.FindObjectsOfType<BehaviourAI>());
		_appliances = new List<Appliance>(Object.FindObjectsOfType<Appliance>());
	}
	
	// Update is called once per frame
	void Update(){
		if(!_isLoaded) {
			_isLoaded = true;
			LoadGameState();
		}

		//Mobile interaction
		//		UpdatePan(_camMgr.camera);
		UpdatePinch();

		//		if(!InspectorUI.activeSelf){
		//if(IsOutsideUI(Input.mousePosition) && !_touchReset) {
		if(!_touchReset) {
			//Touch start
			if(Input.GetMouseButtonDown(0)) {
				OnTouchStart(Input.mousePosition);
			}

			//Touch ongoing
			if(Input.GetMouseButton(0)) {
				OnTouch(Input.mousePosition);
			}

			//Touch end
			if(Input.GetMouseButtonUp(0)) {
				OnTouchEnded(Input.mousePosition);
			}

			if(_touchState == TAP) {
				_doubleTimer += Time.unscaledDeltaTime;
				if(_doubleTimer > D_TAP_TIMEOUT) {
					OnTap(_startPos);
					_doubleTimer = 0;
					_touchState = IDLE;
				}
			}
		} else {
			_touchReset = false;
		}

		_view.date.GetComponent<Text>().text = _timeMgr.CurrentDate;
		_view.power.GetComponent<Text>().text = Mathf.Floor(_mainMeter.Power) + " W";
		float energy = (float)_mainMeter.Energy;
		if(energy < 1) {
			_view.energyCounter.text = Mathf.Floor(energy * 1000) + " Wh";
		} else {
			_view.energyCounter.text = Mathf.Floor(energy) + " kWh";
		}

		_view.cash.GetComponent<Text>().text = _resourceMgr.cash.ToString();
		_view.comfort.GetComponent<Text>().text = _resourceMgr.comfort.ToString();
		_view.UpdateQuestCount(_narrationMgr.GetQuests().Count);
	}

	//
	private void OnTouchStart(Vector3 pos){
		//Debug.Log("OnTouchStart");
		_touchTimer = 0;
		if(_touchState == IDLE) {
			_startPos = pos;
		}
	}
	
	//
	private void OnTouch(Vector3 pos){
		//_camMgr.cameraState = CameraManager.PANNED;
		_touchTimer += Time.unscaledDeltaTime;

		//if(Application.platform == RuntimePlatform.Android){
		//	_camMgr.UpdatePan(Input.GetTouch(0).deltaPosition);
		//}
	}
	
	//
	private void OnTouchEnded(Vector3 pos){
		//Debug.Log("ontouchended");
		_camMgr.cameraState = CameraManager.CameraState.Idle;
		float dist = Vector3.Distance(_startPos, pos);

		//Touch ended before tap timeout, trigger OnTap
		if(!_touchReset) {
			if(_touchTimer < TAP_TIMEOUT && dist < SWIPE_THRESH) {
				_touchTimer = 0;
				if(_touchState == IDLE) {
					_touchState = TAP;
				} else if(_touchState == TAP) {
					_touchState = IDLE;
					_doubleTimer = 0;
					OnDoubleTap(pos);
				}
			} else if(dist >= SWIPE_THRESH) {
				//Debug.Log("swipe");
				OnSwipe(_startPos, pos);
			}
		}
	}
	
	//
	private void OnTap(Vector3 pos){
		if(_curState != InputState.ALL && _curState != InputState.ONLY_TAP) { return; }

		_narrationMgr.OnQuestEvent(Quest.QuestEvent.Tapped);
	}

	//
	private void OnDoubleTap(Vector3 pos){
		//Debug.Log("Double tapped at " + pos);
	}

	//
	private void OnSwipe(Vector3 start, Vector3 end){
		//Debug.Log("cotroller.OnSwipe " + start + " , " + end);
		if(_view.IsAnyOverlayActive()) {
			return;
		}

		if(_curState == InputState.ALL || _curState == InputState.ONLY_SWIPE) {
			float dir = Mathf.Atan2(end.y - start.y, end.x - start.x);
			dir = (dir * Mathf.Rad2Deg + 360) % 360;
			float dist = Vector3.Distance(start, end);

			float dirMod = (dir + 90) % 360;
			if(dirMod > 45 && dirMod <= 135) {
				_camMgr.GotoLeftView();
			} else if(dir > 45 && dir <= 135) {
				_camMgr.GotoLowerView();
			} else if(dir > 135 && dir <= 225) {
				_camMgr.GotoRightView();
			} else if(dir > 225 && dir <= 315) {
				_camMgr.GotoUpperView();
			}

			_narrationMgr.OnQuestEvent(Quest.QuestEvent.Swiped);
			_narrationMgr.OnQuestEvent(Quest.QuestEvent.FindView, _camMgr.GetViewPoint().title);
		}
	}

	//
	public void OnPinchIn() {
		if(_curState == InputState.ALL) {
			_camMgr.GotoUpperView();
		}
	}

	//
	public void OnPinchOut() {
		if(_curState == InputState.ALL) {
			_camMgr.GotoLowerView();
		}
	}

	//
	public void OnPinching(float magnitude) {
	}

	//
	public void ResetTouch(){
		_touchTimer = 0;
		_doubleTimer = 0;
		_startPos = Input.mousePosition;
		_touchState = IDLE;
		_touchReset = true;
		//Debug.Log("cotroller.resetSwipe " + _startPos);
	}

	//
	public override void ControlAvatar(string id, string action, Vector3 pos) {
		foreach(BehaviourAI avatar in _avatars) {
			if(avatar.name == id) {
				avatar.TakeControlOfAvatar();
				avatar.WalkTo(pos);
			}
		}
	}

	//
	public override void ControlAvatar(string id, Object action) {
		foreach(BehaviourAI avatar in _avatars) {
			if(avatar.name == id) {
				AvatarActivity newAct = UnityEngine.ScriptableObject.Instantiate<AvatarActivity>(action as AvatarActivity);
				avatar.StartTemporaryActivity(newAct);
			}
		}
	}

	//
	public void OnOkPressed() {
		ResetTouch();

		if(_curState == InputState.ALL || _curState == InputState.ONLY_PROMPT) {
			_view.messageUI.SetActive(false);

			_narrationMgr.OnQuestEvent(Quest.QuestEvent.OKPressed);
		}

		PlaySound("Press Button");
	}

	//
	public void ToggleMenu() {
		_view.ToggleMenu();
	}

	//Open inspector with details of appliance
	public void SetCurrentUI(Appliance app) {
		if(_curState != InputState.ALL && _curState != InputState.ONLY_APPLIANCE_SELECT) { return; }

		string title = _localMgr.GetPhrase("Appliance:" + app.title + "_Title");
		string description = _localMgr.GetPhrase("Appliance:" + app.title + "_Description");
		_view.BuildInspector(title, description, app);
		SetCurrentUI(_view.inspector);

		//_view.BuildInspector(appliance);
		//SetCurrentUI(_view.inspector);

		//_narrationMgr.OnQuestEvent(Quest.QuestEvent.InspectorOpened);
		_narrationMgr.OnQuestEvent(Quest.QuestEvent.InspectorOpened, app.title);
		if(app.GetComponent<BehaviourAI>()) {
			_narrationMgr.OnQuestEvent(Quest.QuestEvent.AvatarSelected, app.title);
		}
	}

	//Open mail with details of narrative
	public void SetCurrentUI(Quest quest) {
		if(debug) { Debug.Log(name + ": SetCurrentUI(" + quest.name + ")"); }
		if(_curState != InputState.ALL && _curState != InputState.ONLY_OPEN_QUEST) { return; }

		_view.BuildMail(quest);
		SetCurrentUI(_view.mail);

		//_narrationMgr.OnQuestEvent(Quest.QuestEvent.MailOpened);
	}

	//
	public override void ShowMessage(string key, string message, bool showButton) {
		if(debug) { Debug.Log(name + ": ShowMessage(" + key + ", " + message + ")"); }
		string msg = _localMgr.GetPhrase(key);
		if(msg == "") { msg = message + "!"; }

		_view.ShowMessage(msg, true, showButton);
	}

	//Open user interface
	public void SetCurrentUI(RectTransform ui) {
		if(debug) { Debug.Log(name + ": SetCurrentUI(" + ui.name + ")"); }
		if(_curState != InputState.ALL) {
			if(ui == _view.energyPanel && _curState != InputState.ONLY_ENERGY) { return; }
			if(ui == _view.comfortPanel && _curState != InputState.ONLY_COMFORT) { return; }
			if(_view.GetCurrentUI() == _view.mail && ui == _view.inbox && _curState != InputState.ONLY_CLOSE_MAIL) { return; }
		}

		if(_view.GetCurrentUI() != null) {
			if(_view.GetCurrentUI() == _view.inspector) {
				_narrationMgr.OnQuestEvent(Quest.QuestEvent.InspectorClosed);
			} else if(_view.GetCurrentUI() == _view.inbox) {
				_narrationMgr.OnQuestEvent(Quest.QuestEvent.InboxClosed);
			} else if(_view.GetCurrentUI() == _view.mail) {
				_narrationMgr.OnQuestEvent(Quest.QuestEvent.MailClosed);
			}
		}

		if(ui == _view.GetCurrentUI()) {
			HideUI();
		} else {
			_view.SetCurrentUI(ui);
			if(ui == _view.inspector) {
				_narrationMgr.OnQuestEvent(Quest.QuestEvent.InspectorOpened);
			} else if(ui == _view.inbox) {
				_view.BuildInbox(_narrationMgr.GetQuests(), _narrationMgr.GetCompletedQuests());
				_narrationMgr.OnQuestEvent(Quest.QuestEvent.InboxOpened);
			} else if(ui == _view.mail) {
				_narrationMgr.OnQuestEvent(Quest.QuestEvent.MailOpened);
			} else if(ui == _view.energyPanel) {
				_narrationMgr.OnQuestEvent(Quest.QuestEvent.OpenEnergyPanel);
			} else if(ui == _view.comfortPanel) {
				_narrationMgr.OnQuestEvent(Quest.QuestEvent.OpenComfortPanel);
			}
		}

		ResetTouch();
	}

	//Hide any open user interface
	public void HideUI() {
		//if(_curState != InputState.ALL) { return; }

		//if(_view.GetCurrentUI() == _view.inspector) {
		//	_narrationMgr.OnQuestEvent(Quest.QuestEvent.InspectorClosed);
		//} else if(_view.GetCurrentUI() == _view.inbox) {
		//	_narrationMgr.OnQuestEvent(Quest.QuestEvent.InboxClosed);
		//} else if(_view.GetCurrentUI() == _view.mail) {
		//	_narrationMgr.OnQuestEvent(Quest.QuestEvent.MailClosed);
		//}

		_view.SetCurrentUI(null);
	}

	//
	public void OpenMail(Quest quest) {
		if(_curState == InputState.ALL || _curState == InputState.ONLY_OPEN_QUEST) {
			_view.BuildMail(quest);
			ResetTouch();

			_narrationMgr.OnQuestEvent(Quest.QuestEvent.QuestOpened);
		}
	}

	//
	public void OnQuestPressed(Quest quest) {
		if(_curState == InputState.ALL || _curState == InputState.ONLY_OPEN_QUEST) {
			_view.BuildMail(quest);
			//_view.ShowQuest(quest);
			ResetTouch();

			_narrationMgr.OnQuestEvent(Quest.QuestEvent.QuestOpened);
		}
	}
	
	//
	public void OnQuestClosed() {
		_view.HideQuest();
		ResetTouch();
	}

	//
	public void OnHarvestTap(GameObject go) {
		if(_curState == InputState.ALL) {
			_resourceMgr.cash += 10;
			_view.CreateFeedback(go.transform.position, "+" + 10 + "€");
			go.SetActive(false);

			ResetTouch();
			_narrationMgr.OnQuestEvent(Quest.QuestEvent.ResourceHarvested);
		}
	}

	//
	public void OnLightSwitchToggled(ElectricMeter meter) {
		if(meter.GivesPower) {
			_narrationMgr.OnQuestEvent(Quest.QuestEvent.LightSwitchedOn);
		} else {
			_narrationMgr.OnQuestEvent(Quest.QuestEvent.LightSwitchedOff);
		}
	}

	

	//
	public void ApplyEEM(Appliance appliance, EnergyEfficiencyMeasure eem) {
		ResetTouch();
		if(_curState != InputState.ALL && _curState != InputState.ONLY_APPLY_EEM) { return; }

		if(_resourceMgr.cash >= eem.cashCost && _resourceMgr.comfort >= eem.comfortCost) {
			_resourceMgr.cash -= eem.cashCost;
			_resourceMgr.comfort -= eem.comfortCost;

			appliance.ApplyEEM(eem);
			_view.BuildEEMInterface(appliance);

			//_resourceMgr.RefreshProduction();

			//actionObj.SetActive(false);

			_narrationMgr.OnQuestEvent(Quest.QuestEvent.MeasurePerformed, eem.name);
		}

	}

	//
	public void OnAvatarReachedPosition(BehaviourAI avatar, Vector3 pos) {
		//Debug.Log("OnAvatarReachedPosition");
		_narrationMgr.OnQuestEvent(Quest.QuestEvent.AvatarArrived);
		avatar.ReleaseControlOfAvatar();
	}

	//
	public void OnAvatarSessionComplete(string activityState) {
		_narrationMgr.OnQuestEvent(Quest.QuestEvent.AvatarSessionOver, activityState);
	}

	//
	public void OnAvatarActivityComplete(string activity) {
		_narrationMgr.OnQuestEvent(Quest.QuestEvent.AvatarActivityOver, activity);
	}

	//
	public void UpdatePinch(){
		if(Input.touchCount == 2){
			_isPinching = true;

			// Store both touches.
			Touch touchZero = Input.GetTouch(0);
			Touch touchOne = Input.GetTouch(1);
			
			// Find the position in the previous frame of each touch.
			Vector2 touchZeroPrevPos = touchZero.position - touchZero.deltaPosition;
			Vector2 touchOnePrevPos = touchOne.position - touchOne.deltaPosition;
			
			// Find the magnitude of the distance between the touches in each frame.
			float prevTouchDeltaMag = (touchZeroPrevPos - touchOnePrevPos).magnitude;
			float touchDeltaMag = (touchZero.position - touchOne.position).magnitude;
			
			// Find the difference in the distances between each frame.
			float deltaMagnitudeDiff = prevTouchDeltaMag - touchDeltaMag;

			OnPinching(deltaMagnitudeDiff);
		} else if(_isPinching){
			_isPinching = false;

			// Store both touches.
			Touch touchZero = Input.GetTouch(0);
			Touch touchOne = Input.GetTouch(1);

			// Find the position in the previous frame of each touch.
			Vector2 touchZeroPrevPos = touchZero.position - touchZero.deltaPosition;
			Vector2 touchOnePrevPos = touchOne.position - touchOne.deltaPosition;
			
			// Find the magnitude of the distance between the touches in each frame.
			float prevTouchDeltaMag = (touchZeroPrevPos - touchOnePrevPos).magnitude;
			float touchDeltaMag = (touchZero.position - touchOne.position).magnitude;
			
			// Find the difference in the distances between each frame.
			float deltaMagnitudeDiff = prevTouchDeltaMag - touchDeltaMag;
			if(deltaMagnitudeDiff > 0){
				OnPinchOut();
			} else {
				OnPinchIn();
			}
		}
	}

	//
	public bool IsOutsideUI(Vector3 pos){
		bool outsideInspector = true;
		bool outsideMailButton = true;
		if(_view.inspectorUI.activeSelf) {
			outsideInspector =
				pos.x > Screen.width * 0.2f ||
				pos.x < Screen.width - Screen.width * 0.2f ||
				pos.y > Screen.height * 0.12f ||
				pos.y < Screen.height - Screen.height * 0.12f;
		}
		outsideInspector = !_view.inspectorUI.activeSelf;
		outsideMailButton =
			pos.x < Screen.width - Screen.width * 0.2f ||
			pos.y < Screen.height - Screen.height * 0.12f;

		return !_view.IsAnyOverlayActive();
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
	public override void ShowCongratualations(string text) {
		_audioMgr.PlaySound("Fireworks");
		_view.ShowCongratualations(_localMgr.GetPhrase(text));
	}

	//
	public override void PlaySound(string sound) {
		_audioMgr.PlaySound(sound);
	}

	//
	public override void SetControlState(InputState state) {
		_curState = state;
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
		_timeMgr.TimeScale = timeScale;
	}

	//
	public override string GetCurrentDate() {
		return _timeMgr.CurrentDate;
	}

	//
	public override void UnlockView(int x, int y) {
		base.UnlockView(x, y);
	}

	//
	public override void SaveGameState() {
		if(debug) { Debug.Log("Saving game state"); }

		_saveMgr.SetData("ResourceManager", _resourceMgr.SerializeAsJSON());
		_saveMgr.SetData("NarrationManager", _narrationMgr.SerializeAsJSON());
		_saveMgr.SetData("LocalisationManager", _localMgr.SerializeAsJSON());
		_saveMgr.SetData("CameraManager", _camMgr.SerializeAsJSON());

		//Save avatar states
		JSONArray avatarsJSON = new JSONArray();
		foreach(BehaviourAI avatar in _avatars) {
			avatarsJSON.Add(avatar.Encode());
		}
        _saveMgr.SetData("avatarStates", avatarsJSON);

        //Save appliance states
        JSONArray applianceJSON = new JSONArray();
		foreach(Appliance appliance in _appliances) {
			applianceJSON.Add(appliance.SerializeAsJSON());
		}
		_saveMgr.SetData("Appliances", applianceJSON);

		_saveMgr.SetData("lastTime", _timeMgr.time.ToString());
		_saveMgr.SetData("curPilot", Application.loadedLevelName);

		_saveMgr.Save();
	}

	//
	public override void LoadGameState() {
        if (!enableSaveLoad)
        {
            if (debug) { Debug.Log("save/load disabled. Will not load game data."); }
            return;
        }
        if (debug) { Debug.Log("Loading game state"); }

		_saveMgr.Load();

		_resourceMgr.DeserializeFromJSON(_saveMgr.GetClass("ResourceManager"));
		_narrationMgr.DeserializeFromJSON(_saveMgr.GetClass("NarrationManager"));
		_localMgr.DeserializeFromJSON(_saveMgr.GetClass("LocalisationManager"));
		_camMgr.DeserializeFromJSON(_saveMgr.GetClass("CameraManager"));

		//Load avatar states
		if (_saveMgr.GetData("avatarStates") != null)
        {
            JSONArray avatarsJSON = _saveMgr.GetData("avatarStates").AsArray;
            foreach (JSONClass avatarJSON in avatarsJSON)
            {
                foreach (BehaviourAI avatar in _avatars)
                {
                    string loadedName = avatarJSON["name"];
                    if (avatar.name == loadedName)
                    {
                        avatar.Decode(avatarJSON);
                    }
                }
            }
        }

        //Load appliance states
        if (_saveMgr.GetData("Appliances") != null) {
			JSONArray appsJSON = _saveMgr.GetData("Appliances").AsArray;
			foreach(JSONClass appJSON in appsJSON) {
				foreach(Appliance app in _appliances) {
					if(app.GetComponent<UniqueId>().uniqueId.Equals(appJSON["id"])) {
						app.DeserializeFromJSON(appJSON);
					}
				}
			}
		}

		if(_saveMgr.GetData("lastTime") != null) {
			_timeMgr.SetTime(_saveMgr.GetData("lastTime").AsDouble);
		}
	}

	//
	public void GenerateUniqueIDs() {
		List<BehaviourAI> avatars = new List<BehaviourAI>(Object.FindObjectsOfType<BehaviourAI>());
		List<Appliance> appliances = new List<Appliance>(Object.FindObjectsOfType<Appliance>());

		foreach(BehaviourAI avatar in avatars) {
            UniqueId[] ids = avatar.GetComponents<UniqueId>();
            foreach (UniqueId id in ids) {
				DestroyImmediate(id);
			}
			avatar.gameObject.AddComponent<UniqueId>();
		}

		foreach(Appliance app in appliances) {
            UniqueId[] ids = app.GetComponents<UniqueId>();
            foreach (UniqueId id in ids)
            {
                DestroyImmediate(id);
            }
            app.gameObject.AddComponent<UniqueId>();
		}
	}

	//
	public void LoadScene(string scene) {
		SaveGameState();
		_sceneMgr.LoadScene(scene);
	}
}
