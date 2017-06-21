using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnergyEfficiencyMeasureContainer : MonoBehaviour {
	private static EnergyEfficiencyMeasureContainer _instance;
	public static EnergyEfficiencyMeasureContainer GetInstance() {
		return _instance;
	}
	public List<EnergyEfficiencyMeasure> envelope;
	public List<EnergyEfficiencyMeasure> HVAC;
	public List<EnergyEfficiencyMeasure> DHW;
	public List<EnergyEfficiencyMeasure> lighting;
	public List<EnergyEfficiencyMeasure> electricDevices;
	public List<EnergyEfficiencyMeasure> other;
	public List<EnergyEfficiencyMeasure> special;

	private Dictionary<string, EnergyEfficiencyMeasure> _eemLookup = new Dictionary<string, EnergyEfficiencyMeasure>();

	//
	void Awake() {
		_instance = this;

		foreach(EnergyEfficiencyMeasure eem in envelope) {
			if(_eemLookup.ContainsKey(eem.name)) {
				Debug.Log("already contains " + eem.name);
			}
			_eemLookup.Add(eem.name, eem);
		}
		foreach(EnergyEfficiencyMeasure eem in HVAC) {
			_eemLookup.Add(eem.name, eem);
		}
		foreach(EnergyEfficiencyMeasure eem in DHW) {
			_eemLookup.Add(eem.name, eem);
		}
		foreach(EnergyEfficiencyMeasure eem in lighting) {
			_eemLookup.Add(eem.name, eem);
		}
		foreach(EnergyEfficiencyMeasure eem in electricDevices) {
			_eemLookup.Add(eem.name, eem);
		}
		foreach(EnergyEfficiencyMeasure eem in other) {
			_eemLookup.Add(eem.name, eem);
		}
		foreach(EnergyEfficiencyMeasure eem in special) {
			_eemLookup.Add(eem.name, eem);
		}
	}

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	//
	public EnergyEfficiencyMeasure GetEEM(string eemName) {
		if(!_eemLookup.ContainsKey(eemName)) {
			Debug.Log("does not contain " + eemName);
		}
		return _eemLookup[eemName];
	}
}
