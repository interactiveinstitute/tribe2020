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

	//
	void Awake() {
		_instance = this;
	}

	// Use this for initialization
	void Start () {
		_appliances = new List<Appliance>(UnityEngine.Object.FindObjectsOfType<Appliance>());
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	//Serialize state as json for a save file
	public JSONClass SerializeAsJSON() {
		JSONClass json = new JSONClass();

		JSONArray applianceJSON = new JSONArray();
		foreach(Appliance appliance in _appliances) {
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
