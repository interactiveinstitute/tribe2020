using UnityEngine;
using System.Collections.Generic;

public class Room : MonoBehaviour {

	[SerializeField]
	private List<Appliance> _devices;

	public float temperature;
	public float airQuality;
	public float lux;
    [SerializeField]
    private List<BehaviourAI> _occupants;

    public Affordance avatarAffordanceSwitchLight; //Should perhaps be static, or part of a singleton?

    // Use this for initialization
    void Start() {
		_occupants = new List<BehaviourAI>();

		//foreach(Appliance device in GetComponentsInChildren<Appliance>()) {
		//	_devices.Add(device);
		//}
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	//
	public bool IsLit() {
        //Appliance lightSwitch = GetLightSwitch();
        Appliance lightSwitch = GetApplianceWithAffordance(avatarAffordanceSwitchLight);
        if (lightSwitch == null) {
            return false; //Presumption: no light switch -> no light
        }

        return lightSwitch.GetComponent<ElectricMeter>().GivesPower;
	}

    //Tries to retrieve the first available appliance with the given affordance in this room
    public Appliance GetApplianceWithAffordance(Affordance affordance, BehaviourAI owner = null) {
        Appliance targetAppliance = null;
        float minDist = float.MaxValue;
		//Search all appliances in zone and compare to all affordances for each one
        foreach (Appliance device in _devices) {
            List<Appliance.AffordanceResource> affordances = device.avatarAffordances;
            foreach (Appliance.AffordanceResource affordanceSlot in affordances) {
				//If affordance found and either doesn't require to be owner or is owned by given owner
                if(affordanceSlot.affordance == affordance && (owner == null || device.owners.Contains(owner))) {
                    float dist = Vector3.Distance(transform.position, device.transform.position);
					//Keep appliance as candidate if the euclidean distance is shortest so far 
                    if (dist < minDist) {
                        minDist = dist;
                        targetAppliance = device;
                    }
                }
            }
        }
        return targetAppliance;
    }

    public int GetAvatarCount()
    {
        return _occupants.Count;
    }

    public bool IsEmpty()
    {
        return GetAvatarCount() == 0;
    }

    public bool IsObjectInRoom(GameObject obj)
    {
        return this == obj.GetComponentInParent<Room>();
    }

    public void OnAvatarEnter(BehaviourAI avatar)
    {
        //Debug.Log(other.name + " entered " + name);
        _occupants.Add(avatar);
    }

    public void OnAvatarExit(BehaviourAI avatar)
    {
        //Debug.Log(other.name + " left " + name);
        _occupants.Remove(avatar);
    }

	//
	public List<BehaviourAI> GetOccupants() {
		return _occupants;
	}

	//Returns all appliances in the room
	public List<Appliance> GetAppliances() {
		_devices.Clear();

		foreach(Appliance device in GetComponentsInChildren<Appliance>()) {
			_devices.Add(device);
		}

		return _devices;
	}

	//Returns all electric devices in the room
	public List<ElectricDevice> GetElectricDevices() {
		List<ElectricDevice> eDevices = new List<ElectricDevice>();
		foreach(Appliance app in _devices) {
			if(app.GetComponent<ElectricDevice>()) {
				eDevices.Add(app.GetComponent<ElectricDevice>());
			}
		}
		return eDevices;
	}

	public Floor GetFloor() {
        return GetComponentInParent<Floor>();
    }

	//
	public void AddAppliance(Appliance app) {
		_devices.Add(app);
	}

	//
	public void RemoveAppliance(Appliance app) {
		_devices.Remove(app);
	}

	////This functions assumes that the provided affordance is the light switch affordance.
	//public void UpdateLighting(Affordance affordance) {
 //       //Debug.Log("light was changed");
 //       DebugManager.LogError("You are calling the UpdateLighting method. It might currently be broken from changes by Gunnar. Address that before calling it.", this, this);
 //       lux = 0;

 //       //If at least one light is on, there is lux in the room. Simple model...
 //       foreach (Appliance device in _devices) {
	//		//if(device.avatarAffordances.Contains(affordance)) {
 //           foreach(Affordance aff in device.avatarAffordances)
 //           {
 //               if (aff == affordance)
 //               {
 //                   //TODO: The lamp script is not used any longer... What's actually supposed to happen here?
 //                   //I guess we want to check if the ligths are on, right?
 //                   if (device.GetComponent<Lamp>().Power > 0)
 //                   {
 //                       lux = 1;
 //                       return;
 //                   }
 //               } 
 //           }
	//		//}
	//	}

	//	if(_occupants != null) {
	//		foreach(BehaviourAI person in _occupants) {
	//			person.CheckLighting(AvatarActivity.SessionType.TurnOn);
	//		}
	//	}
	//}
}
