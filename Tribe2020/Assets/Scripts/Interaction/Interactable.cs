using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Interactable : MonoBehaviour{
	public string type = "techie";

	private List<Action> _actions;

	// Use this for initialization
	void Start(){
		_actions = new List<Action>();

		foreach(Action a in GetComponents<Action>()){
			_actions.Add(a);
		}
	}
	
	// Update is called once per frame
	void Update(){
	}

	public List<Action> GetActions(){
		return _actions;
	}
}