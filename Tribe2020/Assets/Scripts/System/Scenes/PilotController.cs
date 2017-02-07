using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using SimpleJSON;
using System;

public class PilotController : MonoBehaviour, NarrationInterface, AudioInterface, CameraInterface {
	//Singleton features
	private static PilotController _instance;
	public static PilotController GetInstance() {
		return _instance as PilotController;
	}
	//public enum InputState {
	//	ALL, ONLY_PROMPT, ONLY_SWIPE, ONLY_TAP, ONLY_APPLIANCE_SELECT, ONLY_APPLIANCE_DESELECT,
	//	ONLY_OPEN_INBOX, ONLY_CLOSE_INBOX, ONLY_ENERGY, ONLY_COMFORT, ONLY_SWITCH_LIGHT, ONLY_APPLY_EEM, ONLY_HARVEST, NOTHING,
	//	ONLY_CLOSE_MAIL, ONLY_SELECT_OVERVIEW, ONLY_SELECT_GRIDVIEW
	//};
	private Controller.InputState _curState = Controller.InputState.ALL;

	#region Fields
	public bool debug = false;
	public bool enableSaveLoad = true;

	public double startTime;
	public double endTime;
	public double playPeriod;

	//Access all singleton systemss
	private PilotView _view;
	private GameTime _timeMgr;
	private CameraManager _cameraMgr;
	private AudioManager _audioMgr;
	private MainMeter _mainMeter;
	private ResourceManager _resourceMgr;
	private NarrationManager _narrationMgr;
	private CustomSceneManager _sceneMgr;
	private SaveManager _saveMgr;
	private LocalisationManager _localMgr;
	private AvatarManager _avatarMgr;
	private ApplianceManager _applianceMgr;

	//Interaction props
	[SerializeField]
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

	[Header("Save between sessions")]
	public bool syncTime;
	public bool syncPilot;
	public bool syncCamera;
	public bool syncResources;
	public bool syncNarrative;
	public bool syncLocalization;
	public bool syncAvatars;
	public bool syncAppliances;

	//private List<Appliance> _appliances;

	private bool _firstUpdate = false;
	#endregion

	//Sort use instead of constructor
	void Awake() {
		_instance = this;
		_instance = this;
	}

	//
	public void SetToInstance(PilotController oldInstance) {

	}

	// Use this for initialization
	void Start() {
		_view = PilotView.GetInstance();
		_view.SetController(this);

		_timeMgr = GameTime.GetInstance();

		_audioMgr = AudioManager.GetInstance();
		_audioMgr.SetInterface(this);

		_mainMeter = MainMeter.GetInstance();
		_resourceMgr = ResourceManager.GetInstance();

		_narrationMgr = NarrationManager.GetInstance();
		_narrationMgr.SetInterface(this);

		_sceneMgr = CustomSceneManager.GetInstance();
		_saveMgr = SaveManager.GetInstance();
		_localMgr = LocalisationManager.GetInstance();

		_cameraMgr = CameraManager.GetInstance();
		_cameraMgr.SetInterface(this);

		_avatarMgr = AvatarManager.GetInstance();
		_applianceMgr = ApplianceManager.GetInstance();

		//_appliances = new List<Appliance>(UnityEngine.Object.FindObjectsOfType<Appliance>());

		ClearView();
		_view.TranslateInterface();

		playPeriod = endTime - startTime;
	}

	// Update is called once per frame
	void Update() {
		if(!_firstUpdate) {
			_firstUpdate = true;
			LoadGameState();

			_narrationMgr.Init();
			//_view.UpdateViewpointGuide(_camMgr.GetViewpoints(), _camMgr.GetCurrentViewpoint());
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

		_view.date.GetComponent<Text>().text = _timeMgr.GetTimeWithFormat("HH:mm d MMM yyyy");
		_view.power.GetComponent<Text>().text = Mathf.Floor(_mainMeter.Power) + " W";
		float energy = (float)_mainMeter.Energy;
		if(energy < 1000) {
			_view.energyCounter.text = Mathf.Floor(energy) + " Wh";
		} else if(energy < 1000000) {
			_view.energyCounter.text = Mathf.Floor(energy / 10) / 100 + " kWh";
		} else if(energy < 100000000000) {
			_view.energyCounter.text = Mathf.Floor(energy / 1000) + " kWh";
		} else {
			_view.energyCounter.text = Mathf.Floor(energy / 1000000) + " MWh";
		}

		_view.cash.GetComponent<Text>().text = _resourceMgr.cash.ToString();
		_view.comfort.GetComponent<Text>().text = _resourceMgr.comfort.ToString();
		_view.UpdateQuestCount(_narrationMgr.GetQuests().Count);

		_view.UpdateTime((float)((_timeMgr.time - startTime) / playPeriod));
		if(_timeMgr.time > endTime) {
			LoadScene("MenuScene");
		}
	}

	//
	private void OnTouchStart(Vector3 pos) {
		//Debug.Log("OnTouchStart");
		_touchTimer = 0;
		if(_touchState == IDLE) {
			_startPos = pos;
		}
	}

	//
	private void OnTouch(Vector3 pos) {
		//_camMgr.cameraState = CameraManager.PANNED;
		_touchTimer += Time.unscaledDeltaTime;

		//if(Application.platform == RuntimePlatform.Android){
		//	_camMgr.UpdatePan(Input.GetTouch(0).deltaPosition);
		//}
	}

	//
	private void OnTouchEnded(Vector3 pos) {
		//Debug.Log("ontouchended");
		_cameraMgr.cameraState = CameraManager.CameraState.Idle;
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
	private void OnTap(Vector3 pos) {
		if(_curState != Controller.InputState.ALL && _curState != Controller.InputState.ONLY_TAP) { return; }

		_narrationMgr.OnQuestEvent(Quest.QuestEvent.Tapped);
		_narrationMgr.OnNarrativeEvent("Tapped");
	}

	//
	private void OnDoubleTap(Vector3 pos) {
		//Debug.Log("Double tapped at " + pos);
	}

	//
	private void OnSwipe(Vector3 start, Vector3 end) {
		if(_view.IsAnyOverlayActive()) {
			return;
		}

		if(_curState == Controller.InputState.ALL || _curState == Controller.InputState.ONLY_SWIPE) {
			float dir = Mathf.Atan2(end.y - start.y, end.x - start.x);
			dir = (dir * Mathf.Rad2Deg + 360) % 360;
			float dist = Vector3.Distance(start, end);

			float dirMod = (dir + 90) % 360;
			if(dirMod > 45 && dirMod <= 135) {
				_cameraMgr.GotoLeftView();
			} else if(dir > 45 && dir <= 135) {
				_cameraMgr.GotoLowerView();
			} else if(dir > 135 && dir <= 225) {
				_cameraMgr.GotoRightView();
			} else if(dir > 225 && dir <= 315) {
				_cameraMgr.GotoUpperView();
			}

			_narrationMgr.OnNarrativeEvent("Swiped");

			_narrationMgr.OnQuestEvent(Quest.QuestEvent.Swiped);
			_narrationMgr.OnQuestEvent(Quest.QuestEvent.FindView, _cameraMgr.GetCurrentViewpoint().title);
		}
	}

	//
	public void OnPinchIn() {
		if(_curState == Controller.InputState.ALL) {
			_cameraMgr.GotoUpperView();
		}
	}

	//
	public void OnPinchOut() {
		if(_curState == Controller.InputState.ALL) {
			_cameraMgr.GotoLowerView();
		}
	}

	//
	public void OnPinching(float magnitude) {
	}

	//
	public void ResetTouch() {
		_touchTimer = 0;
		_doubleTimer = 0;
		_startPos = Input.mousePosition;
		_touchState = IDLE;
		_touchReset = true;
		//Debug.Log("cotroller.resetSwipe " + _startPos);
	}

	//
	public void UpdatePinch() {
		if(Input.touchCount == 2) {
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
		} else if(_isPinching) {
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
			if(deltaMagnitudeDiff > 0) {
				OnPinchOut();
			} else {
				OnPinchIn();
			}
		}
	}

	//
	public void OnOkPressed() {
		_instance.ResetTouch();

		if(_instance._curState == Controller.InputState.ALL || _instance._curState == Controller.InputState.ONLY_PROMPT) {
			_instance._view.messageUI.SetActive(false);

			_instance._narrationMgr.OnQuestEvent(Quest.QuestEvent.OKPressed);
			_instance._narrationMgr.OnNarrativeEvent("OKPressed");
		}

		_instance.PlaySound("Press Button");
	}

	//
	public void ToggleMenu() {
		_instance._view.ToggleMenu();
	}

	//Open inspector with details of appliance
	public void SetCurrentUI(Appliance app) {
		if(_curState != Controller.InputState.ALL && _curState != Controller.InputState.ONLY_APPLIANCE_SELECT) { return; }

		_instance._cameraMgr.SetLookAtTarget(app);

		string title = _localMgr.GetPhrase("Appliance:" + app.title + "_Title");
		string description = _localMgr.GetPhrase("Appliance:" + app.title + "_Description");

		if(app.GetComponent<BehaviourAI>()) {
			_instance._view.BuildAvatarPanel(app);
			SetCurrentUI(_instance._view.characterPanel);
		} else {
			_instance._view.BuildDevicePanel(app);
			SetCurrentUI(_instance._view.devicePanel);
		}

		//_view.BuildInspector(appliance);
		//SetCurrentUI(_view.inspector);

		//_narrationMgr.OnQuestEvent(Quest.QuestEvent.InspectorOpened);
		_instance._narrationMgr.OnQuestEvent(Quest.QuestEvent.InspectorOpened, app.title);
		if(app.GetComponent<BehaviourAI>()) {
			_instance._narrationMgr.OnQuestEvent(Quest.QuestEvent.AvatarSelected, app.title);
		}
	}

	//Open mail with details of narrative
	public void SetCurrentUI(Quest quest) {
		if(debug) { Debug.Log(name + ": SetCurrentUI(" + quest.name + ")"); }
		//if(_curState != InputState.ALL && _curState != InputState.ONLY_OPEN_QUEST) { return; }

		//_view.BuildMail(quest);
		//SetCurrentUI(_view.mail);

		//_narrationMgr.OnQuestEvent(Quest.QuestEvent.MailOpened);
	}

	//
	public void SetCurrentUI(RectTransform ui) {
		SetCurrentUI(_instance, ui);
	}

	//Open user interface
	public void SetCurrentUI(PilotController controller, RectTransform ui) {
		if(debug) { Debug.Log(name + ": SetCurrentUI(" + ui.name + ")"); }
		if(_curState != Controller.InputState.ALL) {
			if(ui == controller._view.energyPanel && controller._curState != Controller.InputState.ONLY_ENERGY) { return; }
			if(ui == controller._view.comfortPanel && controller._curState != Controller.InputState.ONLY_COMFORT) { return; }
			if(ui == controller._view.inbox && controller._curState != Controller.InputState.ONLY_OPEN_INBOX) { return; }
		}

		if(controller._view.GetCurrentUI() != null) {
			if(controller._view.GetCurrentUI() == controller._view.inspector) {
				controller._narrationMgr.OnQuestEvent(Quest.QuestEvent.InspectorClosed);
			} else if(controller._view.GetCurrentUI() == controller._view.inbox) {
				controller._narrationMgr.OnQuestEvent(Quest.QuestEvent.InboxClosed);
			}
		}

		if(ui == controller._view.GetCurrentUI()) {
			HideUI();
		} else {
			controller._view.SetCurrentUI(ui);
			if(ui == controller._view.inspector) {
				controller._narrationMgr.OnQuestEvent(Quest.QuestEvent.InspectorOpened);
			} else if(ui == controller._view.inbox) {
				controller._view.BuildInbox(controller._narrationMgr.GetQuests(), controller._narrationMgr.GetCompletedQuests());
				controller._narrationMgr.OnQuestEvent(Quest.QuestEvent.InboxOpened);
				_instance._narrationMgr.OnNarrativeEvent("InboxOpened");
			} else if(ui == controller._view.energyPanel) {
				controller._narrationMgr.OnQuestEvent(Quest.QuestEvent.OpenEnergyPanel);
				_instance._narrationMgr.OnNarrativeEvent("EnergyPanelOpened");
			} else if(ui == controller._view.comfortPanel) {
				controller._narrationMgr.OnQuestEvent(Quest.QuestEvent.OpenComfortPanel);
			} else if(ui == controller._view.apocalypsometer) {
				controller._narrationMgr.OnQuestEvent(Quest.QuestEvent.SelectedOverview);
				_instance._narrationMgr.OnNarrativeEvent("ApocalypsometerOpened");
			}
		}

		controller.ResetTouch();
	}

	//Hide any open user interface
	public void HideUI() {
		if(_instance._view.GetCurrentUI() == _instance._view.apocalypsometer) {
			_instance._narrationMgr.OnQuestEvent(Quest.QuestEvent.ApocalypsometerClosed);
		}

		_instance._view.SetCurrentUI(null);
		_instance._cameraMgr.ClearLookAtTarget();

		//_view.SetCurrentUI(null);
  //      _camMgr.ClearLookAtTarget();
	}

	//
	public void SelectGridView() {
		if(_curState != Controller.InputState.ALL && _curState != Controller.InputState.ONLY_SELECT_GRIDVIEW) { return; }

		_view.EnableEnergyPanel();
		_view.EnableComfortPanel();
		_view.HideApocalypsometer();

		_cameraMgr.GoToGridView();
		_narrationMgr.OnQuestEvent(Quest.QuestEvent.SelectedGridView);
	}

	//
	public void SelectOverview() {
		if(_curState != Controller.InputState.ALL && _curState != Controller.InputState.ONLY_SELECT_OVERVIEW) { return; }

		_view.DisableEnergyPanel();
		_view.DisableComfortPanel();
		_view.ShowApocalypsometer();

		_cameraMgr.GoToOverview();
		_narrationMgr.OnQuestEvent(Quest.QuestEvent.SelectedOverview);
	}

	//Request narration event for current view
	public void RequestCurrentView() {
		_narrationMgr.OnQuestEvent(Quest.QuestEvent.FindView, _cameraMgr.GetCurrentViewpoint().title);
	}

	//
	public void OnHarvestTap(GameObject go) {
		if(_curState != Controller.InputState.ALL && _curState != Controller.InputState.ONLY_HARVEST) { return; }

		_resourceMgr.cash += 10;
		_view.CreateFeedback(go.transform.position, "+" + 10 + "€");
		go.SetActive(false);

		ResetTouch();
		_narrationMgr.OnQuestEvent(Quest.QuestEvent.ResourceHarvested);
	}

	//
	public void OnHarvest(Gem gem) {
		//_tempInstance._resourceMgr.AddComfort(gem.value);
		Destroy(gem.gameObject);

		switch(gem.type) {
			case "cash":
				_instance._resourceMgr.AddCash(gem.value);
				break;
			case "satisfaction":
				_instance._resourceMgr.AddComfort(gem.value);
				break;
		}

		_instance._audioMgr.PlaySound("Press Button");
		_instance._narrationMgr.OnQuestEvent(Quest.QuestEvent.ResourceHarvested, "satisfaction");
	}

	//
	public void OnElectricMeterToggle(ElectricMeter meter) {

        Appliance appliance = meter.gameObject.GetComponent<Appliance>();
        if (appliance) {

            Room zone = appliance.GetZone();

            //If appliance is light switch
            foreach (Appliance.AffordanceResource affordanceSlot in appliance.avatarAffordances) {
                if (affordanceSlot.affordance == zone.avatarAffordanceSwitchLight) {
                    _avatarMgr.OnLightToggled(zone, meter.GivesPower);

                    if (meter.GivesPower) {
                        _narrationMgr.OnQuestEvent(Quest.QuestEvent.LightSwitchedOn);
						_narrationMgr.OnNarrativeEvent("TurnedOnLamp");
                    }
                    else {
                        _narrationMgr.OnQuestEvent(Quest.QuestEvent.LightSwitchedOff);
						_narrationMgr.OnNarrativeEvent("TurnedOffLamp");
					}
                }
            }

        }
	}

	//
	public void OnNewViewpoint(Viewpoint curView, Viewpoint[][] viewMatrix, bool overview) {
		_instance._view.UpdateViewpointGuide(_instance._cameraMgr.GetViewpoints(), curView, overview);
		_instance._view.UpdateViewpointTitle(curView.title);

		_instance._narrationMgr.OnQuestEvent(Quest.QuestEvent.FindView, curView.title);
	}

	//
	public void ApplyEEM(Appliance appliance, EnergyEfficiencyMeasure eem) {
		ResetTouch();
		if(_curState != Controller.InputState.ALL && _curState != Controller.InputState.ONLY_APPLY_EEM) { return; }

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
		if(_narrationMgr != null) {
			_narrationMgr.OnQuestEvent(Quest.QuestEvent.AvatarActivityOver, activity);
		}
	}

	#region Narrative Methods
	//
	public void SetControlState(Controller.InputState state) {
		_curState = state;
	}

	//
	public void LimitInteraction(string interactionLimit) {
		switch(interactionLimit) {
			case "only_ok": _curState = Controller.InputState.ONLY_PROMPT; break;
			case "only_swipe": _curState = Controller.InputState.ONLY_SWIPE; break;
			default: _curState = Controller.InputState.ALL; break;
		}
	}

	//
	public void ControlInterface(string id, string action) {
		_instance._view.ControlInterface(id, action);
	}

	//
	public void ControlInterface(string cmd) {
		JSONNode json = JSON.Parse(cmd);
		string id = json["id"];
		string action = json["action"];

		_view.ControlInterface(id,action);
	}

	//
	public void PlaySound(string sound) {
		_instance._audioMgr.PlaySound(sound);
	}

	//
	public void PlayUIAnimation(string animation) {
		_instance._view.PlayUIAnimation(animation);
	}

	//
	public void StopUIAnimation() {
		_instance._view.PlayUIAnimation("IdleCanvas");
	}

	//
	public void ShowMessage(string cmd) {
		JSONNode json = JSON.Parse(cmd);
		string avatarName = json["avatar"];
		string group = json["group"];
		string key = json["key"];
		bool button = json["button"].AsBool;

		Sprite portrait = null;
		if(avatarName != null) {
			if(avatarName == "player") {
				portrait = _instance._avatarMgr.playerPortrait;
			} else {
				portrait = _instance._avatarMgr.GetAvatar(avatarName).portrait;
			}
		}
		string message = GetPhrase(group, key);

		_instance._view.ShowMessage(message, portrait, true, button);
	}

	//
	public void HideMessage() {
		_instance._view.HideMessage();
	}

	//
	public void ShowMessage(string key, string message, Sprite portrait, bool showButton) {
		if(debug) { Debug.Log(name + ": ShowMessage(" + key + ", " + message + ")"); }
		string msg = _localMgr.GetPhrase(key);
		if(msg == "") { msg = message + "!"; }

		_instance._view.ShowMessage(msg, portrait, true, showButton);
	}

	//
	public void CreateHarvest(string command) {
		JSONNode json = JSON.Parse(command);
		string currency = json["type"];
		string location = json["location"];

		if(location.Equals("current")) {
			Room room = _cameraMgr.GetCurrentViewpoint().relatedZones[0];
			room.GetApplianceWithAffordance(room.avatarAffordanceSwitchLight).AddHarvest();
		}
	}

	//
	public void ControlAvatar(string id, string action, Vector3 pos) {
		_avatarMgr.ControlAvatar(id, action, pos);
	}

	//
	public void ControlAvatar(string id, UnityEngine.Object action) {
		_avatarMgr.ControlAvatar(id, action);
	}

	//
	public void UnlockView(int x, int y) {
		_cameraMgr.UnlockView(x, y);
		_view.UpdateViewpointGuide(_cameraMgr.GetViewpoints(), _cameraMgr.GetCurrentViewpoint());
	}

	//
	public void ShowCongratualations(string text) {
		_instance._audioMgr.PlaySound("Fireworks");
		_instance._view.ShowCongratualations(_localMgr.GetPhrase(text));
		_instance._cameraMgr.PlayFireworks();
	}

	//
	public void ClearView() {
		_view.ClearView();
	}
	#endregion

	//
	public string GetPhrase(string groupKey, string key, int index) {
		return _localMgr.GetPhrase(groupKey, key, index);
	}

	//
	public string GetPhrase(string groupKey, string key) {
		return _instance._localMgr.GetPhrase(groupKey, key);
	}

	//
	public void SetTimeScale(int timeScale) {
		_instance._timeMgr.VisualTimeScale = timeScale;
	}

	//
	public void SetTimeScale(float timeScale) {
		_instance._timeMgr.SimulationTimeScaleFactor = timeScale;
	}

	//
	public void StepTimeForward(int days) {
		_timeMgr.Offset(86400 * days);
	}

	//
	public string GetCurrentDate() {
		return _timeMgr.CurrentDate;
	}

	//
	public void SaveGameState() {
		if(debug) { Debug.Log("Saving game state"); }

		if(syncResources) _saveMgr.SetCurrentSlotClass("ResourceManager", _resourceMgr.SerializeAsJSON());
		if(syncNarrative) _saveMgr.SetCurrentSlotClass("NarrationManager", _narrationMgr.SerializeAsJSON());
		if(syncLocalization) _saveMgr.SetData("language", _localMgr.curLanguage.name);
		if(syncCamera) _saveMgr.SetCurrentSlotClass("CameraManager", _cameraMgr.SerializeAsJSON());
		if(syncAvatars) _saveMgr.SetCurrentSlotClass("AvatarManager", _avatarMgr.SerializeAsJSON());
		if(syncAppliances) _saveMgr.SetCurrentSlotClass("ApplianceManager", _applianceMgr.SerializeAsJSON());

		//Save appliance states
		//if(syncAppliances) {
		//	JSONArray applianceJSON = new JSONArray();
		//	foreach(Appliance appliance in _appliances) {
		//		applianceJSON.Add(appliance.SerializeAsJSON());
		//	}
		//	_saveMgr.SetCurrentSlotArray("Appliances", applianceJSON);
		//}

		if(syncTime) _saveMgr.SetCurrentSlotData("lastTime", _timeMgr.offset.ToString());
		if(syncPilot) _saveMgr.SetCurrentSlotData("curPilot", Application.loadedLevelName);

		_saveMgr.SaveCurrentSlot();
	}

	//
	public void LoadGameState() {
		if(!enableSaveLoad) {
			if(debug) { Debug.Log("save/load disabled. Will not load game data."); }
			return;
		}
		if(debug) { Debug.Log("Loading game state"); }

		if(syncPilot) _saveMgr.LoadCurrentSlot();

		if(syncResources) _resourceMgr.DeserializeFromJSON(_saveMgr.GetCurrentSlotClass("ResourceManager"));
		if(syncNarrative) _narrationMgr.DeserializeFromJSON(_saveMgr.GetCurrentSlotClass("NarrationManager"));
		if(syncLocalization) _localMgr.SetLanguage(_saveMgr.GetData("language"));
		if(syncCamera) _cameraMgr.DeserializeFromJSON(_saveMgr.GetCurrentSlotClass("CameraManager"));
		if(syncAvatars) _avatarMgr.DeserializeFromJSON(_saveMgr.GetCurrentSlotClass("AvatarManager"));
		if(syncAppliances) _applianceMgr.DeserializeFromJSON(_saveMgr.GetCurrentSlotClass("ApplianceManager"));

		//Load appliance states
		//if(syncAppliances && _saveMgr.GetCurrentSlotData("Appliances") != null) {
		//	JSONArray appsJSON = _saveMgr.GetCurrentSlotData("Appliances").AsArray;
		//	foreach(JSONClass appJSON in appsJSON) {
		//		foreach(Appliance app in _appliances) {
		//			if(app.GetComponent<UniqueId>().uniqueId.Equals(appJSON["id"])) {
		//				app.DeserializeFromJSON(appJSON);
		//			}
		//		}
		//	}
		//}

		if(syncTime && _saveMgr.GetCurrentSlotData("lastTime") != null) {
			_timeMgr.offset = (_saveMgr.GetCurrentSlotData("lastTime").AsDouble);
		}
	}

	//
	public void GenerateUniqueIDs() {
		List<BehaviourAI> avatars = new List<BehaviourAI>(UnityEngine.Object.FindObjectsOfType<BehaviourAI>());
		List<Appliance> appliances = new List<Appliance>(UnityEngine.Object.FindObjectsOfType<Appliance>());

		foreach(BehaviourAI avatar in avatars) {
			UniqueId[] ids = avatar.GetComponents<UniqueId>();
			foreach(UniqueId id in ids) {
				DestroyImmediate(id);
			}
			avatar.gameObject.AddComponent<UniqueId>();
		}

		foreach(Appliance app in appliances) {
			UniqueId[] ids = app.GetComponents<UniqueId>();
			foreach(UniqueId id in ids) {
				DestroyImmediate(id);
			}
			app.gameObject.AddComponent<UniqueId>();
		}
	}

	//
	public void LoadScene(string scene) {
		_instance.SaveGameState();
		_instance._sceneMgr.LoadScene(scene);
	}

	//
	public void MoveCamera(string animation) {
		_instance._cameraMgr.PlayAnimation(animation);
	}

	public void StopCamera() {
		_instance._cameraMgr.StopAnimation();
	}

	public void OnAnimationEvent(string animationEvent) {
		_narrationMgr.OnQuestEvent(Quest.QuestEvent.CameraAnimationEvent, animationEvent);
		_narrationMgr.OnNarrativeEvent("AnimationEvent", animationEvent);
	}
}
