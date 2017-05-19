using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using SimpleJSON;
using System;

public class PilotController : MonoBehaviour, NarrationInterface, AudioInterface, CameraInterface, ResourceInterface {
	//Singleton features
	private static PilotController _instance;
	public static PilotController GetInstance() {
		return _instance as PilotController;
	}
	public enum InputState {
		ALL, ONLY_PROMPT, ONLY_SWIPE, ONLY_TAP, ONLY_APPLIANCE_SELECT, ONLY_APPLIANCE_DESELECT, ONLY_APOCALYPSE, ONLY_ENVELOPE,
		ONLY_TIME, ONLY_OPEN_INBOX, ONLY_CLOSE_INBOX, ONLY_ENERGY, ONLY_COMFORT, ONLY_SWITCH_LIGHT, ONLY_APPLY_EEM, ONLY_HARVEST,
		NOTHING, ONLY_CLOSE_MAIL, ONLY_SELECT_OVERVIEW, ONLY_SELECT_GRIDVIEW
	};
	[SerializeField]
	private InputState _curState = InputState.ALL;

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
	public const float SWIPE_THRESH = 200;

	[Header("Save between sessions")]
	public bool syncTime;
	public bool syncPilot;
	public bool syncCamera;
	public bool syncResources;
	public bool syncNarrative;
	public bool syncLocalization;
	public bool syncAvatars;
	public bool syncAppliances;

	//Event flow state fields
	private int _pendingTimeSkip = -1;

	private bool _firstUpdate = false;
	#endregion

	//Called once on all behaviours before any Start call
	void Awake() {
		_instance = this;
	}

	//Called once on all behaviours before any Update call
	void Start() {
		_view = PilotView.GetInstance();
		_view.SetController(this);

		_timeMgr = GameTime.GetInstance();

		_audioMgr = AudioManager.GetInstance();
		_audioMgr.SetInterface(this);

		_mainMeter = MainMeter.GetInstance();

		_resourceMgr = ResourceManager.GetInstance();
		_resourceMgr.SetInterface(this);

		_narrationMgr = NarrationManager.GetInstance();
		_narrationMgr.SetInterface(this);

		_sceneMgr = CustomSceneManager.GetInstance();
		_saveMgr = SaveManager.GetInstance();
		_localMgr = LocalisationManager.GetInstance();

		_cameraMgr = CameraManager.GetInstance();
		_cameraMgr.SetInterface(this);

		_avatarMgr = AvatarManager.GetInstance();
		_applianceMgr = ApplianceManager.GetInstance();

		_instance._view.ClearView();
		_view.TranslateInterface();

		playPeriod = endTime - startTime;
	}

	//Called every frame
	void Update() {
		if(!_firstUpdate) {
			_firstUpdate = true;
			_instance.LoadGameState();

			if(_saveMgr.GetClass("battleReport") != null) {
				string loserName = _saveMgr.GetClass("battleReport")["avatar"];
				Debug.Log("battle won over " + loserName);
				_instance._narrationMgr.OnNarrativeEvent("BattleWon", loserName);
			}
		}

		UpdateTouch();
		UpdatePinch();

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
		_view.UpdateQuestCount(_narrationMgr.active.Count);

		_view.UpdateTime((float)((_timeMgr.time - startTime) / playPeriod));
		if(_timeMgr.time > endTime) {
			OnGameOver();
		}
	}

	//Updates basic onStart, onTouch, onEnd, tap, double tap and swipe interaction
	private void UpdateTouch() {
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

			//Delay tap for possibility to double tap
			if(_touchState == TAP) {
				_doubleTimer += Time.unscaledDeltaTime;
				if(_doubleTimer > D_TAP_TIMEOUT) {
					OnTap(_startPos);
					ResetTouch();
				}
			}
		} else {
			_touchReset = false;
		}
	}

	//New touch started
	private void OnTouchStart(Vector3 pos) {
		if(_touchState == IDLE) {
			_touchTimer = 0;
			_startPos = pos;
		}
	}

	//Touch ongoing
	private void OnTouch(Vector3 pos) {
		_touchTimer += Time.unscaledDeltaTime;
	}

	//Touch ended
	private void OnTouchEnded(Vector3 pos) {
		float dist = Vector3.Distance(_startPos, pos);
		if(!_touchReset) {
			if(_touchTimer < TAP_TIMEOUT && dist < SWIPE_THRESH) {
				if(_touchState == IDLE) {
					//First tap, start double tap timer
					_touchState = TAP;
					_touchTimer = 0;
				} else if(_touchState == TAP) {
					//Second tap before double tap timer ran out, trigger double tap
					OnDoubleTap(pos);
					ResetTouch();
				}
			} else if(dist >= SWIPE_THRESH) {
				//Swipe distance greater than threshold, trigger swipe
				OnSwipe(_startPos, pos);
				ResetTouch();
			}
		}
	}

	//Callback for when tap is triggered
	private void OnTap(Vector3 pos) {
		if(_curState != InputState.ALL && _curState != InputState.ONLY_TAP) { return; }

		if((_view.GetCurrentUIKey() == "Character Panel" || _view.GetCurrentUIKey() == "Device Panel") &&
			pos.x < Screen.width / 2) {
			ClearView();
		}

		_touchState = IDLE;
		_instance._narrationMgr.OnNarrativeEvent("Tapped");
	}

	//Callback for when double tap triggered
	private void OnDoubleTap(Vector3 pos) {
		_touchState = IDLE;
		_instance._narrationMgr.OnNarrativeEvent("DoubleTapped");
	}

	//Callback for when swipe triggered
	private void OnSwipe(Vector3 start, Vector3 end) {
		if(_view.IsAnyOverlayActive()) {
			return;
		}

		if(_curState == InputState.ALL || _curState == InputState.ONLY_SWIPE) {
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

			_instance._narrationMgr.OnNarrativeEvent("Swiped");
			_instance._narrationMgr.OnNarrativeEvent("SelectedView", _cameraMgr.GetCurrentViewpoint().title);
		}
		_touchState = IDLE;
	}

	//
	public void OnPinchIn() {
		if(_curState == InputState.ALL) {
			_cameraMgr.GotoUpperView();
		}
	}

	//
	public void OnPinchOut() {
		if(_curState == InputState.ALL) {
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
		//_startPos = Input.mousePosition;
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
	public void ToggleMenu() {
		_instance._view.ToggleMenu();
	}

	//Open inspector with details of appliance
	public void SetCurrentUI(Appliance app) {
		if(_curState != InputState.ALL && _curState != InputState.ONLY_APPLIANCE_SELECT) { return; }

		_instance._cameraMgr.SetLookAtTarget(app);

		if(app.GetComponent<BehaviourAI>()) {
			_instance._view.BuildAvatarPanel(app);
			SetCurrentUI(_instance._view.GetUIPanel("Character Panel"));
			_instance._narrationMgr.OnNarrativeEvent("AvatarSelected", app.title);
		} else {
			_instance._view.BuildDevicePanel(app);
			SetCurrentUI(_instance._view.GetUIPanel("Device Panel"));
			_instance._narrationMgr.OnNarrativeEvent("DeviceSelected", app.title);
		}
	}

	//Open user interface
	public void SetCurrentUI(RectTransform ui) {
		if(_instance._curState != InputState.ALL && _instance._curState != ui.GetComponent<UIPanel>().relatedAction) {
			return;
		}

		if(!ui) {
			CloseUI(ui);
		} else if(ui.name == _instance._view.GetCurrentUIKey()) {
			CloseUI(ui);
		} else {
			if(_instance._view.GetCurrentUIKey() != "") {
				CloseUI(_instance._view.GetCurrentUI());
			}
			OpenUI(ui);
		}

		_instance.ResetTouch();
	}

	//
	public void OpenUI(RectTransform ui) {
		if(_instance._curState != InputState.ALL && _instance._curState != ui.GetComponent<UIPanel>().relatedAction) {
			return;
		}

		_instance._view.SetUIPanel(ui.name);
		switch(ui.name) {
			case "Comfort Panel":
				break;
			case "Energy Panel":
				_instance._view.BuildEnergyPanel(_instance._cameraMgr.GetCurrentViewpoint().GetElectricDevices());
				break;
			case "Inbox":
				_instance._view.BuildInbox(_instance._narrationMgr.active, _instance._narrationMgr.archive);
				break;
			case "Apocalypse Panel":
				break;
			case "Building Panel":
				break;
			case "Character Panel":
				break;
			case "Device Panel":
				break;
			case "Tim Control Panel":
				break;
		}
		_instance._narrationMgr.OnNarrativeEvent(ui.name + "Opened");
		_instance.ResetTouch();
	}

	//
	public void CloseUI(RectTransform ui) {
		if(!ui) { return; }

		if(_instance._curState != InputState.ALL && _instance._curState != ui.GetComponent<UIPanel>().relatedAction) {
			return;
		}

		_instance._view.SetUIPanel("");
		
		switch(ui.name) {
			case "Comfort Panel":
				break;
			case "Energy Panel":
				_instance._view.BuildEnergyPanel(_instance._cameraMgr.GetCurrentViewpoint().GetElectricDevices());
				break;
			case "Inbox":
				_instance._view.BuildInbox(_instance._narrationMgr.active, _instance._narrationMgr.archive);
				break;
			case "Apocalypse Panel":
				break;
			case "Building Panel":
				break;
			case "Character Panel":
				_instance._cameraMgr.ClearLookAtTarget();
				_instance._view._characterPanel.OnClose();
				break;
			case "Device Panel":
				_instance._cameraMgr.ClearLookAtTarget();
				_instance._view._devicePanel.OnClose();
				break;
			case "Time Control Panel":
				break;
		}
		_instance._narrationMgr.OnNarrativeEvent(ui.name + "Closed");
		_instance.ResetTouch();
	}

	//Hide any open user interface
	public void ClearView() {
		CloseUI(_instance._view.GetCurrentUI());

		_instance._view.SetUIPanel("");
		_instance._view.ClearView();
		_instance._cameraMgr.ClearLookAtTarget();
		_instance._view.HideMessage();
	}

	//Request narration event for current view
	public void RequestCurrentView() {
		_instance._narrationMgr.OnNarrativeEvent("SelectedView", _cameraMgr.GetCurrentViewpoint().title);
	}

	//
	public void OnHarvest(Gem gem) {
		//_tempInstance._resourceMgr.AddComfort(gem.value);
		_instance._view.CreateFeedback(gem.transform.position, "+" + gem.value);
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
		//_instance._narrationMgr.OnQuestEvent(Quest.QuestEvent.ResourceHarvested, "satisfaction");
		_instance._narrationMgr.OnNarrativeEvent("ResourceHarvested", "Satisfaction");
	}

	//
	public void OnElectricMeterToggle(ElectricMeter meter) {
		if(_instance._curState != InputState.ALL && _instance._curState != InputState.ONLY_SWITCH_LIGHT) { return; }

		meter.Toggle();

		Appliance appliance = meter.gameObject.GetComponent<Appliance>();
        if (appliance) {
            Room zone = appliance.GetZone();

            //If appliance is light switch
            foreach (Appliance.AffordanceResource affordanceSlot in appliance.avatarAffordances) {
                if (affordanceSlot.affordance == zone.avatarAffordanceSwitchLight) {
                    _avatarMgr.OnLightToggled(zone, meter.GivesPower);

                    if (meter.GivesPower) {
                        //_narrationMgr.OnQuestEvent(Quest.QuestEvent.LightSwitchedOn);
						_narrationMgr.OnNarrativeEvent("TurnedOnLamp");
                    }
                    else {
                        //_narrationMgr.OnQuestEvent(Quest.QuestEvent.LightSwitchedOff);
						_narrationMgr.OnNarrativeEvent("TurnedOffLamp");
					}
                }
            }

        }
	}

	//
	public void OnNewViewpoint(Viewpoint curView, Viewpoint[][] viewMatrix, bool overview) {
		_instance._view.UpdateViewpointGuide(_instance._cameraMgr.GetViewpoints(), curView, overview);
		if(_cameraMgr.cameraState == CameraManager.CameraState.PlayerControl) {
			_instance._view.UpdateViewpointTitle(curView);
		}

		//Debug.Log("SelectedView:" + curView.title);
		_instance._narrationMgr.OnNarrativeEvent("SelectedView", curView.title);
	}

	//
	public void OnCameraArrived(Viewpoint viewpoint) {
		_instance._narrationMgr.OnNarrativeEvent("CameraArrived", viewpoint.title);
	}

	//
	public void ApplyEEM(Appliance appliance, EnergyEfficiencyMeasure eem) {
		ResetTouch();
		if(_curState != InputState.ALL && _curState != InputState.ONLY_APPLY_EEM) { return; }

		if(eem.IsAffordable(_resourceMgr.cash, _resourceMgr.comfort) && !appliance.IsEEMApplied(eem)) {
			_resourceMgr.cash -= eem.cashCost;
			_resourceMgr.comfort -= eem.comfortCost;

			if(eem.callback != "") {
				//_instance.SendMessage(eem.callback, eem.callbackArgument);
				if (eem.callbackAffordance) {
					appliance.SendMessage(eem.callback, eem.callbackAffordance);
				} else {
					appliance.SendMessage(eem.callback, eem.callbackArgument);
				}
			}
			GameObject returnedGO = appliance.ApplyEEM(eem);

            //Redraw device panel
            _view.BuildDevicePanel(returnedGO.GetComponent<Appliance>());

			_instance._narrationMgr.OnNarrativeEvent("EEMPerformed", eem.name);
		}
	}

	//
	public void OnAvatarReachedPosition(BehaviourAI avatar, Vector3 pos) {
		//Debug.Log("OnAvatarReachedPosition");
		//_narrationMgr.OnQuestEvent(Quest.QuestEvent.AvatarArrived);
		_instance._narrationMgr.OnNarrativeEvent("AvatarArrived");
		avatar.ReleaseControlOfAvatar();
	}

	//
	public void OnAvatarSessionComplete(string activityState) {
		_instance._narrationMgr.OnNarrativeEvent("SessionOver", activityState);
		//_narrationMgr.OnQuestEvent(Quest.QuestEvent.AvatarSessionOver, activityState);
	}

	//
	public void OnAvatarActivityComplete(string activity) {
		if(_instance._narrationMgr != null) {
			_instance._narrationMgr.OnNarrativeEvent("ActivityOver", activity);
		}
	}

	//
	public void LimitInteraction(string interactionLimit) {
		switch(interactionLimit) {
			case "all": _curState = InputState.ALL; break;
			case "only_ok": _curState = InputState.ONLY_PROMPT; break;
			case "only_swipe": _curState = InputState.ONLY_SWIPE; break;
			case "only_tap": _curState = InputState.ONLY_TAP; break;
			case "only_apocalypsometer": _curState = InputState.ONLY_APOCALYPSE; break;
			case "only_energy_panel": _curState = InputState.ONLY_ENERGY; break;
			default: _curState = InputState.ALL; break;
		}
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
	public void ShowMessage(string[] cmd) {
		string avatarName = cmd[0];
		JSONNode json = JSON.Parse(cmd[1]);
		string group = json["g"];
		string key = json["k"];

		Sprite portrait = null;
		if(avatarName != "") {
			if(avatarName == "player") {
				portrait = _instance._avatarMgr.playerPortrait;
				avatarName = "F. Shaman: ";
			} else {
				portrait = _instance._avatarMgr.GetAvatar(avatarName).portrait;
				avatarName = avatarName + ": ";
			}
		}
		string message = GetPhrase(group, key);

		_instance._view.ShowMessage(avatarName + message, portrait, true, false);
	}

	//
	public void ShowPrompt(string[] cmd) {
		string avatarName = cmd[0];
		JSONNode json = JSON.Parse(cmd[1]);
		string group = json["g"];
		string key = json["k"];

		Sprite portrait = null;
		if(avatarName != "") {
			if(avatarName == "player") {
				portrait = _instance._avatarMgr.playerPortrait;
				avatarName = "F. Shaman: ";
			} else {
				portrait = _instance._avatarMgr.GetAvatar(avatarName).portrait;
				avatarName = avatarName + ": ";
			}
		}
		string message = GetPhrase(group, key);

		_instance._view.ShowMessage(avatarName + message, portrait, true, true);

		LimitInteraction("only_ok");
	}

	//
	public void OnOkPressed() {
		_instance.ResetTouch();

		if(_instance._curState == InputState.ALL || _instance._curState == InputState.ONLY_PROMPT) {
			_instance._view.messageUI.SetActive(false);

			_instance.LimitInteraction("all");
			_instance._narrationMgr.OnNarrativeEvent("OKPressed");
		}

		_instance.PlaySound("Press Button");
	}

	//
	public void ShowCongratulations(string[] cmd) {
		JSONNode json = JSON.Parse(cmd[1]);
		string group = json["g"];
		string key = json["k"];

		_instance._audioMgr.PlaySound("Fireworks");
		_instance._view.ShowCongratulations(_instance._localMgr.GetPhrase(group, key));
		_instance._cameraMgr.PlayFireworks();

		LimitInteraction("only_tap");
	}

    //
    public void MarkAvatar(string cmd) {
        Appliance app = _avatarMgr.GetAvatar(cmd).GetComponent<Appliance>();
        GameObject ip = Instantiate(_narrationMgr.interactionPoint, app.transform);
        ip.GetComponentInChildren<NarrativeInteractionPoint>().app = app;

        float y = 0.1f + Helpers.LargestBounds(app.transform).max.y;

        ip.transform.localPosition = new Vector3(0.0f, y + 0.25f, 0.0f);
    }

    //
    public void UnmarkAvatar(string cmd) {
        Destroy(_avatarMgr.GetAvatar(cmd).GetComponentInChildren<NarrativeInteractionPoint>().gameObject);
    }

    //
    public void MarkDevice(string cmd) {
        foreach (Appliance app in _applianceMgr.GetAppliances().FindAll(x => x.title == cmd)) {
            GameObject ip = Instantiate(_narrationMgr.interactionPoint, app.transform);
            ip.GetComponentInChildren<NarrativeInteractionPoint>().app = app;

            float y = 0.1f + Helpers.LargestBounds(app.transform).max.y;

            ip.transform.localPosition = new Vector3(0.0f, y + 0.25f, 0.0f);
        }
    }

    //
    public void UnmarkDevice(string cmd) {
        foreach (Appliance app in _applianceMgr.GetAppliances().FindAll(x => x.title == cmd)) {
            Destroy(app.GetComponentInChildren<NarrativeInteractionPoint>().gameObject);
        }
    }

    //
    public void AddObjective(string cmd) {
		JSONNode json = JSON.Parse(cmd);
	}

	//
	public void UnlockView(int x, int y) {
		_cameraMgr.UnlockView(x, y);
		_view.UpdateViewpointGuide(_cameraMgr.GetViewpoints(), _cameraMgr.GetCurrentViewpoint());
	}

	//
	public void ChallengeAvatar(Appliance app) {
		JSONClass challengeData = app.GetComponent<AvatarStats>().SerializeAsJSON();
		_instance._saveMgr.SetClass("pendingChallenge", challengeData);
		_instance.SaveGameState();
		_instance.LoadScene("BattleScene");
	}

	//
	public string GetPhrase(string groupKey, string key, int index) {
		return _localMgr.GetPhrase(groupKey, key, index);
	}

	//
	public string GetPhrase(string groupKey, string key) {
		return _instance._localMgr.GetPhrase(groupKey, key);
	}

	//
	public void SetVisualTimeScale(int timeScale) {
		_instance._timeMgr.VisualTimeScale = timeScale;
	}


	public void SetSimulationTimeScale(float timeScale) {
		_instance._timeMgr.SimulationTimeScaleFactor = timeScale;
	}

	//
	public void SetSimulationTimeScale(string timeScale) {
		SetSimulationTimeScale(float.Parse(timeScale));
	}

	//
	public void StepHoursForward(int hours) {
		PlayUIAnimation("TimeSkipped");
		SetVisualTimeScale(10);
		switch(hours) {
			case 1:
				SetSimulationTimeScale(100);
				break;
			case 24:
				SetSimulationTimeScale(1000);
				break;
			case 168:
				SetSimulationTimeScale(100000);
				break;
			case 5040:
				SetSimulationTimeScale(100000);
				break;
		}
		_instance._pendingTimeSkip = hours;
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

		if(syncTime) _saveMgr.SetCurrentSlotData("lastTime", _timeMgr.offset.ToString());
		if(syncPilot) _saveMgr.SetCurrentSlotData("curPilot", Application.loadedLevelName);

		_saveMgr.SaveCurrentSlot();
		PlayUIAnimation("GameSaved");
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
		_instance._view.UpdateViewpointTitle("");
		_instance._cameraMgr.PlayAnimation(animation);
	}

	//
	public void GotoViewpoint(string title) {
		Viewpoint[][] vps = _instance._cameraMgr.GetViewpoints();
		for(int y = 0; y < vps.Length; y++) {
			for(int x = 0; x < vps[y].Length; x++) {
				if(vps[y][x].title == title) {
					_instance._cameraMgr.SetViewpoint(x, y, Vector2.zero);
				}
			}
		}
	}

	//
	public void StopCamera() {
		_instance._view.UpdateViewpointTitle(_instance._cameraMgr.GetCurrentViewpoint());
		_instance._cameraMgr.StopAnimation();
	}

	//
	public void OnAnimationEvent(string animationEvent) {
		_narrationMgr.OnNarrativeEvent("AnimationEvent", animationEvent);
		switch(animationEvent) {
			case "timeSkippedOver":
				_instance._timeMgr.Offset(3600 * _instance._pendingTimeSkip);
				SetVisualTimeScale(1);
				SetSimulationTimeScale(60);
				Debug.Log("time skip over");
				break;
		}
	}

	//Callback from resouce manager for when monthly salary received
	public void OnResourcesReceived(string data) {
		PlayUIAnimation("CashReceived");
	}

	//
	public void OnGameOver() {
		LoadScene("MenuScene");
	}

	//
	public void OnNarrativeAction(Narrative narrative, Narrative.Step step, string callback, string[] parameters) {
		switch(callback) {
			case "ShowMessage":
			case "ShowPrompt":
			case "ShowCongratulations":
				parameters[1] = "{g:\"Narrative." + narrative.title + "\", k:\"" + step.description + "\"}";
				SendMessage(callback, parameters);
				break;
			default:
				SendMessage(callback, parameters[0]);
				break;
		}
	}

	//Reset interaction limitations and save on narrative completion
	public void OnNarrativeCompleted(Narrative narrative) {
		_instance.LimitInteraction("all");
		_instance.SaveGameState();
	}

	//TODO
	public void OnNarrativeActivated(Narrative narrative) {
	}
}
