using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnergyPanelDevice : MonoBehaviour {
	public ElectricDevice device;
	public Text deviceName;
	public Text energyText;
	public Image icon;

	private PilotController _controller;

	// Use this for initialization
	void Start () {
		_controller = PilotController.GetInstance();

		if(device.GetComponent<Appliance>()) {
			Appliance app = device.GetComponent<Appliance>();
			deviceName.text = _controller.GetPhrase("Content.Appliances", app.title);
			icon.sprite = app.icon;
		}
	}
	
	// Update is called once per frame
	void Update () {
		energyText.text = device.Power + "W";
	}
}
