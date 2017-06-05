using UnityEngine;
using System.Collections.Generic;
using SimpleJSON;

public class ApplianceManager : MonoBehaviour {
	//Singleton hack
	private static ApplianceManager _instance;
	public static ApplianceManager GetInstance() {
		return _instance;
	}

	[SerializeField]
	private List<Appliance> _appliances;
	private Dictionary<string, Appliance> _uidLookup = new Dictionary<string, Appliance>();
	private Dictionary<string, List<Appliance>> _appLookup = new Dictionary<string, List<Appliance>>();
	private List<string> _performedEEMs = new List<string>();
	public List<Appliance> allDevices;

	//
	void Awake() {
		_instance = this;

		//Init uid lookup by uid
		UniqueId[] uids = Object.FindObjectsOfType<UniqueId>();
		foreach(UniqueId uid in uids) {
			_uidLookup.Add(uid.uniqueId, uid.GetComponent<Appliance>());
		}

		//Init appliance lookup by app title
		foreach(Appliance app in allDevices) {
			_appLookup.Add(app.title, new List<Appliance>());
		}
	}

	// Use this for initialization
	void Start() {
	}

	// Update is called once per frame
	void Update() {

	}

	//
	public void AddAppliance(Appliance appliance) {
		//Debug.Log("adding a " + appliance.title);
		_appliances.Add(appliance);
		if(_appLookup.ContainsKey(appliance.title)) {
			_appLookup[appliance.title].Add(appliance);
		}
		//else {
		//	Debug.Log("There is no " + appliance.title, appliance.gameObject);
		//}
	}

	//
	public void RemoveAppliance(Appliance appliance) {
		if(!_appLookup.ContainsKey(appliance.title)) { return; }
		_appliances.Remove(appliance);
		_appLookup[appliance.title].Remove(appliance);
	}

	//
	public void ReplaceAppliance(Appliance oldApp, Appliance newApp) {
		//Only applies to base appliances
		if(!oldApp.transform.parent.GetComponent<ApplianceSlot>()) {
			//If old has uid, clone to new and update lookup table
			if(oldApp.GetComponent<UniqueId>()) {
				newApp.gameObject.AddComponent<UniqueId>();
				string uid = oldApp.gameObject.GetComponent<UniqueId>().uniqueId;
				newApp.GetComponent<UniqueId>().uniqueId = uid;
				_uidLookup[uid] = newApp;
			}

			//Remove old and add new to list
			//RemoveAppliance(oldApp);
			//AddAppliance(newApp);
		}
	}

	//
	public bool ContainsAppliance(Appliance app) {
		return _appliances.Contains(app);
	}

	//
	public List<Appliance> GetAppliances() {
		return _appliances;
	}

	//
	public Appliance GetAppliance(string uid) {
		return _uidLookup[uid];
		//foreach(Appliance app in _appliances) {
		//	if(app.GetComponent<UniqueId>() && app.GetComponent<UniqueId>().uniqueId == uid) {
		//		return app;
		//	}
		//}
		//return null;
	}

	//
	public List<Appliance> GetAppliancesOfType(string title) {
		return _appLookup[title];
	}

	//
	public void AddEEMTitle(string eemTitle) {
		_performedEEMs.Add(eemTitle);
	}

	//
	public bool WasEEMPerformed(string eemTitle) {
		return _performedEEMs.Contains(eemTitle);
	}

	//Serialize state as json for a save file
	public JSONClass SerializeAsJSON() {
		JSONClass json = new JSONClass();

		JSONArray applianceJSON = new JSONArray();
		int index = 0;
		foreach(Appliance appliance in _appliances) {
			if(!appliance) {
				
				Debug.Log("No, here it is! " + index++);
			}
			applianceJSON.Add(appliance.SerializeAsJSON());
		}

		json.Add("appliances", applianceJSON);
		return json;

		//_saveMgr.SetCurrentSlotArray("Appliances", applianceJSON);


		//JSONArray avatarsJSON = new JSONArray();
		//foreach(BehaviourAI avatar in _avatars) {
		//	avatarsJSON.Add(avatar.Encode());
		//}

		//json.Add("avatars", avatarsJSON);
		//return json;
	}

	//Deserialize json and apply states to aspects handled by the manager
	public void DeserializeFromJSON(JSONClass json) {
		if(json != null) {
			JSONArray appsJSON = json["appliances"].AsArray;

			foreach(JSONClass appJSON in appsJSON) {
				foreach(Appliance app in _appliances) {
					if(app.GetComponent<UniqueId>().uniqueId.Equals(appJSON["id"])) {
						app.DeserializeFromJSON(appJSON);
					}
				}
			}

			//if(syncAppliances && _saveMgr.GetCurrentSlotData("Appliances") != null) {

			//}



			//JSONArray avatarsJSON = json["avatars"].AsArray;

			//foreach(JSONClass avatarJSON in avatarsJSON) {
			//	foreach(BehaviourAI avatar in _avatars) {
			//		string loadedName = avatarJSON["name"];
			//		if(avatar.name == loadedName) {
			//			avatar.Decode(avatarJSON);
			//		}
			//	}
			//}
		}
	}
}
