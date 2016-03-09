using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Interactable : MonoBehaviour{
	public List<Action> avatarActions;
	public List<Action> playerActions;
	public List<BaseAction> actions;

	// Use this for initialization
	void Start(){
			
	}
	
	// Update is called once per frame
	void Update(){
	}

	public List<Action> GetPlayerActions() {
		return new List<Action>(GetComponents<Action>());
	}
}