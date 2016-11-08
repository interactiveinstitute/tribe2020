﻿using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using SimpleJSON;

public class Appliance : MonoBehaviour, IPointerClickHandler {
	private PilotController _ctrlMgr;

	public string title;
	public string description;
	public List<EnergyEfficiencyMeasure> playerAffordances;
	public List<AvatarActivity.Target> avatarAffordances;
	public List<string> owners;

	public List<EnergyEfficiencyMeasure> appliedEEMs;
	public List<BaseAction> performedEnergyMeasures;
	public Vector3 interactionPos;
	public float cashProduction;
	public float comfortPorduction;

	public GameObject harvestButtonRef;
	private GameObject _harvestButton;

	// Use this for initialization
	void Start() {
		_ctrlMgr = PilotController.GetInstance();

		_harvestButton = Instantiate(harvestButtonRef) as GameObject;
		_harvestButton.transform.position = transform.position + Vector3.up * 1.5f;
		_harvestButton.transform.SetParent(transform);

		_harvestButton.GetComponentInChildren<Button>().
				onClick.AddListener(() => _ctrlMgr.OnHarvestTap(_harvestButton));

		harvestButtonRef.SetActive(false);

		appliedEEMs = new List<EnergyEfficiencyMeasure>();

		InteractionPoint ip = GetComponentInChildren<InteractionPoint>();
		if(ip != null) {
			interactionPos = ip.transform.position;
		} else {
            //Debug.Log("didn't find interaction point for " + this.title + " with name " + this.name + ", usig the gameObjects transform instead");
			interactionPos = transform.position;
		}
	}
	
	// Update is called once per frame
	void Update() {
	}

	//
	public void OnPointerClick(PointerEventData eventData) {
		_ctrlMgr.SetCurrentUI(this);
	}

	//
	public void ApplyEEM(EnergyEfficiencyMeasure eem) {
		appliedEEMs.Add(eem);
	}

	//
	public void PerformAction(BaseAction action) {
		//if(playerAffordances.Contains(action) && !performedEnergyMeasures.Contains(action)) {
		//	performedEnergyMeasures.Add(action);
		//	cashProduction += action.cashProduction;
		//	comfortPorduction += action.comfortPorduction;
		//}

		//if(action.deactivateDeviceName != "") {
		//	foreach(Transform child in transform) {
		//		if(child.name == action.deactivateDeviceName) {
		//			child.gameObject.SetActive(false);
		//		}
		//		if(child.name == action.activateDeviceName) {
		//			child.gameObject.SetActive(true);
		//		}
		//	}
		//}
	}

	//
	public bool IsPerformed(BaseAction action) {
		return !performedEnergyMeasures.Contains(action);
	}

	//
	public List<BaseAction> GetPlayerActions() {
		//List<BaseAction> availableActions = new List<BaseAction>();
		//foreach(BaseAction action in playerAffordances) {
		//	if(!performedEnergyMeasures.Contains(action)) {
		//		availableActions.Add(action);
		//	}
		//}
		//return availableActions;
		return null;
	}

	//
	public List<EnergyEfficiencyMeasure> GetEEMs() {
		return playerAffordances;
	}

	//
	public void AddHarvest() {
		_harvestButton.SetActive(true);
	}

	//
	public JSONClass SerializeAsJSON() {
		JSONClass json = new JSONClass();

		json.Add("id", GetComponent<UniqueId>().uniqueId);
		json.Add("title", title);

		JSONArray eemsJSON = new JSONArray();
		foreach(EnergyEfficiencyMeasure eem in appliedEEMs) {
			JSONClass eemJSON = new JSONClass();
			eemJSON.Add("title", eem.title);
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
				foreach(EnergyEfficiencyMeasure eem in playerAffordances) {
					if(eem.title.Equals(appliedEEMJSON["title"])) {
						ApplyEEM(eem);
					}
				}
			}
		}
	}
}
