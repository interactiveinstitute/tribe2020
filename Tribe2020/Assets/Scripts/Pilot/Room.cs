using UnityEngine;
using System.Collections.Generic;

public class Room : MonoBehaviour {
	[SerializeField]
	private List<Appliance> _devices;

	public float temperature;
	public float airQuality;
	public float lux;
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
        Appliance lightSwitch = GetLightSwitch();
        if(lightSwitch == null)
        {
            return false; //Presumption: no light switch -> no light
        }
        return GetLightSwitch().GetComponent<ElectricMeter>().GivesPower;
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

    public int GetPersonCount()
    {
        return _occupants.Count;
    }

    public bool IsEmpty()
    {
        return GetPersonCount() == 0;
    }

	//
	void OnTriggerEnter(Collider other) {
		if(other.GetComponent<BehaviourAI>()) {
			//Debug.Log(other.name + " entered " + name);
			_occupants.Add(other.GetComponent<BehaviourAI>());
        }
	}

	//
	void OnTriggerExit(Collider other) {
        if (other.GetComponent<BehaviourAI>()) {
			//Debug.Log(other.name + " left " + name);
			_occupants.Remove(other.GetComponent<BehaviourAI>());
        }
	}

	//
	public void UpdateLighting() {
        //Debug.Log("light was changed");

        lux = 0;

        foreach (Appliance device in _devices) {
			if(device.avatarAffordances.Contains(AvatarActivity.Target.Lamp)) {
				if(device.GetComponent<Lamp>().Power > 0) {
					lux = 1;
					return;
				}
			}
		}

		if(_occupants != null) {
			foreach(BehaviourAI person in _occupants) {
				person.CheckLighting(AvatarActivity.SessionType.TurnOn);
			}
		}
	}
}
