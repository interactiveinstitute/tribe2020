using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuCanvas : MonoBehaviour {
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
	public Text returnAboutText;

	// Use this for initialization
	void Start () {
		_controller = MenuController.GetInstance();
	}
	
	// Update is called once per frame
	void Update () {
		if(_firstLoop) {
			playText.text = _controller.GetPhrase("Interface", "main play");
			continueText.text = _controller.GetPhrase("Interface", "main continue");
			settingsText.text = _controller.GetPhrase("Interface", "main settings");
			aboutText.text = _controller.GetPhrase("Interface", "main about");
			fbText.text = _controller.GetPhrase("Interface", "main facebook");
			slot1Text.text = _controller.GetPhrase("Interface", "main slot") + " 1";
			slot2Text.text = _controller.GetPhrase("Interface", "main play") + " 2";
			slot3Text.text = _controller.GetPhrase("Interface", "main play") + " 3";
			returnFileText.text = _controller.GetPhrase("Interface", "main back");
			returnPilotText.text = _controller.GetPhrase("Interface", "main back");
			returnSettingsText.text = _controller.GetPhrase("Interface", "main back");
			returnAboutText.text = _controller.GetPhrase("Interface", "main back");

			_firstLoop = false;
		}
	}
}
