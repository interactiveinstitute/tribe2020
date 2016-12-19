﻿using UnityEngine;
using System.Collections.Generic;

public class AvatarManager : MonoBehaviour {
	private List<BehaviourAI> _avatars;

	// Use this for initialization
	void Start () {
		_avatars = new List<BehaviourAI>(UnityEngine.Object.FindObjectsOfType<BehaviourAI>());
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	//
	public void OnLightToggled(ElectricMeter meter, Room zone) {
		List<BehaviourAI> occupants = zone.GetOccupants();

        if (occupants.Count == 0) return;
		//Let's try to only notify one avatar. So we don't get several characters running for the light switch
		occupants[0].CheckLighting(AvatarActivity.SessionType.TurnOn);
	}
}
