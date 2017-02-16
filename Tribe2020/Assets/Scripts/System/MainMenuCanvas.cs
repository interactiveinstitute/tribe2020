using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuCanvas : MonoBehaviour {
	private static MainMenuCanvas _instance;
	public static MainMenuCanvas GetInstance() {
		return _instance;
	}

	private MenuController _controller;
	private bool _firstLoop = true;

	public Text playText;
	public Text continueText;
	public Text settingsText;
	public Text aboutText;
	public Text fbText;
	public Text slot1Text;
	public Text slot2Text;
	public Text slot3Text;
	public Text returnFileText;
	public Text returnPilotText;
	public Text returnSettingsText;
	public Text clearSavesText;
	public Text returnAboutText;

	public List<Language> languages;
	public GameObject localisationButtonPrefab;
	public Transform localisationButtonContainer;

	//
	void Awake() {
		_instance = this;
	}

	// Use this for initialization
	void Start () {
		_controller = MenuController.GetInstance();

		foreach(Language lang in languages) {
			Language curLang = lang;
			GameObject newButton = Instantiate(localisationButtonPrefab);
			newButton.transform.SetParent(localisationButtonContainer, false);
			newButton.GetComponent<Image>().sprite = curLang.flagSprite;
			newButton.GetComponentInChildren<Button>().onClick.AddListener(() => _controller.OnLanguagePicked(curLang.name));
		}
	}
	
	// Update is called once per frame
	void Update () {
		if(_firstLoop) {
			Translate();
			_firstLoop = false;
		}
	}

	//
	public void Translate() {
		playText.text = _controller.GetPhrase("Interface", "main play");
		continueText.text = _controller.GetPhrase("Interface", "main continue");
		settingsText.text = _controller.GetPhrase("Interface", "main settings");
		aboutText.text = _controller.GetPhrase("Interface", "main about");
		fbText.text = _controller.GetPhrase("Interface", "main facebook");

		_controller.InitSlotButton(slot1Text.transform.parent, slot1Text.transform.parent.parent.GetChild(0), 0);
		_controller.InitSlotButton(slot2Text.transform.parent, slot2Text.transform.parent.parent.GetChild(0), 1);
		_controller.InitSlotButton(slot3Text.transform.parent, slot3Text.transform.parent.parent.GetChild(0), 2);
		//slot1Text.text = _controller.GetPhrase("Interface", "main slot") + " 1";
		//slot2Text.text = _controller.GetPhrase("Interface", "main slot") + " 2";
		//slot3Text.text = _controller.GetPhrase("Interface", "main slot") + " 3";

		returnFileText.text = _controller.GetPhrase("Interface", "main back");
		returnPilotText.text = _controller.GetPhrase("Interface", "main back");
		returnSettingsText.text = _controller.GetPhrase("Interface", "main back");
		clearSavesText.text = _controller.GetPhrase("Interface", "main clearsaves");
		returnAboutText.text = _controller.GetPhrase("Interface", "main back");
	}
}
