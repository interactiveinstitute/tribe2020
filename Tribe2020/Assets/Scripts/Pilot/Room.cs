using UnityEngine;
using System.Collections.Generic;

public class Room : MonoBehaviour {
	[SerializeField]
	private List<Appliance> _devices;

	public float temperature;
	public float airQuality;
	public float lux;
	public int personCount;

	// Use this for initialization
	void Start () {
		foreach(Appliance device in GetComponentsInChildren<Appliance>()) {
			_devices.Add(device);
		}
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	//
	public bool IsLit() {

		return false;
	}

	//
	public Appliance GetLightSwitch() {
		foreach(Appliance device in _devices) {
			if(device.avatarAffordances.Contains("light_switch")) {
				return device;
			}
		}

		return null;
	}

	//
	void OnTriggerEnter(Collider other) {
		if(other.GetComponent<BehaviourAI>()) {
			personCount++;
		}
	}

	//
	void OnTriggerExit(Collider other) {
		if(other.GetComponent<BehaviourAI>()) {
			personCount--;
		}
	}

	//
	public void UpdateLighting() {
		Debug.Log("light was changed");
		foreach(Appliance device in _devices) {
			if(device.avatarAffordances.Contains("light")) {
				if(device.GetComponent<ElectricDevice>().GivesPower) {
					lux = 1;
					return;
				}
			}
		}
		lux = 0;
	}
}
