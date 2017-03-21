﻿using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using SimpleJSON;

public class ResourceManager : MonoBehaviour {
	//Singleton features
	private static ResourceManager _instance;
	public static ResourceManager GetInstance() {
		return _instance;
	}

	private PilotView _uiMgr;
	private GameTime _timeMgr;
	private List<Transform> _avatars;
	private List<Appliance> _appliances;

	[Header("Containers")]
	public float cash;
	public float comfort;
	public int temperature;
	public int power;
	public double CO2;
	public double CO2Change;
	public double Cost;
	public double CostSavings;

	public DataSeries CO2Outcome;
	public DataSeries CO2Baseline;
	public DataSeries CO2ChangeSeries;
	public DataSeries CostOutcome;
	public DataSeries CostBaseline;
	public DataSeries electricityOutcome;
	public DataSeries gasOutcome;
	public GameTime.TimeContext timeContext;
	private double _currentTime = 0;

	[Header("Production")]
	public float cashProduction;
	public float comfortProduction;

	[Header("Properties")]
	public float happyComfortInterval = 300;
	public float euphoricComfrotInterval = 60;
	public float moodResetTime = 1000;
	public int comfortHarvestCount = 0;
	public int comfortHarvestMax = 30;


	[Header("DEBUG")]
	public double c2outcome_debug, c2baseline_debug;
	public DataPoint CO2DataOutcome, CO2DataBaseline, CO2DataChange;


	//Sort use instead of constructor
	void Awake() {
		_instance = this;
	}

	// Use this for initialization
	void Start() {
		_uiMgr = PilotView.GetInstance();
		_timeMgr = GameTime.GetInstance();

		_avatars = new List<Transform>();
		_appliances = new List<Appliance>();

		RefreshProduction();

		DataContainer dataContainer = DataContainer.GetInstance();
		CO2Outcome = dataContainer.cO2Outcome;
		CO2Baseline = dataContainer.cO2Baseline;
		CO2ChangeSeries = dataContainer.cO2ChangeSeries;
		CostOutcome = dataContainer.costOutcome;
		CostBaseline = dataContainer.costBaseline;
		electricityOutcome = dataContainer.electricityOutcome;
		gasOutcome = dataContainer.gasOutcome;
		_currentTime = _timeMgr.time;
	}

	//
	void CalculateCo2(double ts) {
		if(CO2Outcome == null || CO2Baseline == null)
			return;

		double now = GameTime.GetInstance().time;
		List<DataPoint> data_oucome, data_baseline, data_change;

		data_oucome = CO2Outcome.GetPeriod(now - 3 * 3600, now);
		data_baseline = CO2Baseline.GetPeriod(now - 3 * 3600, now);
		data_change = CO2ChangeSeries.GetPeriod(now - 3 * 3600, now);

		//CO2DataOutcome = CO2Outcome.GetDataAt(ts);
		//CO2DataBaseline = CO2Baseline.GetDataAt(ts);
		if(data_oucome.Count > 0)
			CO2DataOutcome = data_oucome[data_oucome.Count - 1];
		//     else
		//print("no data!");

		if(data_baseline.Count > 0)
			CO2DataBaseline = data_baseline[data_baseline.Count - 1];
		//     else
		//print("no data!");

		if(data_change.Count > 0)
			CO2DataChange = data_change[data_change.Count - 1];
		//     else
		//print("no data!");

		CO2 = CO2DataOutcome.Values[1];

		c2outcome_debug = CO2DataOutcome.Values[1];
		c2baseline_debug = CO2DataBaseline.Values[1];


		if(CO2DataChange.Values != null)
			CO2Change = CO2DataChange.Values[1];

	}

	//
	void CalculateCost(double ts) {
		DataPoint CostDataOutcome, CostDataBaseline;

		if(CostOutcome == null || CostBaseline == null)
			return;

		CostDataOutcome = CostOutcome.GetDataAt(ts);
		CostDataBaseline = CostBaseline.GetDataAt(ts);

		Cost = CostDataOutcome.Values[1];
		CostSavings = CostDataBaseline.Values[1] - CostDataOutcome.Values[1];

	}

	// Update is called once per frame
	void Update() {
		//co2 = 
		double now = _timeMgr.time;
		CalculateCo2(now);
		CalculateCost(now);

	}

	//
	public void RefreshProduction() {
		_avatars.Clear();
		_appliances.Clear();
		cashProduction = 0;

		foreach(GameObject avatarObj in GameObject.FindGameObjectsWithTag("Avatar")) {
			_avatars.Add(avatarObj.transform);
			cashProduction += 1;
			comfortProduction += 1;
		}

		foreach(GameObject applianceObj in GameObject.FindGameObjectsWithTag("Appliance")) {
			foreach(Appliance appliance in applianceObj.GetComponents<Appliance>()) {
				_appliances.Add(appliance);
				//if(action.performed){
				//	cashProduction += action.cashProduction;
				//}
			}
		}
	}

	public void AddComfort(int value) {
		comfort += value;
	}

	public void AddComfort(Gem gem) {
		comfort += gem.value;
	}

	public void AddCash(int value) {
		cash += value;
	}

	//
	public void RefreshProductionForAppliance(GameObject go) {

	}

	//
	public JSONArray SerializeDataseries(DataSeries ds) {
		JSONArray dsJSON = new JSONArray();
		if(ds.GetData() != null) {
			foreach(DataPoint dp in ds.GetData()) {
				JSONClass dpJSON = new JSONClass();
				dpJSON.Add("ts", "" + dp.Timestamp);
				JSONArray valuesJSON = new JSONArray();
				foreach(double v in dp.Values) {
					valuesJSON.Add("" + v);
				}
				dpJSON.Add("values", valuesJSON);
				dsJSON.Add(dpJSON);
			}
		}
		return dsJSON;
	}

	//
	public void DeserializeDataseries(JSONArray dsJSON, DataSeries ds) {
		foreach(JSONClass dpJSON in dsJSON) {
			DataPoint dp = new DataPoint();
			dp.Timestamp = dpJSON["ts"].AsDouble;
			int valueCount = dpJSON["values"].AsArray.Count;
			double[] values = new double[valueCount];
			for(int i = 0; i < valueCount; i++) {
				values[i] = dpJSON["values"].AsArray[i].AsDouble;
			}
			dp.Values = values;
			ds.InsertData(dp);
		}
	}

	//
	public JSONClass SerializeAsJSON() {
		JSONClass json = new JSONClass();

		json.Add("money", cash.ToString());
		json.Add("comfort", comfort.ToString());

		json.Add("electricity", SerializeDataseries(electricityOutcome));
		json.Add("gas", SerializeDataseries(gasOutcome));
		//json.Add("CO2Outcome", SerializeDataseries(CO2Outcome));
		//json.Add("CO2Baseline", SerializeDataseries(CO2Baseline));
		//json.Add("CO2ChangeSeries", SerializeDataseries(CO2ChangeSeries));
		//json.Add("CostOutcome", SerializeDataseries(CostOutcome));
		//json.Add("CostBaseline", SerializeDataseries(CostBaseline));

		return json;
	}

	//
	public void DeserializeFromJSON(JSONClass json) {
		if(json != null) {
			cash = json["money"].AsInt;
			comfort = json["comfort"].AsInt;

			DeserializeDataseries(json["electricity"].AsArray, electricityOutcome);
			DeserializeDataseries(json["gas"].AsArray, gasOutcome);

			//DeserializeDataseries(json["CO2Outcome"].AsArray, CO2Outcome);

			//JSONArray co2outcomeJSON = json["CO2Outcome"].AsArray;
			//foreach(JSONClass datapointJSON in co2outcomeJSON) {
			//	DataPoint dp = new DataPoint();
			//	dp.Timestamp = datapointJSON["ts"].AsDouble;
			//	dp.Values.SetValue(datapointJSON["value"].AsDouble, 0);
			//	CO2Outcome.InsertData(dp);
			//}
		}
	}
}