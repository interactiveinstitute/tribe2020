using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnergyPanelDevice : MonoBehaviour {
	public List<ElectricDevice> devices;
	public ElectricDevice device;
	public Text deviceName;
	public Text energyText;
	public Image icon;

	private PilotController _controller;

	// Use this for initialization
	void Start () {
		_controller = PilotController.GetInstance();

		if(devices.Count > 0 && devices[0].GetComponent<Appliance>()) {
			Appliance app = devices[0].GetComponent<Appliance>();
			deviceName.text = _controller.GetPhrase("Content.Appliances", app.title);
			icon.sprite = app.icon;
		}
	}
	
	// Update is called once per frame
	void Update () {
		float power = 0;
		foreach(ElectricDevice ed in devices) {
			power += ed.Power;
		}
		energyText.text = "x " + devices.Count +": " + power + " W";
	}
}
