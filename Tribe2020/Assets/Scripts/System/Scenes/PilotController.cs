using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using SimpleJSON;
using System;

public class PilotController : Controller, NarrationInterface, AudioInterface, CameraInterface {
	//Singleton features
	private static PilotController _tempInstance;
	public static PilotController GetInstance() {
		return _instance as PilotController;
	}
	private InputState _curState = InputState.ALL;

	#region Fields
	public bool debug = false;
	public bool enableSaveLoad = true;

	public double startTime;
	public double endTime;
	public double playPeriod;

	//Access all singleton systemss
	public PilotView view;
	private GameTime _timeMgr;
	private CameraManager cameraMgr;
	private AudioManager _audioMgr;
	private MainMeter _mainMeter;
	private ResourceManager _resourceMgr;
	private NarrationManager _narrationMgr;
	private CustomSceneManager _sceneMgr;
	private SaveManager _saveMgr;
	private LocalisationManager _localMgr;
	[SerializeField]
	private AvatarManager _avatarMgr;

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

	private List<Appliance> _appliances;

	private bool _firstUpdate = false;
	#endregion

	//Sort use instead of constructor
	void Awake() {
		_instance = this;
		_tempInstance = this;
	}

	//
	public void SetToInstance(PilotController oldInstance) {

	}

	// Use this for initialization
	void Start() {
		view = PilotView.GetInstance();
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

		cameraMgr = CameraManager.GetInstance();
		cameraMgr.SetInterface(this);

		_avatarMgr = AvatarManager.GetInstance();

		_appliances = new List<Appliance>(UnityEngine.Object.FindObjectsOfType<Appliance>());

		ClearView();

		playPeriod = endTime - startTime;
	}

	// Update is called once per frame
	void Update() {
		if(!_firstUpdate) {
			_firstUpdate = true;
			LoadGameState();
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

		view.date.GetComponent<Text>().text = _timeMgr.GetTimeWithFormat("HH:mm d MMM yyyy");
		view.power.GetComponent<Text>().text = Mathf.Floor(_mainMeter.Power) + " W";
		float energy = (float)_mainMeter.Energy;
		if(energy < 1000) {
			view.energyCounter.text = Mathf.Floor(energy) + " Wh";
		} else if(energy < 1000000) {
			view.energyCounter.text = Mathf.Floor(energy / 10) / 100 + " kWh";
		} else if(energy < 100000000000) {
			view.energyCounter.text = Mathf.Floor(energy / 1000) + " kWh";
		} else {
			view.energyCounter.text = Mathf.Floor(energy / 1000000) + " MWh";
		}

		view.cash.GetComponent<Text>().text = _resourceMgr.cash.ToString();
		view.comfort.GetComponent<Text>().text = _resourceMgr.comfort.ToString();
		view.UpdateQuestCount(_narrationMgr.GetQuests().Count);

		view.UpdateTime((float)((_timeMgr.time - startTime) / playPeriod));
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
		cameraMgr.cameraState = CameraManager.CameraState.Idle;
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
		if(_curState != InputState.ALL && _curState != InputState.ONLY_TAP) { return; }

		_narrationMgr.OnQuestEvent(Quest.QuestEvent.Tapped);
	}

	//
	private void OnDoubleTap(Vector3 pos) {
		//Debug.Log("Double tapped at " + pos);
	}

	//
	private void OnSwipe(Vector3 start, Vector3 end) {
		//Debug.Log("cotroller.OnSwipe " + start + " , " + end);
		if(view.IsAnyOverlayActive()) {
			return;
		}

		if(_curState == InputState.ALL || _curState == InputState.ONLY_SWIPE) {
			float dir = Mathf.Atan2(end.y - start.y, end.x - start.x);
			dir = (dir * Mathf.Rad2Deg + 360) % 360;
			float dist = Vector3.Distance(start, end);

			float dirMod = (dir + 90) % 360;
			if(dirMod > 45 && dirMod <= 135) {
				cameraMgr.GotoLeftView();
			} else if(dir > 45 && dir <= 135) {
				cameraMgr.GotoLowerView();
			} else if(dir > 135 && dir <= 225) {
				cameraMgr.GotoRightView();
			} else if(dir > 225 && dir <= 315) {
				cameraMgr.GotoUpperView();
			}

			_narrationMgr.OnQuestEvent(Quest.QuestEvent.Swiped);
			_narrationMgr.OnQuestEvent(Quest.QuestEvent.FindView, cameraMgr.GetCurrentViewpoint().title);
		}
	}

	//
	public void OnPinchIn() {
		if(_curState == InputState.ALL) {
			cameraMgr.GotoUpperView();
		}
	}

	//
	public void OnPinchOut() {
		if(_curState == InputState.ALL) {
			cameraMgr.GotoLowerView();
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
		_tempInstance.ResetTouch();

		if(_tempInstance._curState == InputState.ALL || _tempInstance._curState == InputState.ONLY_PROMPT) {
			_tempInstance.view.messageUI.SetActive(false);

			_tempInstance._narrationMgr.OnQuestEvent(Quest.QuestEvent.OKPressed);
		}

		_tempInstance.PlaySound("Press Button");
	}

	//
	public void ToggleMenu() {
		_tempInstance.view.ToggleMenu();
	}

	//Open inspector with details of appliance
	public void SetCurrentUI(Appliance app) {
		if(_curState != InputState.ALL && _curState != InputState.ONLY_APPLIANCE_SELECT) { return; }

		_tempInstance.cameraMgr.SetLookAtTarget(app);

		string title = _localMgr.GetPhrase("Appliance:" + app.title + "_Title");
		string description = _localMgr.GetPhrase("Appliance:" + app.title + "_Description");

		_tempInstance.view.BuildInspector(title, description, app);
		SetCurrentUI(_tempInstance.view.characterPanel);

		//_view.BuildInspector(appliance);
		//SetCurrentUI(_view.inspector);

		//_narrationMgr.OnQuestEvent(Quest.QuestEvent.InspectorOpened);
		_tempInstance._narrationMgr.OnQuestEvent(Quest.QuestEvent.InspectorOpened, app.title);
		if(app.GetComponent<BehaviourAI>()) {
			_tempInstance._narrationMgr.OnQuestEvent(Quest.QuestEvent.AvatarSelected, app.title);
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
		SetCurrentUI(_tempInstance, ui);
	}

	//Open user interface
	public void SetCurrentUI(PilotController controller, RectTransform ui) {
		if(debug) { Debug.Log(name + ": SetCurrentUI(" + ui.name + ")"); }
		if(_curState != InputState.ALL) {
			if(ui == controller.view.energyPanel && controller._curState != InputState.ONLY_ENERGY) { return; }
			if(ui == controller.view.comfortPanel && controller._curState != InputState.ONLY_COMFORT) { return; }
			if(ui == controller.view.inbox && controller._curState != InputState.ONLY_OPEN_INBOX) { return; }
		}

		if(controller.view.GetCurrentUI() != null) {
			if(controller.view.GetCurrentUI() == controller.view.inspector) {
				controller._narrationMgr.OnQuestEvent(Quest.QuestEvent.InspectorClosed);
			} else if(controller.view.GetCurrentUI() == controller.view.inbox) {
				controller._narrationMgr.OnQuestEvent(Quest.QuestEvent.InboxClosed);
			}
		}

		if(ui == controller.view.GetCurrentUI()) {
			HideUI();
		} else {
			controller.view.SetCurrentUI(ui);
			if(ui == controller.view.inspector) {
				controller._narrationMgr.OnQuestEvent(Quest.QuestEvent.InspectorOpened);
			} else if(ui == controller.view.inbox) {
				controller.view.BuildInbox(controller._narrationMgr.GetQuests(), controller._narrationMgr.GetCompletedQuests());
				controller._narrationMgr.OnQuestEvent(Quest.QuestEvent.InboxOpened);
			} else if(ui == controller.view.energyPanel) {
				controller._narrationMgr.OnQuestEvent(Quest.QuestEvent.OpenEnergyPanel);
			} else if(ui == controller.view.comfortPanel) {
				controller._narrationMgr.OnQuestEvent(Quest.QuestEvent.OpenComfortPanel);
			} else if(ui == controller.view.apocalypsometer) {
				controller._narrationMgr.OnQuestEvent(Quest.QuestEvent.SelectedOverview);
			}
		}

		controller.ResetTouch();
	}

	//Hide any open user interface
	public void HideUI() {
		_tempInstance.view.SetCurrentUI(null);
		_tempInstance.cameraMgr.ClearLookAtTarget();

		//_view.SetCurrentUI(null);
  //      _camMgr.ClearLookAtTarget();
	}

	//
	public void SelectGridView() {
		if(_curState != InputState.ALL && _curState != InputState.ONLY_SELECT_GRIDVIEW) { return; }

		view.EnableEnergyPanel();
		view.EnableComfortPanel();
		view.HideApocalypsometer();

		cameraMgr.GoToGridView();
		_narrationMgr.OnQuestEvent(Quest.QuestEvent.SelectedGridView);
	}

	//
	public void SelectOverview() {
		if(_curState != InputState.ALL && _curState != InputState.ONLY_SELECT_OVERVIEW) { return; }

		view.DisableEnergyPanel();
		view.DisableComfortPanel();
		view.ShowApocalypsometer();

		cameraMgr.GoToOverview();
		_narrationMgr.OnQuestEvent(Quest.QuestEvent.SelectedOverview);
	}

	//Request narration event for current view
	public void RequestCurrentView() {
		_narrationMgr.OnQuestEvent(Quest.QuestEvent.FindView, cameraMgr.GetCurrentViewpoint().title);
	}

	//
	public void OnHarvestTap(GameObject go) {
		if(_curState != InputState.ALL && _curState != InputState.ONLY_HARVEST) { return; }

		_resourceMgr.cash += 10;
		view.CreateFeedback(go.transform.position, "+" + 10 + "€");
		go.SetActive(false);

		ResetTouch();
		_narrationMgr.OnQuestEvent(Quest.QuestEvent.ResourceHarvested);
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
                    }
                    else {
                        _narrationMgr.OnQuestEvent(Quest.QuestEvent.LightSwitchedOff);
                    }
                }
            }

        }
	}

	//
	public override void OnNewViewpoint(Viewpoint curView, Viewpoint[][] viewMatrix, bool overview) {
		_tempInstance.view.UpdateViewpointGuide(_tempInstance.cameraMgr.GetViewpoints(), curView, overview);
		_tempInstance.view.UpdateViewpointTitle(curView.title);

		_tempInstance._narrationMgr.OnQuestEvent(Quest.QuestEvent.FindView, curView.title);
	}

	//
	public void ApplyEEM(Appliance appliance, EnergyEfficiencyMeasure eem) {
		ResetTouch();
		if(_curState != InputState.ALL && _curState != InputState.ONLY_APPLY_EEM) { return; }

		if(_resourceMgr.cash >= eem.cashCost && _resourceMgr.comfort >= eem.comfortCost) {
			_resourceMgr.cash -= eem.cashCost;
			_resourceMgr.comfort -= eem.comfortCost;

			appliance.ApplyEEM(eem);
			view.BuildEEMInterface(appliance);

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
	public override void SetControlState(InputState state) {
		_curState = state;
	}

	//
	public override void ControlInterface(string id, string action) {
		_tempInstance.view.ControlInterface(id, action);
	}

	//
	public override void PlaySound(string sound) {
		_audioMgr.PlaySound(sound);
	}

	//
	public void ShowMessage(string key, string message, Sprite portrait, bool showButton) {
		if(debug) { Debug.Log(name + ": ShowMessage(" + key + ", " + message + ")"); }
		string msg = _localMgr.GetPhrase(key);
		if(msg == "") { msg = message + "!"; }

		view.ShowMessage(msg, portrait, true, showButton);
	}

	//
	public void CreateHarvest(string command) {
		JSONNode json = JSON.Parse(command);
		string currency = json["type"];
		string location = json["location"];

		if(location.Equals("current")) {
			Room room = cameraMgr.GetCurrentViewpoint().relatedZones[0];
			room.GetApplianceWithAffordance(room.avatarAffordanceSwitchLight).AddHarvest();
		}
	}

	//
	public override void ControlAvatar(string id, string action, Vector3 pos) {
		_avatarMgr.ControlAvatar(id, action, pos);
	}

	//
	public override void ControlAvatar(string id, UnityEngine.Object action) {
		_avatarMgr.ControlAvatar(id, action);
	}

	//
	public override void UnlockView(int x, int y) {
		cameraMgr.UnlockView(x, y);
		view.UpdateViewpointGuide(cameraMgr.GetViewpoints(), cameraMgr.GetCurrentViewpoint());
	}

	//
	public override void ShowCongratualations(string text) {
		_audioMgr.PlaySound("Fireworks");
		view.ShowCongratualations(_localMgr.GetPhrase(text));
	}

	//
	public override void ClearView() {
		view.ClearView();
	}
	#endregion

	//
	public string GetPhrase(string groupKey, string key, int index) {
		return _localMgr.GetPhrase(groupKey, key, index);
	}

	//
	public override void SetTimeScale(int timeScale) {
		_timeMgr.VisualTimeScale = timeScale;
	}

	//
	public void SetTimeScale(float timeScale) {
		_timeMgr.SimulationTimeScaleFactor = timeScale;
	}

	//
	public void StepTimeForward(int days) {
		_timeMgr.Offset(86400 * days);
	}

	//
	public override string GetCurrentDate() {
		return _timeMgr.CurrentDate;
	}	

	//
	public override void SaveGameState() {
		if(debug) { Debug.Log("Saving game state"); }

		_saveMgr.SetCurrentSlotClass("ResourceManager", _resourceMgr.SerializeAsJSON());
		_saveMgr.SetCurrentSlotClass("NarrationManager", _narrationMgr.SerializeAsJSON());
		_saveMgr.SetCurrentSlotClass("LocalisationManager", _localMgr.SerializeAsJSON());
		_saveMgr.SetCurrentSlotClass("CameraManager", cameraMgr.SerializeAsJSON());
		_saveMgr.SetCurrentSlotClass("AvatarManager", _avatarMgr.SerializeAsJSON());

		//Save appliance states
		JSONArray applianceJSON = new JSONArray();
		foreach(Appliance appliance in _appliances) {
			applianceJSON.Add(appliance.SerializeAsJSON());
		}
		_saveMgr.SetCurrentSlotArray("Appliances", applianceJSON);

		_saveMgr.SetCurrentSlotData("lastTime", _timeMgr.time.ToString());
		_saveMgr.SetCurrentSlotData("curPilot", Application.loadedLevelName);

		_saveMgr.SaveCurrentSlot();
	}

	//
	public override void LoadGameState() {
		if(!enableSaveLoad) {
			if(debug) { Debug.Log("save/load disabled. Will not load game data."); }
			return;
		}
		if(debug) { Debug.Log("Loading game state"); }

		_saveMgr.LoadCurrentSlot();

		_resourceMgr.DeserializeFromJSON(_saveMgr.GetCurrentSlotClass("ResourceManager"));
		_narrationMgr.DeserializeFromJSON(_saveMgr.GetCurrentSlotClass("NarrationManager"));
		_localMgr.DeserializeFromJSON(_saveMgr.GetCurrentSlotClass("LocalisationManager"));
		cameraMgr.DeserializeFromJSON(_saveMgr.GetCurrentSlotClass("CameraManager"));
		_avatarMgr.DeserializeFromJSON(_saveMgr.GetCurrentSlotClass("AvatarManager"));

		//Load appliance states
		if(_saveMgr.GetCurrentSlotData("Appliances") != null) {
			JSONArray appsJSON = _saveMgr.GetCurrentSlotData("Appliances").AsArray;
			foreach(JSONClass appJSON in appsJSON) {
				foreach(Appliance app in _appliances) {
					if(app.GetComponent<UniqueId>().uniqueId.Equals(appJSON["id"])) {
						app.DeserializeFromJSON(appJSON);
					}
				}
			}
		}

		if(_saveMgr.GetCurrentSlotData("lastTime") != null) {
			_timeMgr.SetTime(_saveMgr.GetCurrentSlotData("lastTime").AsDouble);
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
		SaveGameState();
		_sceneMgr.LoadScene(scene);
	}

	//
	public void ShowCongratulations(string text) {
		cameraMgr.PlayFireworks();
		view.ShowCongratualations(text);
	}

	//
	public void MoveCamera(string animation) {
		cameraMgr.PlayAnimation(animation);
	}

	public void StopCamera() {
		cameraMgr.StopAnimation();
	}

	public void OnAnimationEvent(string animationEvent) {
		_narrationMgr.OnQuestEvent(Quest.QuestEvent.CameraAnimationEvent, animationEvent);
	}

}
