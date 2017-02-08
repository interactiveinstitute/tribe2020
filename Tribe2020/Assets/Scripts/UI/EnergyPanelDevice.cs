using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnergyPanelDevice : MonoBehaviour {
	public ElectricDevice device;
	public Text energyText;
	public Image icon;

	// Use this for initialization
	void Start () {
		if(device.GetComponent<Appliance>()) {
			icon.sprite = device.GetComponent<Appliance>().icon;
		}
	}
	
	// Update is called once per frame
	void Update () {
		energyText.text = device.Power + "W";
	}
}
