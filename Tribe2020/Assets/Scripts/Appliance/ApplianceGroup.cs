using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ApplianceGroup : MonoBehaviour {
	[System.Serializable]
	public struct ApplianceReference {
		public string key;
		public GameObject prefab;
		public Transform transform;
		public string[] replaces;
	}

	[System.Serializable]
	public struct ApplianceSlot {
		public string key;
		public Transform transform;
		public GameObject[] possibleAppliances;
	}

	public List<ApplianceReference> appliances;

	public List<ApplianceSlot> applianceSlots;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	//
	public void RefreshSlots() {
		ApplianceSlot[] slots = GetComponentsInChildren<ApplianceSlot>();
		foreach(ApplianceSlot slot in slots) {
			ElectricDevice removedDevice = slot.transform.GetComponent<ElectricDevice>();

		}
	}

	//
	public void SetAppliance(string slotKey, GameObject appliancePrefab) {
	}


	//
	public void ActivateAppliance(string key) {
		ApplianceReference newApp = GetReference(key);
		//TODO check if empty
		//Deactivate appliance in occupied spot
		DeactivateAppliance(GetApplianceAtSpot(newApp.transform));
		//Instantiate object for replacement appliance
		GameObject newAppObj = Instantiate(newApp.prefab);
		


	}

	//
	public void DeactivateAppliance(GameObject go) {

	}

	//
	//public void ReplaceAppliance(string existingKey, string replacementKey) {
	//	ApplianceReference existing = GetReference(existingKey);
	//	ApplianceReference replacement = 
	//}

	//
	public GameObject GetApplianceAtSpot(Transform spot) {
		foreach(ApplianceReference app in appliances) {
			if(app.transform == spot) {
				//transform.FindChild(
			}
		}
		return null;
	}

	//
	public ApplianceReference GetReference(string key) {
		foreach(ApplianceReference app in appliances) {
			if(app.key == key) {
				return app;
			}
		}
		return new ApplianceReference();
	}
}
