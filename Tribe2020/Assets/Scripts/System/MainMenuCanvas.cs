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
		TranslateText(playText);
		TranslateText(continueText);
		TranslateText(settingsText);
		TranslateText(aboutText);
		TranslateText(fbText);

		_controller.InitSlotButton(slot1Text.transform.parent, slot1Text.transform.parent.parent.GetChild(0), 0);
		_controller.InitSlotButton(slot2Text.transform.parent, slot2Text.transform.parent.parent.GetChild(0), 1);
		_controller.InitSlotButton(slot3Text.transform.parent, slot3Text.transform.parent.parent.GetChild(0), 2);

		TranslateText(returnFileText);
		TranslateText(returnPilotText);
		TranslateText(returnSettingsText);
		TranslateText(clearSavesText);
		TranslateText(returnAboutText);

		foreach(Text t in pilotPlayButtonTexts) {
			TranslateText(t);
		}
		foreach(Text t in energyDistributionTexts) {
			TranslateText(t);
		}
		foreach(Text t in hotWaterTexts) {
			TranslateText(t);
		}
		foreach(Text t in coolingTexts) {
			TranslateText(t);
		}
		foreach(Text t in heatingTexts) {
			TranslateText(t);
		}
		foreach(Text t in lightingTexts) {
			TranslateText(t);
		}
		foreach(Text t in fanTexts) {
			TranslateText(t);
		}
		foreach(Text t in appliancesTexts) {
			TranslateText(t);
		}
		foreach(Text t in statsTitleTexts) {
			TranslateText(t);
		}
		foreach(Text t in statsDescriptionTexts) {
			TranslateText(t);
		}
		foreach(Text t in aboutTitleTexts) {
			TranslateText(t);
		}

		for(int i = 0; i < pilots.Length; i++) {
			statsTexts[i].text = _controller.GetPhrase(pilots[i], "Stats");
			aboutTexts[i].text = _controller.GetPhrase(pilots[i], "Info");
		}
	}

	//Given a UI text, look for translation using object name as key
	public void TranslateText(Text text) {
		text.text = _controller.GetPhrase("MainMenu", text.name);
	}
}
