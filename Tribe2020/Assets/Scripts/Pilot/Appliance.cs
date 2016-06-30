﻿using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class Appliance : MonoBehaviour, IPointerClickHandler {
	private PilotController _ctrlMgr;

	public string title;
	public string description;
	public List<BaseAction> playerAffordances;
	public List<string> avatarAffordances;
	public List<string> owners;

	public List<BaseAction> performedActions;
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

		InteractionPoint ip = GetComponentInChildren<InteractionPoint>();
		if(ip != null) {
			interactionPos = ip.transform.position;
		} else {
			interactionPos = transform.position;
		}
	}
	
	// Update is called once per frame
	void Update() {
	}

	//
	public void OnPointerClick(PointerEventData eventData) {
		_ctrlMgr.OnDeviceSelected(this);
	}

	//
	public void PerformAction(BaseAction action) {
		if(playerAffordances.Contains(action) && !performedActions.Contains(action)) {
			performedActions.Add(action);
			cashProduction += action.cashProduction;
			comfortPorduction += action.comfortPorduction;
		}
	}

	//
	public bool IsPerformed(BaseAction action) {
		return !performedActions.Contains(action);
	}

	//
	public List<BaseAction> GetPlayerActions() {
		List<BaseAction> availableActions = new List<BaseAction>();
		foreach(BaseAction action in playerAffordances) {
			if(!performedActions.Contains(action)) {
				availableActions.Add(action);
			}
		}
		return availableActions;
	}

	//
	public void AddHarvest() {
		_harvestButton.SetActive(true);
	}
}