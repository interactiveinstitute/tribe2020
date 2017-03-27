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

	[Header("Pilot Panel")]
	public Text[] pilotPlayButtonTexts;
	public Text[] energyDistributionTexts;
	public Text[] hotWaterTexts;
	public Text[] coolingTexts;
	public Text[] heatingTexts;
	public Text[] lightingTexts;
	public Text[] fanTexts;
	public Text[] appliancesTexts;
	public Text[] statsTitleTexts;
	public Text[] statsDescriptionTexts;
	public Text[] statsTexts;
	public Text[] aboutTitleTexts;
	public Text[] aboutTexts;
	private string[] pilots = 
		new string[] { "San Pablo", "CIRCE", "Emmeline de Pankhurst", "IES Azucarera", "Ozyegin University" };

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

		foreach(Text t in pilotPlayButtonTexts) {
			t.text = _controller.GetPhrase("Interface", "main play level");
		}
		foreach(Text t in energyDistributionTexts) {
			t.text = _controller.GetPhrase("Interface", "main pilot distribution");
		}
		foreach(Text t in hotWaterTexts) {
			t.text = _controller.GetPhrase("Interface", "main pilot distribution", 0);
		}
		foreach(Text t in coolingTexts) {
			t.text = _controller.GetPhrase("Interface", "main pilot distribution", 1);
		}
		foreach(Text t in heatingTexts) {
			t.text = _controller.GetPhrase("Interface", "main pilot distribution", 2);
		}
		foreach(Text t in lightingTexts) {
			t.text = _controller.GetPhrase("Interface", "main pilot distribution", 3);
		}
		foreach(Text t in fanTexts) {
			t.text = _controller.GetPhrase("Interface", "main pilot distribution", 4);
		}
		foreach(Text t in appliancesTexts) {
			t.text = _controller.GetPhrase("Interface", "main pilot distribution", 5);
		}
		foreach(Text t in statsTitleTexts) {
			t.text = _controller.GetPhrase("Interface", "main pilot stat title");
		}
		foreach(Text t in statsDescriptionTexts) {
			t.text = _controller.GetPhrase("Interface", "main pilot stat description");
		}
		foreach(Text t in aboutTitleTexts) {
			t.text = _controller.GetPhrase("Interface", "main pilot about title");
		}

		for(int i = 0; i < pilots.Length; i++) {
			statsTexts[i].text = _controller.GetPhrase(pilots[i], "Stats");
			aboutTexts[i].text = _controller.GetPhrase(pilots[i], "Info");
		}
	}
}
