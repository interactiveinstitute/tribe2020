using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using Facebook.Unity;

public class MenuController : Controller {
	public GameObject animatedCharacter;
	public Transform animatedPilot;
	public RectTransform animatedLogo;

	private CustomSceneManager _sceneMgr;
	private SaveManager _saveMgr;
	private LocalisationManager _localMgr;

	[Header("UI Panels")]
	public RectTransform mainMenu;
	public RectTransform fileMenu;
	public RectTransform pilotMenu;
	public RectTransform settingsMenu;
	public RectTransform aboutMenu;

	private RectTransform _curMenu;
	public List<Transform> menus;

	public Transform continueButton;

	[Header("Save Slot Buttons")]
	public Transform file1;
	public Transform remove1;
	public Transform file2;
	public Transform remove2;
	public Transform file3;
	public Transform remove3;
	int pendingNewGameSlot = 0;

	[Header("Pilot Buttons")]
	public Transform pilotSanPablo;
	public Transform pilotIESAzucarera;
	public Transform pilotCIRCEHQ;
	public Transform pilotEmmelinedePankhurst;
	public Transform pilotUniversidadOuezygin;
	//private bool _isLoaded = false;

	[Header("Settings")]
	public Transform languageDropdown;

	// Use this for initialization
	void Start () {
		_sceneMgr = CustomSceneManager.GetInstance();
		_saveMgr = SaveManager.GetInstance();
		_localMgr = LocalisationManager.GetInstance();

		_curMenu = mainMenu;
		menus = new List<Transform>();
		menus.Add(mainMenu);
		menus.Add(fileMenu);
		menus.Add(pilotMenu);
		menus.Add(settingsMenu);
		menus.Add(aboutMenu);

		RefreshContinue();

		InitSlotButton(file1, remove1, 0);
		InitSlotButton(file2, remove2, 1);
		InitSlotButton(file3, remove3, 2);

		InitPilotButton(pilotSanPablo);
		InitPilotButton(pilotIESAzucarera);
		InitPilotButton(pilotCIRCEHQ);
		InitPilotButton(pilotEmmelinedePankhurst);
		InitPilotButton(pilotUniversidadOuezygin);

		InitLocalisation();

		FB.Init(this.OnInitComplete, this.OnHideUnity);
	}
	
	// Update is called once per frame
	void Update () {
		if(animatedLogo.anchoredPosition.x < 166) {
			float curX = animatedLogo.anchoredPosition.x;
			curX = 166 + (curX - 166) * 0.75f;
			animatedLogo.anchoredPosition = new Vector2(curX, animatedLogo.anchoredPosition.y);
		}

		foreach(RectTransform menu in menus) {
			if(menu != _curMenu) {
				if(menu.anchoredPosition.x < 200) {
					float curX = menu.anchoredPosition.x;
					curX = 200 + (curX - 200) * 0.75f;
					menu.anchoredPosition = new Vector2(curX, menu.anchoredPosition.y);
				}
			} else {
				if(menu.anchoredPosition.x > -180) {
					float curX = menu.anchoredPosition.x;
					curX = -180 + (curX + 180) * 0.75f;
					menu.anchoredPosition = new Vector2(curX, menu.anchoredPosition.y);
				}
			}
		}
	}

	//
	public void OpenMenu(string menuState) {
		switch(menuState) {
			case "Main":
				_curMenu = mainMenu;
				break;
			case "File":
				_curMenu = fileMenu;
				break;
			case "Settings":
				_curMenu = settingsMenu;
				break;
			case "About":
				_curMenu = aboutMenu;
				break;
		}
	}

	//
	public void OpenPilotMenu(int slot) {
		pendingNewGameSlot = slot;
		_curMenu = pilotMenu;
	}

	//
	public void RefreshContinue() {
		int lastSlot = _saveMgr.GetLastSlot();

		if(lastSlot != -1) {
			continueButton.gameObject.SetActive(true);
			continueButton.GetComponent<Button>().onClick.RemoveAllListeners();
			continueButton.GetComponent<Button>().onClick.AddListener(() =>
				ContinueGame(_saveMgr.GetData(lastSlot, "curPilot"), lastSlot));
		} else {
			continueButton.gameObject.SetActive(false);
		}
	}

	//
	public void InitSlotButton(Transform button, Transform removeButton, int slot) {
		button.GetComponent<Button>().onClick.RemoveAllListeners();
		if(_saveMgr.IsSlotVacant(slot)) {
			button.GetComponentInChildren<Text>().text = "New Game";
			pendingNewGameSlot = slot;
			button.GetComponent<Button>().onClick.AddListener(() => OpenPilotMenu(slot));
			removeButton.gameObject.SetActive(false);
		} else {
			button.GetComponentInChildren<Text>().text = _saveMgr.GetSlotData(slot);
			button.GetComponent<Button>().onClick.AddListener(() => ContinueGame(_saveMgr.GetData(slot, "curPilot"), slot));
			removeButton.gameObject.SetActive(true);
		}
	}

	//
	public void InitPilotButton(Transform button) {
		if(_saveMgr.GetData(button.name) != null) {
			bool isUnlocked = _saveMgr.GetData(button.name).AsBool;
			button.GetComponent<Button>().interactable = isUnlocked;
		}
	}

	//
	public void InitLocalisation() {
		Dropdown dropdown = languageDropdown.GetComponent<Dropdown>();
		List<string> langStrings = new List<string>();

		foreach(Language lang in _localMgr.languages) {
			langStrings.Add(lang.name);
		}

		dropdown.AddOptions(langStrings);
	}

	//
	public void NewGame(string pilot) {
		SaveManager.currentSlot = pendingNewGameSlot;
		_saveMgr.SetData("lastSlot", pendingNewGameSlot.ToString());
		LoadScene(pilot);
	}

	//
	public void ClearSlots() {
		DeleteSlot(0);
		DeleteSlot(1);
		DeleteSlot(2);
	}

	//
	public void ContinueGame(string scene, int slot) {
		SaveManager.currentSlot = slot;
		LoadScene(scene);
	}

	//
	public void ContinueGame() {
	}

	//
	public void DeleteSlot(int slot) {
		_saveMgr.Delete(slot);
		switch(slot) {
			case 0:
				InitSlotButton(file1, remove1, 0);
				break;
			case 1:
				InitSlotButton(file2, remove2, 1);
				break;
			case 2:
				InitSlotButton(file3, remove3, 2);
				break;
		}

		int lastSlot = _saveMgr.GetLastSlot();
		if(lastSlot == slot) {
			_saveMgr.ResetLastSlot();
		}

		RefreshContinue();
	}

	//
	public void LoadScene(string scene) {
		_sceneMgr.LoadScene(scene);
	}

	//
	public void PostToFacebook() {
		FB.LogInWithReadPermissions(new List<string>() { "public_profile", "email", "user_friends" }, this.HandleResult);
	}

	private void CallFBLogin() {
		FB.LogInWithReadPermissions(new List<string>() { "public_profile", "email", "user_friends" }, this.HandleResult);
	}

	private void CallFBLoginForPublish() {
		// It is generally good behavior to split asking for read and publish
		// permissions rather than ask for them all at once.
		//
		// In your own game, consider postponing this call until the moment
		// you actually need it.
		FB.LogInWithPublishPermissions(new List<string>() { "publish_actions" }, this.HandleResult);
	}

	private void CallFBLogout() {
		FB.LogOut();
	}

	private void OnInitComplete() {
		//this.Status = "Success - Check logk for details";
		//this.LastResponse = "Success Response: OnInitComplete Called\n";
		string logMessage = string.Format("OnInitCompleteCalled IsLoggedIn='{0}' IsInitialized='{1}'", FB.IsLoggedIn, FB.IsInitialized);
		//LogView.AddLog(logMessage);
	}

	private void OnHideUnity(bool isGameShown) {
		//this.Status = "Success - Check logk for details";
		//this.LastResponse = string.Format("Success Response: OnHideUnity Called {0}\n", isGameShown);
		//LogView.AddLog("Is game shown: " + isGameShown);
	}

	//
	protected void HandleResult(IResult result) {
		if(result == null) {
			//this.LastResponse = "Null Response\n";
			//LogView.AddLog(this.LastResponse);
			return;
		}

		//this.LastResponseTexture = null;

		//// Some platforms return the empty string instead of null.
		if(!string.IsNullOrEmpty(result.Error)) {
		//	this.Status = "Error - Check log for details";
		//	this.LastResponse = "Error Response:\n" + result.Error;
		//	LogView.AddLog(result.Error);
		} else if(result.Cancelled) {
		//	this.Status = "Cancelled - Check log for details";
		//	this.LastResponse = "Cancelled Response:\n" + result.RawResult;
		//	LogView.AddLog(result.RawResult);
		} else if(!string.IsNullOrEmpty(result.RawResult)) {
		//	this.Status = "Success - Check log for details";
		//	this.LastResponse = "Success Response:\n" + result.RawResult;
		//	LogView.AddLog(result.RawResult);
		} else {
		//	this.LastResponse = "Empty Response\n";
		//	LogView.AddLog(this.LastResponse);
		}
	}
}
