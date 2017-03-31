using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnergyEfficiencyMeasureContainer : MonoBehaviour {
	public List<EnergyEfficiencyMeasure> envelope;
	public List<EnergyEfficiencyMeasure> HVAC;
	public List<EnergyEfficiencyMeasure> DHW;
	public List<EnergyEfficiencyMeasure> lighting;
	public List<EnergyEfficiencyMeasure> electricDevices;
	public List<EnergyEfficiencyMeasure> other;
	public List<EnergyEfficiencyMeasure> special;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
