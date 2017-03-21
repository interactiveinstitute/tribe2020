using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DataContainer : MonoBehaviour {
	private static DataContainer _instance;
	public static DataContainer GetInstance() {
		return _instance;
	}

	public DataSeries cO2Outcome;
	public DataSeries cO2Baseline;
	public DataSeries cO2Gas;
	public DataSeries cO2Electricity;
	public DataSeries cO2ChangeSeries;
	public DataSeries costOutcome;
	public DataSeries costBaseline;
	public DataSeries electricityOutcome;
	public DataSeries gasOutcome;

	//
	void Awake() {
		_instance = this;
	}

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	//
	public DataSeries GetSeriesByName(string name) {
		switch(name) {
			case "CO2Outcome": return cO2Outcome;
			case "CO2Baseline": return cO2Baseline;
			case "CO2Gas": return cO2Gas;
			case "CO2Electricty": return cO2Electricity;
			case "CO2ChangeSeries": return cO2ChangeSeries;
			case "CostOutcome": return costOutcome;
			case "CostBaseline": return costBaseline;
			case "ElectricityOutcome": return electricityOutcome;
			case "GasOutcome": return gasOutcome;
			default: return null;
		}
	}
}
