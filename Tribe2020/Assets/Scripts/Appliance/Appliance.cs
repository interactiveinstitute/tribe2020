using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using SimpleJSON;
using System.Collections;
using System;

public class Appliance : MonoBehaviour, IPointerClickHandler {
	private PilotController _ctrlMgr;
	private PilotView _pilotView;
	private ApplianceManager _applianceManager;
	private EnergyEfficiencyMeasureContainer _eemMgr;

	[Header("Properties")]
	public string title;
	public string description;
	public Sprite icon;
	public bool isPilot = false;
	private bool _isTopAppliance = false;

	[Header("Affordances")]
	public List<EnergyEfficiencyMeasure> playerAffordances;
	public List<AffordanceResource> avatarAffordances;

	[Header("References")]
	public List<BehaviourAI> owners;
	private Room _zone;

	[System.Serializable]
	public class PoseSlot {
		public Vector3 position;
		public Quaternion rotation;
		public BehaviourAI occupant;
	}

	[System.Serializable]
	public class AffordanceResource {
		public Affordance affordance;
		public int nrOfSlots;
		public List<AvatarActivity> subscribingActivities = new List<AvatarActivity>();

		public int usedSlots() {
			return subscribingActivities.Count;
		}

		public bool AddUser(AvatarActivity activity) {
			if(usedSlots() < nrOfSlots) {
				subscribingActivities.Add(activity);
				return true;
			}
			return false;
		}

		public bool RemoveUser(AvatarActivity activity) {
			if(usedSlots() > 0) {
				subscribingActivities.Remove(activity);
				return true;
			}
			return false;
		}

		public bool RemoveAllUsers() {
			foreach(AvatarActivity activity in subscribingActivities.ToArray()) {
				activity.LoseAffordanceSlot();
				subscribingActivities.Remove(activity);
			}
			return false;
		}

		public int AvailableSlots() {
			return nrOfSlots - usedSlots();
		}

	}

	public List<EnergyEfficiencyMeasure> appliedEEMs;
	public Vector3 interactionPos;
	public List<PoseSlot> posePositions = new List<PoseSlot>();

	[System.Serializable]
	public class EEMMeta {
		public string title;
		public EnergyEfficiencyMeasure eem;
		public bool applied;
	}

	void Awake() {
		InteractionPoint ip = GetComponentInChildren<InteractionPoint>();
		if(ip != null) {
			interactionPos = ip.transform.position;
		} else {
			DebugManager.Log("didn't find interaction point for " +
				this.title + " with name " + this.name + ", usig the gameObjects transform instead", this);
			interactionPos = transform.position;
		}
	}

	// Use this for initialization
	void Start() {
		_ctrlMgr = PilotController.GetInstance();
		_pilotView = PilotView.GetInstance();
		_applianceManager = ApplianceManager.GetInstance();
		_eemMgr = EnergyEfficiencyMeasureContainer.GetInstance();

		_isTopAppliance = !GetComponentInParent<ApplianceSlot>();

		if(_isTopAppliance) {
			_zone = GetComponentInParent<Room>();
		} else {
			_zone = transform.parent.parent.GetComponentInParent<Room>();
		}
		if(_zone) {
			_zone.AddAppliance(this);
		}

		//if(!transform.parent.GetComponent<ApplianceSlot>()) {
		//if(_applianceManager && _isTopAppliance) {
		if(_applianceManager) {
			_applianceManager.AddAppliance(this);
		}

		//Setting the posePositions for this appliance. Retrieving them from the transforms of the PosePoint components in the gameobject.
		PosePoint[] poseArray = GetComponentsInChildren<PosePoint>();

		foreach(PosePoint point in poseArray) {
			PoseSlot item = new PoseSlot();
			item.position = point.transform.position;
			item.rotation = point.transform.rotation;
			posePositions.Add(item);
		}

		//RefreshSlots();
	}

	//Called before destroyed
	void OnDestroy() {
		//if(_applianceManager && !transform.parent.GetComponent<ApplianceSlot>()) {
		//if(_applianceManager && _isTopAppliance) {
		if(_applianceManager) {
			_applianceManager.RemoveAppliance(this);	
		}
		if(_zone) {
			_zone.RemoveAppliance(this);
		}
		//if(_applianceManager && _applianceManager.ContainsAppliance(this)) {
		//	_applianceManager.RemoveAppliance(GetComponent<Appliance>());
		//}
	}

	// Update is called once per frame
	void Update() {
	}

	//Callback for when the appliance is selected
	public void OnPointerClick(PointerEventData eventData) {
		if(!isPilot) {
			_ctrlMgr.SetCurrentUI(this);
		}
	}

	//
	public void RefreshSlots() {
		ApplianceSlot[] slots = GetComponentsInChildren<ApplianceSlot>();
		foreach(ApplianceSlot slot in slots) {
			Appliance oldApp = slot.GetComponentInChildren<Appliance>();
			
			GameObject newAppObj = Instantiate(slot.appliancePrefabs[slot.currentApplianceIndex]);
			newAppObj.transform.SetParent(slot.transform, false);
			newAppObj.AddComponent<UniqueId>();

			if(oldApp) {
				string uid = oldApp.gameObject.GetComponent<UniqueId>().uniqueId;
				newAppObj.GetComponent<UniqueId>().uniqueId = uid;
				DestroyImmediate(oldApp.gameObject);
			}

			//ElectricDevice removedDevice = slot.transform.GetComponentInChildren<ElectricDevice>();
			//if(removedDevice) {
			//	DestroyImmediate(removedDevice.gameObject);
			//}

			//if(slot.appliancePrefabs[slot.currentApplianceIndex]) {
			//	GameObject newApp = Instantiate(slot.appliancePrefabs[slot.currentApplianceIndex]);
			//	newApp.transform.SetParent(slot.transform, false);
			//}
		}
	}

	//
	public GameObject ApplyEEM(EnergyEfficiencyMeasure eem) {
		GameObject newAppGO = gameObject;

		if(!eem.multipleUse) {
			appliedEEMs.Insert(appliedEEMs.Count, eem);
		}

		_applianceManager.AddEEMTitle(eem.name);

		if(eem.replacementPrefab != null) {
			newAppGO = Instantiate(eem.replacementPrefab);
			newAppGO.transform.SetParent(transform.parent, false);
			newAppGO.transform.localPosition = transform.localPosition;
			newAppGO.transform.localRotation = transform.localRotation;
			newAppGO.gameObject.layer = gameObject.layer;

			ElectricDevice edOld = GetComponent<ElectricDevice>();
			ElectricDevice edNew = newAppGO.GetComponent<ElectricDevice>();
			edNew.DefaultRunlevel = edOld.runlevel == edOld.runlevelOn ? edNew.runlevelOn : edNew.runlevelOff;

			_applianceManager.ReplaceAppliance(this, newAppGO.GetComponent<Appliance>());
			Destroy(gameObject);
		}

		if(eem.setEnergyEffeciency) {
			ElectricDevice ed = GetComponent<ElectricDevice>();
			if(ed) {
				ed.energyEffeciency = eem.energyEffeciency;
				foreach(Runlevel runlevel in ed.runlevels) {
					runlevel.SetPowerByEE(eem.energyEffeciency);
				}
				ed.SetRunlevel(ed.runlevel);

				_pilotView.BuildDevicePanel(this);
			}
		}

		if(GetComponent<ElectricDevice>()) {
			ElectricDevice device = GetComponent<ElectricDevice>();
			device.SetEnergyMod(device.GetEnergyMod() - eem.energyFactor);
		}

		return newAppGO;
	}

	//
	public void RefreshEEMs() {
		//Determine highest replacement eem
		EnergyEfficiencyMeasure lastReplacement = null;
		foreach(EnergyEfficiencyMeasure eem in appliedEEMs) {
			if(eem.replacementPrefab != null) {
				lastReplacement = eem;
			}
		}
		//If there was a last replacement eem and it wasn't to the current type
		if(lastReplacement != null && lastReplacement.replacementPrefab.GetComponent<Appliance>().title != title) {
			GameObject newAppGO = 
				Instantiate(lastReplacement.replacementPrefab, transform.position, transform.rotation, transform.parent);
			newAppGO.layer = gameObject.layer;

			ElectricDevice edOld = GetComponent<ElectricDevice>();
			ElectricDevice edNew = newAppGO.GetComponent<ElectricDevice>();
			edNew.DefaultRunlevel = edOld.runlevel == edOld.runlevelOn ? edNew.runlevelOn : edNew.runlevelOff;

			_applianceManager.ReplaceAppliance(this, newAppGO.GetComponent<Appliance>());
			newAppGO.GetComponent<Appliance>().appliedEEMs = appliedEEMs;
			newAppGO.GetComponent<Appliance>().RefreshEEMs();
			Destroy(gameObject);
		}

	}

	//If Avatar, challenge to a battle
	public void Challenge() {
		_ctrlMgr.ChallengeAvatar(this);
		//_ctrlMgr.PrepareForBattle(this);
		//_ctrlMgr.LoadScene("BattleScene");
	}

	//
	public void SRCAvatarIsBattleReady(CallbackResult result) {
		result.result = GetComponent<BehaviourAI>().battleReady;
	}

	//
	public List<EnergyEfficiencyMeasure> GetEEMs() {
		//List<
		//if(_isTopAppliance) {
		//}
		return playerAffordances;
	}

	//
	public void OnUsage(Affordance affordance) {
		AddHarvest();
	}

	//
	public void AddHarvest() {
		//_harvestButton.SetActive(true);
	}

	//
	public bool DecreaseNrOfAffordanceSlots(Affordance affordance, int count = 1) {
		for(int i = 0; i < avatarAffordances.Count; i++) {
			if(avatarAffordances[i].affordance == affordance) {
				AffordanceResource resource = avatarAffordances[i];

				int margin = resource.nrOfSlots - resource.usedSlots();
				int mustBeRemoved = count - margin;

				if(mustBeRemoved < 0)
					mustBeRemoved = 0;

				for(int j = 0; j < mustBeRemoved; j++) {
					resource.subscribingActivities[0].LoseAffordanceSlot();
				}

				resource.nrOfSlots -= count;
				if(resource.nrOfSlots < 0) {
					resource.nrOfSlots = 0;
					return false;
				}
				return true;
			}
		}
		return false;
	}

	//
	public bool IncreaseNrOfAffordanceSlots(Affordance affordance, int count = 1) {
		for(int i = 0; i < avatarAffordances.Count; i++) {
			if(avatarAffordances[i].affordance == affordance) {
				avatarAffordances[i].nrOfSlots += count;
				return true;
			}
		}
		return false;
	}

	//
	public bool RemoveAllUsersOfAffordance(Affordance affordance) {
		foreach(AffordanceResource affordanceResource in avatarAffordances) {
			if(affordanceResource.affordance == affordance) {
				return affordanceResource.RemoveAllUsers();

			}
		}
		return false;
	}

	//
	public bool TakeAffordanceSlot(Affordance affordance, AvatarActivity activity) {
		foreach(AffordanceResource affordanceResource in avatarAffordances) {
			if(affordanceResource.affordance == affordance) {
				return affordanceResource.AddUser(activity);
			}
		}
		return false;
	}

	//
	public bool ReleaseAffordanceSlot(Affordance affordance, AvatarActivity activity) {
		foreach(AffordanceResource affordanceResource in avatarAffordances) {
			if(affordanceResource.affordance == affordance) {
				return affordanceResource.RemoveUser(activity);
			}
		}
		return false;
	}

	//Releases all pose slots that are currently occupied by the supplied occupant
	public void ReleasePoseSlot(BehaviourAI occupant) {
		foreach(PoseSlot slot in posePositions) {
			if(slot.occupant == occupant) {
				slot.occupant = null;
			}
		}
	}

	//Get the zone where the appliance iz @
	public Room GetZone() {
		return _zone;
	}

	//Get the uid string if appliance has a uid component
	public string GetUniqueId() {
		UniqueId uid = GetComponent<UniqueId>();
		if(uid) {
			return uid.uniqueId;
		}
		return "";
	}

	//For a given eem, returns whether its been applied on this appliance
	public bool IsEEMApplied(EnergyEfficiencyMeasure eem) {
		return appliedEEMs.Contains(eem);
	}

	//Is appliance child of an appliance group within a slot or a "top" appliance
	public bool IsTopAppliance() {
		return _isTopAppliance;
	}

	//
	public JSONClass SerializeAsJSON() {
		JSONClass json = new JSONClass();

		json.Add("id", GetComponent<UniqueId>().uniqueId);
		json.Add("title", title);

		JSONArray eemsJSON = new JSONArray();
		foreach(EnergyEfficiencyMeasure eem in appliedEEMs) {
			JSONClass eemJSON = new JSONClass();
			eemJSON.Add("name", eem.name);
			eemsJSON.Add(eemJSON);
		}
		json.Add("appliedEEMs", eemsJSON);

		return json;
	}

	//
	public void DeserializeFromJSON(JSONClass json) {
		if(json != null) {
			JSONArray eemsJSON = json["appliedEEMs"].AsArray;
			foreach(JSONClass appliedEEMJSON in eemsJSON) {
				appliedEEMs.Add(_eemMgr.GetEEM(appliedEEMJSON["name"]));
			}
			RefreshEEMs();
		}
	}
}
