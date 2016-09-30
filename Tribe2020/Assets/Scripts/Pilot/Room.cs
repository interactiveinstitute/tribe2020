using UnityEngine;
using System.Collections.Generic;

public class Room : MonoBehaviour {
	[SerializeField]
	private List<Appliance> _devices;

	public float temperature;
	public float airQuality;
	public float lux;
	public int personCount;
	private List<BehaviourAI> _occupants;

	// Use this for initialization
	void Start() {
		_occupants = new List<BehaviourAI>();

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
			if(device.avatarAffordances.Contains(AvatarActivity.Target.LampSwitch)) {
				return device;
			}
		}

		return null;
	}

	//
	void OnTriggerEnter(Collider other) {
		if(other.GetComponent<BehaviourAI>()) {
			//Debug.Log(other.name + " entered " + name);
			personCount++;
			_occupants.Add(other.GetComponent<BehaviourAI>());
		}
	}

	//
	void OnTriggerExit(Collider other) {
		if(other.GetComponent<BehaviourAI>()) {
			//Debug.Log(other.name + " left " + name);
			personCount--;
			_occupants.Remove(other.GetComponent<BehaviourAI>());
		}
	}

	//
	public void UpdateLighting() {
		//Debug.Log("light was changed");
		foreach(Appliance device in _devices) {
			if(device.avatarAffordances.Contains(AvatarActivity.Target.Lamp)) {
				if(device.GetComponent<Lamp>().Power > 0) {
					lux = 1;
					return;
				}
			}
		}

		lux = 0;

		if(_occupants != null) {
			foreach(BehaviourAI person in _occupants) {
				person.CheckLighting();
			}
		}
	}
}
