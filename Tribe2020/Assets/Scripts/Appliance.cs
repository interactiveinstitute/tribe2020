using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class Appliance : MonoBehaviour {
	private InteractionManager _ixnMgr;

	public string title;
	public string description;
	public List<BaseAction> playerAffordances;
	public List<string> avatarAffordances;

	public List<BaseAction> performedActions;
	public float cashProduction;
	public float comfortPorduction;

	public GameObject harvestButtonRef;

	// Use this for initialization
	void Start () {
		_ixnMgr = InteractionManager.GetInstance();

		GameObject harvestButton = Instantiate(harvestButtonRef) as GameObject;
		harvestButton.transform.position = transform.position + Vector3.up * 0.5f;
		harvestButton.transform.SetParent(transform);
		
		harvestButton.GetComponentInChildren<Button>().
				onClick.AddListener(() => _ixnMgr.OnHarvestTap(harvestButton));

		harvestButtonRef.SetActive(true);
	}
	
	// Update is called once per frame
	void Update () {
	
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
}
