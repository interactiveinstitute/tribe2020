using UnityEngine;
using System.Collections.Generic;

public class Room : MonoBehaviour {
	[SerializeField]
	private List<Appliance> _devices;

	public float temperature;
	public float airQuality;
	public float lux;
    private List<BehaviourAI> _occupants;

    public Affordance avatarAffordanceSwitchLight;

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

	//Retrieve the first occasion of a light switch. Not nice with string comparison! Please improve
	public Appliance GetLightSwitch() {
        foreach (Appliance device in _devices) {
            foreach (Affordance aff in device.avatarAffordances)
            {
                //Checking with strings should only be done in special cases!
                //Aim for always getting an actual reference to the affordance we want to compare against rather than using strings.
                if (aff == avatarAffordanceSwitchLight)
                {
                    return device;
                }
            }
		}

		return null;
	}

    //Tries to retrieve the first available appliance with the given affordance in this room
    public Appliance GetApplianceWithAffordance(Affordance affordance)
    {
        foreach (Appliance device in _devices)
        {
            foreach(Affordance aff in device.avatarAffordances)
            {
                //We must check for reference equality. We only want to return if it's the same scriptableObject asset (the assets are actually different instances of the scriptableObject)
                //Hmm. But doesn't Equals default to reference equality? So if we haven't overridden it we should be fine, no?
                if(Object.ReferenceEquals(aff, affordance))
                {
                    return device;
                }

            }
            //if (device.avatarAffordances.Contains(affordance))//Must check for being same instance
            //{
            //    return device;
            //}
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
