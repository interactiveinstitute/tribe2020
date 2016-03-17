using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Appliance : MonoBehaviour {
	public string title;
	public string description;
	public List<BaseAction> playerAffordances;
	public List<string> avatarAffordances;

	public List<BaseAction> performedActions;
	public float cashProduction;
	public float comfortPorduction;

	// Use this for initialization
	void Start () {
	
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
