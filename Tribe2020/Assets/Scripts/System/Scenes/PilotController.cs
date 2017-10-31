using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using SimpleJSON;
using System;
using UnityEngine.SceneManagement;

public class PilotController : MonoBehaviour, NarrationInterface, AudioInterface, CameraInterface, ResourceInterface, InteractionListener {
	//Singleton features
	private static PilotController _instance;
	public static PilotController GetInstance() {
		return _instance as PilotController;
	}
	public enum InputState {
		ALL, ONLY_PROMPT, ONLY_SWIPE, ONLY_TAP, ONLY_APPLIANCE_SELECT, ONLY_APPLIANCE_DESELECT, ONLY_APOCALYPSE, ONLY_ENVELOPE,
		ONLY_TIME, InboxOnly, ONLY_CLOSE_INBOX, ONLY_ENERGY, ONLY_COMFORT, ONLY_SWITCH_LIGHT, ONLY_APPLY_EEM, ONLY_HARVEST,
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
	private InteractionManager _interMgr;
	private MonitorManager _monitorMgr;

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
    //private double _skipToOffset = -1;

	private bool _firstUpdate = false;

    //private UniqueId[] uniqueIds;
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

		_interMgr = InteractionManager.GetInstance();
		_interMgr.SetListener(this);

		_monitorMgr = MonitorManager.GetInstance();

		_view.ClearView();

		playPeriod = endTime - startTime;
	}

	//Called every frame
	void Update() {
		if(!_firstUpdate) {
			_firstUpdate = true;
			_instance.LoadGameState();
			_view.TranslateInterface();
			//Fire empty event to activate active narratives
			_instance._narrationMgr.OnNarrativeEvent();

			if(_saveMgr.GetClass("battleReport") != null) {
				string loserName = _saveMgr.GetClass("battleReport")["avatar"];
				//Debug.Log("battle won over " + loserName);
				_instance._narrationMgr.OnNarrativeEvent("BattleWon", loserName);
			}
		}

		_view.date.GetComponent<Text>().text = _timeMgr.GetTimeWithFormat("HH:mm ddd d MMM yyyy");
		if (_timeMgr.RedLetterDay)
			_view.date.GetComponent<Text> ().color = Color.red;
		else
			_view.date.GetComponent<Text> ().color = Color.white;
		
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
		_view.UpdateQuestCount(_narrationMgr.GetNumberOfActiveChecklists());

		_view.UpdateTime((float)((_timeMgr.time - startTime) / playPeriod));
		if(_timeMgr.time > endTime) {
			OnGameOver();
		}

        //Skip forward in time, animation check
        if (_instance._timeMgr._skipToOffset != -1 && _instance._timeMgr.offset > _instance._timeMgr._skipToOffset) {
            FinishStepHoursForward();
        }
	}

	//Callback for when tap is triggered
	public void OnTap(Vector3 pos) {
		if(_curState != InputState.ALL && _curState != InputState.ONLY_TAP) { return; }

		if(_view.victoryUI.activeSelf) {
			ClearView();
			LimitInteraction("all");
		}

		_instance._narrationMgr.OnNarrativeEvent("Tapped");
	}

	//Callback for when double tap triggered
	public void OnSwipe(Vector2 direction) {
		//No swiping between rooms while an ui is open
		if(_view.IsAnyOverlayActive()) {
			return;
		}
		//If action not limited, change room according to swipe direction
		if(_curState == InputState.ALL || _curState == InputState.ONLY_SWIPE) {
			if(direction == Vector2.right) {
				_cameraMgr.GotoLeftView();
			} else if(direction == Vector2.up) {
				_cameraMgr.GotoLowerView();
			} else if(direction == Vector2.left) {
				_cameraMgr.GotoRightView();
			} else if(direction == Vector2.down) {
				_cameraMgr.GotoUpperView();
			}
			//Send narrative events
			_instance._narrationMgr.OnNarrativeEvent("Swiped");
			_instance._narrationMgr.OnNarrativeEvent("SelectedView", _cameraMgr.GetCurrentViewpoint().title);
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
		//_instance.ResetTouch();
	}

	//Open user interface
	public void SetCurrentUI(RectTransform ui) {
		if(_instance._curState != InputState.ALL && _instance._curState != ui.GetComponent<UIPanel>().relatedAction) {
			return;
		}

		//Close give ui if null or already open
		if(!ui || ui.name == _instance._view.GetCurrentUIKey()) {
			CloseUI(ui);
		} else {
			//Close current ui and open new ui
			CloseUI(_instance._view.GetCurrentUI());
			OpenUI(ui);
		}

		//_instance.ResetTouch();
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
				_instance._view.BuildPilotPanel();
				break;
			case "Character Panel":
				break;
			case "Device Panel":
				break;
			case "Tim Control Panel":
				break;
		}
		_instance._narrationMgr.OnNarrativeEvent(ui.name + "Opened");
		//_instance.ResetTouch();
	}

	//
	public void CloseUI(RectTransform ui) {
		if(!ui || (_instance._curState != InputState.ALL && _instance._curState != ui.GetComponent<UIPanel>().relatedAction)) {
			return;
		}

		_instance._view.SetUIPanel("");
		switch(ui.name) {
			case "Comfort Panel":
				break;
			case "Energy Panel":
				break;
			case "Inbox":
				_instance._view.DestroyInbox();
				break;
			case "Apocalypse Panel":
				break;
			case "Building Panel":
				break;
			case "Character Panel":
				_instance._cameraMgr.ClearLookAtTarget();
				break;
			case "Device Panel":
				_instance._cameraMgr.ClearLookAtTarget();
				break;
			case "Time Control Panel":
				break;
		}
		_instance._narrationMgr.OnNarrativeEvent(ui.name + "Closed");
		//_instance.ResetTouch();
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

	//Resource crystal was tapped
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

	//Light switch was tapped
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
	public void ApplyEEM(Appliance app, EnergyEfficiencyMeasure eem) {
		if(_curState != InputState.ALL && _curState != InputState.ONLY_APPLY_EEM) { return; }

		if(eem.IsAffordable(_resourceMgr.cash, _resourceMgr.comfort) && !app.IsEEMApplied(eem)) {
			_resourceMgr.cash -= eem.cashCost;
			_instance._narrationMgr.OnNarrativeEvent("MoneyChanged", "" + _resourceMgr.cash);

			_resourceMgr.comfort -= eem.comfortCost;
			_instance._narrationMgr.OnNarrativeEvent("ComfortChanged", "" + _resourceMgr.comfort);

			_instance._monitorMgr.Publish("event", "event", "MeasureApplied", "description", eem.name + ":" + eem.title);

			if(eem.callback != "") {
				//_instance.SendMessage(eem.callback, eem.callbackArgument);
				if (eem.callbackAffordance) {
					app.SendMessage(eem.callback, eem.callbackAffordance);
				} else {
					app.SendMessage(eem.callback, eem.callbackArgument);
				}
			}
			GameObject returnedGO = app.ApplyEEM(eem);
			Appliance returnApp = returnedGO.GetComponent<Appliance>();

			//Redraw device panel
			if(_view.GetCurrentUI().GetComponent<UIPanel>().title == "Device") {
				_view.BuildDevicePanel(returnApp);
			} else if(_view.GetCurrentUI().GetComponent<UIPanel>().title == "Character") {
				_view.BuildAvatarPanel(returnApp);
			} else if(_view.GetCurrentUI().GetComponent<UIPanel>().title == "Building") {
				_view.BuildPilotPanel();
			}

			_instance._narrationMgr.OnNarrativeEvent("EEMPerformed", eem.name, true);
			_instance._monitorMgr.Publish("event", "event", eem.name);
		}
	}

	//
	public Appliance GetPilotAppliance() {
		return _applianceMgr.pilotAppliance;
	}

	//
	public void AddMoney(float money) {
		_resourceMgr.cash += money;
		_instance._narrationMgr.OnNarrativeEvent("MoneyIsEqualTo", "" + _resourceMgr.cash);
	}

	//
	public void AddComfort(float comfort) {
		_resourceMgr.comfort += comfort;
		_instance._narrationMgr.OnNarrativeEvent("ComfortIsEqualTo", "" + _resourceMgr.comfort);
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
			case "Inbox_Only": _curState = (InputState) Enum.Parse(typeof(InputState), interactionLimit, true); break;
			default: _curState = InputState.ALL; break;
		}
	}

	//
	public void PlaySound(string sound) {
		_instance._audioMgr.PlaySound(sound);
	}

	//
	public void StopSound(string sound) {
		_instance._audioMgr.StopSound(sound);
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
		string[] avatarParse = cmd[0].Split(',');
		string avatarName = avatarParse[0];
		AvatarManager.Mood avatarMood = AvatarManager.Mood.Happy;
		if(avatarParse.Length > 1) { avatarMood = (AvatarManager.Mood) Enum.Parse(typeof(AvatarManager.Mood), avatarParse[1]); }
		JSONNode json = JSON.Parse(cmd[1]);
		string group = json["g"];
		string key = json["k"];

		AvatarModel avatarModel = null;
		if(avatarName != "") {
			
			if(avatarName == "player") { avatarName = "F. Shaman"; }
			avatarModel = _instance._avatarMgr.GetAvatarModel(avatarName);
			avatarName = avatarName + ": ";
		}
		string message = GetPhrase(group, key);

		_instance._view.ShowMessage(avatarName + message, avatarModel, avatarMood, false);
	}

	//
	public void ShowPrompt(string[] cmd) {
		string[] avatarParse = cmd[0].Split(',');
		string avatarName = avatarParse[0];
		AvatarManager.Mood avatarMood = AvatarManager.Mood.Happy;
		if(avatarParse.Length > 1) { avatarMood = (AvatarManager.Mood)Enum.Parse(typeof(AvatarManager.Mood), avatarParse[1]); }
		JSONNode json = JSON.Parse(cmd[1]);
		string group = json["g"];
		string key = json["k"];

		AvatarModel avatarModel = null;
		if(avatarName != "") {
			if(avatarName == "player") { avatarName = "F. Shaman"; }
			avatarModel = _instance._avatarMgr.GetAvatarModel(avatarName);
			avatarName = avatarName + ": ";
		}
		string message = GetPhrase(group, key);

		_instance._view.ShowMessage(avatarName + message, avatarModel, avatarMood, true);
		LimitInteraction("only_ok");
	}

	//
	public void OnOkPressed() {
		_instance._interMgr.ResetTouch();

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
		if(!_avatarMgr.GetAvatar(cmd).GetComponentInChildren<NarrativeInteractionPoint>()) {
			return;
		}

		Destroy(_avatarMgr.GetAvatar(cmd).GetComponentInChildren<NarrativeInteractionPoint>().gameObject);
    }

    //
    public void SetAvatarBattleReady(string cmd) {
        _avatarMgr.GetAvatar(cmd).GetComponent<BehaviourAI>().battleReady = true;
    }

    //[unique id; children appliance title]
	public void MarkDevice(string[] cmd) {
		//cmd[0] = uid, acquire appliance for given uid
		Appliance app = _applianceMgr.GetAppliance(cmd[0]);  

		if (cmd[1] == "") {
			MarkAppliance(app);
		} else {
			List<Appliance> children = new List<Appliance>(app.GetComponentsInChildren<Appliance>());
			foreach (Appliance child in children.FindAll(x => x.title == cmd[1])) {
				MarkAppliance(child);
			}
		}
	}

	//
    public void MarkDevices(string cmd) {
        foreach (Appliance app in _applianceMgr.GetAppliances().FindAll(x => x.title == cmd)) {
            MarkAppliance(app);
        }
    }

	//
    void MarkAppliance(Appliance app) {
        GameObject ip = Instantiate(_narrationMgr.interactionPoint, app.transform);
        ip.GetComponentInChildren<NarrativeInteractionPoint>().app = app;
        Bounds bounds = Helpers.LargestBounds(app.transform);
        ip.transform.localPosition = Vector3.zero;
    }

    //
    public void UnmarkDevice(string[] cmd) {
		//cmd[0] = uid, acquire appliance for given uid
		Appliance app = _applianceMgr.GetAppliance(cmd[0]); ;

		if (app.GetComponentInChildren<NarrativeInteractionPoint>() && cmd[1] == "") {
			Destroy(app.GetComponentInChildren<NarrativeInteractionPoint>().gameObject);
		} else {
			List<Appliance> children = new List<Appliance>(app.GetComponentsInChildren<Appliance>());
			foreach (Appliance child in children.FindAll(x => x.title == cmd[1])) {
				if(child.GetComponentInChildren<NarrativeInteractionPoint>()) {
					Destroy(child.GetComponentInChildren<NarrativeInteractionPoint>().gameObject);
				}
			}
		}
    }

    //
    public void UnmarkDevices(string cmd) {
        foreach (Appliance app in _applianceMgr.GetAppliances().FindAll(x => x.title == cmd)) {
            Destroy(app.GetComponentInChildren<NarrativeInteractionPoint>().gameObject);
        }
    }

    //
    public void AddObjective(string cmd) {
		JSONNode json = JSON.Parse(cmd);
	}

	//
	public void UnlockViews(int y) {
		_cameraMgr.UnlockViews(y);
		_view.UpdateViewpointGuide(_cameraMgr.GetViewpoints(), _cameraMgr.GetCurrentViewpoint());
	}

	//
	public void UnlockView(int x, int y) {
		_cameraMgr.UnlockView(x, y);
		_view.UpdateViewpointGuide(_cameraMgr.GetViewpoints(), _cameraMgr.GetCurrentViewpoint());
	}

	//
	public void ChallengeAvatar(string avatarName) {
		ChallengeAvatar(_avatarMgr.GetAvatar(avatarName).GetComponent<Appliance>());
	}

	//
	public void ChallengeAvatar(Appliance app) {
		JSONClass challengeData = app.GetComponent<AvatarStats>().SerializeAsJSON();
		_instance._saveMgr.SetClass("pendingChallenge", challengeData);
		_instance.SaveGameState();
		_instance.LoadScene("BattleScene");
	}

	//
	public bool HasWonChallenge() {
		//Debug.Log(_saveMgr.GetClass("battleReport"));
		return _saveMgr.GetClass("battleReport") != null;
	}

	//
	public void ClearChallengeData() {
		_instance._saveMgr.RemoveClass("pendingChallenge");
		_instance._saveMgr.RemoveClass("battleReport");
		//_instance.SaveGameState();
	}

	//
	public void ControlAvatar(string[] cmd) {
		JSONNode posJSON = JSON.Parse(cmd[1]);
		Vector3 pos = new Vector3(posJSON["x"].AsFloat, posJSON["y"].AsFloat, posJSON["z"].AsFloat);
		Debug.Log(cmd[0] + " -> " + pos);
		_avatarMgr.ControlAvatar(cmd[0], pos);
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
		PlayUIAnimation("TimeSkippedStart");
		SetVisualTimeScale(10);
		switch(hours) {
			case 1:
				SetSimulationTimeScale(100000);
				break;
			case 24:
				SetSimulationTimeScale(100000);
				break;
			case 168:
				SetSimulationTimeScale(1000000);
				break;
			case 720:
				SetSimulationTimeScale(1000000);
				break;
		}
        _instance._timeMgr._skipToOffset = _instance._timeMgr.offset + 3600 * hours;
        //_instance._pendingTimeSkip = hours;
    }

    void FinishStepHoursForward() {
        SetVisualTimeScale(1);
        SetSimulationTimeScale(60);
        _instance._timeMgr.offset = _instance._timeMgr._skipToOffset;
        _instance._timeMgr._skipToOffset = -1;
        PlayUIAnimation("TimeSkippedEnd");
        
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
        if (debug) { Debug.Log("Loading game state"); }

		if(syncPilot) _saveMgr.LoadCurrentSlot();

        if (syncTime && _saveMgr.GetCurrentSlotData("lastTime") != null) {
            _timeMgr.offset = (_saveMgr.GetCurrentSlotData("lastTime").AsDouble);
            _timeMgr.time = _timeMgr.StartTime + _timeMgr.offset;
        }

        if (syncResources) _resourceMgr.DeserializeFromJSON(_saveMgr.GetCurrentSlotClass("ResourceManager"));

        if (syncNarrative) _narrationMgr.DeserializeFromJSON(_saveMgr.GetCurrentSlotClass("NarrationManager"));

        if (syncLocalization) _localMgr.SetLanguage(_saveMgr.GetData("language"));
		if(syncCamera) _cameraMgr.DeserializeFromJSON(_saveMgr.GetCurrentSlotClass("CameraManager"));
		if(syncAvatars) _avatarMgr.DeserializeFromJSON(_saveMgr.GetCurrentSlotClass("AvatarManager"));
		if(syncAppliances) _applianceMgr.DeserializeFromJSON(_saveMgr.GetCurrentSlotClass("ApplianceManager"));

		
	}

	//
	public void GenerateUniqueIDs() {
		List<BehaviourAI> avatars = new List<BehaviourAI>(UnityEngine.Object.FindObjectsOfType<BehaviourAI>());
		List<Appliance> appliances = new List<Appliance>(UnityEngine.Object.FindObjectsOfType<Appliance>());

		foreach(BehaviourAI avatar in avatars) {
			//UniqueId[] ids = avatar.GetComponents<UniqueId>();
			//foreach(UniqueId id in ids) {
			//	DestroyImmediate(id);
			//}
			if(!avatar.GetComponent<UniqueId>()) {
				avatar.gameObject.AddComponent<UniqueId>();
			}
		}

		foreach(Appliance app in appliances) {
			//UniqueId[] ids = app.GetComponents<UniqueId>();
			//foreach(UniqueId id in ids) {
			//	DestroyImmediate(id);
			//}
			if(!app.GetComponent<UniqueId>()) {
				app.gameObject.AddComponent<UniqueId>();
			}
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
				break;
            case "gameOverAnimationFinished":
                LoadScene("MenuScene");
                break;
        }
	}

	//Callback from resouce manager for when monthly salary received
	public void OnResourcesReceived(string data) {
		PlayUIAnimation("CashReceived");
	}

	//
	public void OnGameOver() {
        bool gameWon = false; //Replace with appropiate calculation/function call

        if (gameWon) {
            PlayUIAnimation("GameWonAnimation");
			_instance._monitorMgr.Publish("event", "event", "won:" + SceneManager.GetActiveScene());
		}
        else {
            PlayUIAnimation("GameOverAnimation");
			_instance._monitorMgr.Publish("event", "event", "lost:" + SceneManager.GetActiveScene());
		}
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
			case "AddComfort":
			case "AddMoney":
				SendMessage(callback, float.Parse(parameters[0]));
				break;
			case "ControlAvatar":
			case "MarkDevice":
			case "UnmarkDevice":
				SendMessage(callback, parameters);
				break;
			case "ClearChallengeData":
				SendMessage(callback);
				break;
			case "UnlockViews":
				SendMessage(callback, int.Parse(parameters[0]));
				break;
			default:
				SendMessage(callback, parameters[0]);
				break;
		}
	}

	//Reset interaction limitations and save on narrative completion
	public void OnNarrativeCompleted(Narrative narrative) {
		_instance._narrationMgr.OnNarrativeEvent("NarrativeComplete", narrative.title, true);
		_instance._monitorMgr.Publish("event", "event", "completed:" + narrative.title);
		_instance.LimitInteraction("all");
		_instance.SaveGameState();
	}

	//TODO
	public void OnNarrativeActivated(Narrative narrative) {
	}

	public bool HasEventFired(Narrative narrative, Narrative.Step step) {
		switch(step.conditionType) {
			case "EEMPerformed":
				return _instance._applianceMgr.WasEEMPerformed(step.conditionProp);
			case "NarrativeComplete":
				foreach(Narrative n in _instance._narrationMgr.archive) {
					if(n.title == step.conditionProp) {
						return true;
					}
				}
				break;
			case "CameraArrived":
				return _cameraMgr.GetCurrentViewpoint().title == step.conditionProp;
		}
		return false;
	}

	//
	public bool NarrativeCheck(string callback) {
		JSONArray parse = JSON.Parse(callback).AsArray;
		switch(parse[0]) {
			case "HasWonChallenge":
				return HasWonChallenge();
			case "money":
				switch(parse[1]) {
					case "<":
						return _instance._resourceMgr.cash < parse[2].AsFloat;
					default:
						return false;
				}
			case "comfort":
				switch(parse[1]) {
					case "<":
						return _instance._resourceMgr.comfort < parse[2].AsFloat;
					default:
						return false;
				}
		}
		return false;
	}
}
