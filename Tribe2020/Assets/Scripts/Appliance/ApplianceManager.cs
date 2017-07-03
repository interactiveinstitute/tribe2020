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
	public Appliance pilotAppliance;
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
			if(_uidLookup.ContainsKey(uid.uniqueId)) {
				Debug.Log("duplicate uid " + uid.uniqueId + " for " + uid.name, uid.gameObject);
			}
			if(_uidLookup.ContainsKey("")) {
				Debug.Log("uid not generated for " + uid.name, uid.gameObject);
			}
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
		if(appliance.isPilot) {
			pilotAppliance = appliance;
			return;
		}

		_appliances.Add(appliance);
		if(_appLookup.ContainsKey(appliance.title)) {
			_appLookup[appliance.title].Add(appliance);
		}
	}

	//
	public void RemoveAppliance(Appliance appliance) {
		if(!_appLookup.ContainsKey(appliance.title)) { return; }
		_appliances.Remove(appliance);
		_appLookup[appliance.title].Remove(appliance);
	}

	//
	public void ReplaceAppliance(Appliance oldApp, Appliance newApp) {
		if(oldApp.GetComponent<UniqueId>()) {
			newApp.gameObject.AddComponent<UniqueId>();
			string uid = oldApp.gameObject.GetComponent<UniqueId>().uniqueId;
			newApp.GetComponent<UniqueId>().uniqueId = uid;
			_uidLookup[uid] = newApp;

			newApp.appliedEEMs = oldApp.appliedEEMs;
			newApp.RefreshEEMs();
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
	public void RefreshAllSlots() {
		Appliance[] apps = Object.FindObjectsOfType<Appliance>();
		foreach(Appliance app in apps) {
			app.RefreshSlots();
		}
	}

	//
	public Appliance GetAppliance(string uid) {
		if(!_uidLookup.ContainsKey(uid)) {
			Debug.Log("does not contain " + uid);
		}
		return _uidLookup[uid];
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
		foreach(Appliance app in _appliances) {
			applianceJSON.Add(app.SerializeAsJSON());
		}

		json.Add("appliances", applianceJSON);
		return json;
	}

	//Deserialize json and apply states to aspects handled by the manager
	public void DeserializeFromJSON(JSONClass json) {
		if(json != null) {
			JSONArray appsJSON = json["appliances"].AsArray;

			foreach(JSONClass appJSON in appsJSON) {
				GetAppliance(appJSON["id"]).DeserializeFromJSON(appJSON);

				//foreach(Appliance app in _appliances) {
				//	if(app.GetComponent<UniqueId>().uniqueId.Equals(appJSON["id"])) {
				//		app.DeserializeFromJSON(appJSON);
				//	}
				//}
			}
		}
	}
}
