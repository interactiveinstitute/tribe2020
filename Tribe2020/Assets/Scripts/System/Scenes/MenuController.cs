using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class MenuController : Controller {
	public GameObject animatedCharacter;
	public Transform animatedPilot;
	public RectTransform animatedLogo;

	private CustomSceneManager _sceneMgr;
	private SaveManager _saveMgr;

	public RectTransform mainMenu;
	public RectTransform fileMenu;
	public RectTransform pilotMenu;
	public RectTransform settingsMenu;
	public RectTransform aboutMenu;

	private RectTransform _curMenu;
	public List<Transform> menus;

	public Transform continueButton;

	public Transform file1;
	public Transform remove1;
	public Transform file2;
	public Transform remove2;
	public Transform file3;
	public Transform remove3;
	int pendingNewGameSlot = 0;
	//private bool _isLoaded = false;

	// Use this for initialization
	void Start () {
		_sceneMgr = CustomSceneManager.GetInstance();
		_saveMgr = SaveManager.GetInstance();

		_curMenu = mainMenu;
		menus = new List<Transform>();
		menus.Add(mainMenu);
		menus.Add(fileMenu);
		menus.Add(pilotMenu);
		menus.Add(settingsMenu);
		menus.Add(aboutMenu);

		InitSlotButton(file1, remove1, 0);
		InitSlotButton(file2, remove2, 1);
		InitSlotButton(file3, remove3, 2);

		int lastSlot = _saveMgr.GetLastSlot();
		continueButton.gameObject.SetActive(lastSlot != -1);
		continueButton.GetComponent<Button>().onClick.AddListener(() =>
			ContinueGame(_saveMgr.GetData(lastSlot, "pilot"), lastSlot));
	}
	
	// Update is called once per frame
	void Update () {
		//if(!_isLoaded) {
		//	_isLoaded = true;
		//	_saveMgr.Load();


		//}

		//Animator anim = animatedCharacter.GetComponent<Animator>();
		//anim.SetFloat("Speed", 100);

		//animatedPilot.Rotate(Vector3.up * Time.deltaTime);

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
	public void InitSlotButton(Transform button, Transform removeButton, int slot) {
		Debug.Log("InitSlotButton" + button.name + ", " + removeButton.name + ", " + slot + ", " + _saveMgr.IsSlotVacant(slot));
		button.GetComponent<Button>().onClick.RemoveAllListeners();
		if(_saveMgr.IsSlotVacant(slot)) {
			button.GetComponentInChildren<Text>().text = "New Game";
			pendingNewGameSlot = slot;
			button.GetComponent<Button>().onClick.AddListener(() => OpenPilotMenu(slot));
			removeButton.gameObject.SetActive(false);
		} else {
			button.GetComponentInChildren<Text>().text = _saveMgr.GetSlotData(slot);
			button.GetComponent<Button>().onClick.AddListener(() => ContinueGame(_saveMgr.GetData(slot, "pilot"), slot));
			removeButton.gameObject.SetActive(true);
		}
	}

	//
	public void InitPilotButton(Transform button) {
	}

	//
	public void NewGame(string pilot) {
		SaveManager.currentSlot = pendingNewGameSlot;
		Debug.Log("NewGame: " + SaveManager.currentSlot);
		_saveMgr.InitSlot(pendingNewGameSlot, pilot);
		LoadScene(pilot);
	}

	//
	public void ClearSlots() {
		_saveMgr.Delete(0);
		_saveMgr.Delete(1);
		_saveMgr.Delete(2);

		InitSlotButton(file1, remove1, 0);
		InitSlotButton(file2, remove2, 1);
		InitSlotButton(file3, remove3, 2);
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
			continueButton.gameObject.SetActive(false);
			_saveMgr.ResetLastSlot();
		}
	}

	//
	public void LoadScene(string scene) {
		_sceneMgr.LoadScene(scene);
	}
}
