﻿using UnityEngine;
using System.Collections;

[CreateAssetMenu(fileName = "Actions", menuName = "Actions/Base Action", order = 1)]
public class BaseAction : ScriptableObject {
	[TextArea(3, 10)]
	public string actionName;

	public string callback;
	public string callbackArgument;

	public string deactivateDeviceName;
	public string activateDeviceName;

	public float energyFactor = 0;
	public float gasFactor = 0;
	public float co2Factor = 0;
	public float moneyFactor = 0;
	public float comfortFactor = 0;

	public int cashCost;
	public int comfortCost;

	public int cashProduction;
	public int comfortPorduction;

	public bool performed;
	public bool passive;
	public bool hidden;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
