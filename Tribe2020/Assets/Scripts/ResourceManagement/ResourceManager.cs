using UnityEngine;
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
	private ResourceInterface _interface;

	[Header("Containers")]
	public float cash;
	public float comfort;
	public int temperature;
	public int power;
	public double CO2;
	public double CO2Change;
	public double Cost;
	public double CostSavings;
	private int _pendingCash;
	private double _lastHarvestTick;

	[Space(10)]
	public string CO2OutcomeName;
	public DataSeries CO2Outcome;
	[Space(10)]
	public string CO2BaselineName;
	public DataSeries CO2Baseline;
	[Space(10)]
	public DataSeries CO2ChangeSeries;
	[Space(10)]
	public DataSeries CostOutcome;
	[Space(10)]
	public DataSeries CostBaseline;
	[Space(10)]
	public string electricityBaselineName;
	public DataSeries electricityBaseline;
	[Space(10)]
	public string electricityOutcomeName;
	public DataSeries electricityOutcome;
	[Space(10)]
	public string gasBaselineName;
	public DataSeries gasBaseline;
	[Space(10)]
	public string gasOutcomeName;
	public DataSeries gasOutcome;
	[Space(10)]
	public GameTime.TimeContext timeContext;
	private double _currentTime = 0;

    [Header("Salary")]
    public int cashSalary;
    public int comfortSalary;

    [Header("Production")]
	public float cashProduction;
	public float comfortProduction;

	[Header("Properties")]
	public float happyComfortInterval = 300;
	public float euphoricComfrotInterval = 60;
	public float moodResetTime = 1000;
	public int comfortHarvestCount = 0;
	public int comfortHarvestMax = 30;

	[Header("Manipulations")]
	public List<Manipulation> Manipulations = new List<Manipulation>();
	public bool initsimulation = true;

	[Header("DEBUG")]
	public double c2outcome_debug, c2baseline_debug;
	public DataPoint CO2DataOutcome, CO2DataBaseline, CO2DataChange;

	[Space(10)]
	public double d_gas_out;
	public double d_gas_base;
	public double d_gas_diff;
	[Space(10)]
	public double d_el_out;
	public double d_el_base;
	public double d_el_diff;
	[Space(10)]
	public double d_co2_out;
	public double d_co2_base;
	public double d_CO2_diff;
	[Space(20)]
	public double d_30gas_out;
	public double d_30gas_base;
	public double d_30gas_diff;
	[Space(10)]
	public double d_30el_out;
	public double d_30el_base;
	public double d_30el_diff;
	[Space(10)]
	public double d_30co2_out;
	public double d_30co2_base;
	public double d_30CO2_diff;

	//public double d_gas30, d_gas_co230, d_el30, d_el_co230, d_co230;



	//Sort use instead of constructor
	void Awake() {
		_instance = this;


		//We need access to dataseries before they are written to do initialization.  
		if (CO2Outcome == null)
			CO2Outcome = DataSeries.GetSeriesByName (CO2OutcomeName);
		else
			CO2OutcomeName = CO2Outcome.NodeName;

		if (gasBaseline == null)
			gasBaseline = DataSeries.GetSeriesByName (gasBaselineName);
		else
			gasBaselineName = gasBaseline.NodeName;

		if (gasOutcome == null)
			gasOutcome = DataSeries.GetSeriesByName (gasOutcomeName);
		else
			gasOutcomeName = gasOutcome.NodeName;

		if (electricityBaseline == null)
			electricityBaseline = DataSeries.GetSeriesByName (electricityBaselineName);
		else
			electricityBaselineName = electricityBaseline.NodeName;

		if (electricityOutcome == null)
			electricityOutcome = DataSeries.GetSeriesByName (electricityOutcomeName);
		else
			electricityOutcomeName = electricityOutcome.NodeName;







	}

	// Use this for initialization
	void Start() {
		_uiMgr = PilotView.GetInstance();
		_timeMgr = GameTime.GetInstance();

		_avatars = new List<Transform>();
		_appliances = new List<Appliance>();

		RefreshProduction();

		if (initsimulation)
			FillHistoricalData ();

		DataContainer dataContainer = DataContainer.GetInstance();
		//CO2Outcome = dataContainer.cO2Outcome;
		CO2Baseline = dataContainer.cO2Baseline;
		CO2ChangeSeries = dataContainer.cO2ChangeSeries;
		CostOutcome = dataContainer.costOutcome;
		CostBaseline = dataContainer.costBaseline;
		//electricityOutcome = dataContainer.electricityOutcome;
		//gasOutcome = dataContainer.gasOutcome;


		//print (electricityOutcome.FistTimestamp());

		_currentTime = _timeMgr.time;
	}

	void FillHistoricalData(){



		double now = GameTime.GetInstance ().time;
		//DataNode.Subscription GhostSubscription;

		if (double.IsNaN (electricityOutcome.FirstTimestamp ())) {
			electricityOutcome.CopyPeriod (electricityBaseline, 0, now);
			DataPoint dp = electricityOutcome.GetLast ();
			//GhostSubscription = new DataNode.Subscription ();
			//dp.Values [0] = 0;
			//GhostSubscription.LastTransmission = dp;
			//electricityOutcome.Sources.Add (GhostSubscription);
			MainMeter meter = MainMeter.GetInstance();
		//	print (dp.Values [1]);
			meter.init (dp.Timestamp, dp.Values [0], dp.Values [1]);
			//meter.Energy = dp.Values [1];

			
		}

		if (double.IsNaN (gasOutcome.FirstTimestamp ())) {
			gasOutcome.CopyPeriod (gasBaseline, 0, now);

			//DataPoint dp = gasOutcome.GetLast ();
			//GhostSubscription = new DataNode.Subscription ();
			//dp.Values [0] = 0;
			//GhostSubscription.LastTransmission = dp;
			//gasOutcome.Sources.Add (GhostSubscription);
		}

	
	}

	//
	void CalculateCo2(double now) {
		if(CO2Outcome == null || CO2Baseline == null)
			return;

		//double now = _timeMgr.time;
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

        NewMonthTest();


		now = _timeMgr.time;

		d_gas_out = gasOutcome.GetDataAt(now).Values[1];
		d_gas_base = gasBaseline.GetDataAt(now).Values[1];
		d_gas_diff = d_gas_out - d_gas_base;

		d_el_out = electricityOutcome.GetDataAt(now).Values[1];
		d_el_base = electricityBaseline.GetDataAt(now).Values[1];
		d_el_diff = d_el_out - d_el_base;

		d_co2_out = CO2Outcome.GetDataAt(now).Values[1];
		d_co2_base= CO2Baseline.GetDataAt(now).Values[1];
		d_CO2_diff = d_co2_out - d_co2_base;



	}

	//
	public void SetInterface(ResourceInterface i) {
		_interface = i;
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
	public bool CanAfford(int costCash, int costComfort) {
		return cash >= costCash && comfort >= costComfort;
	}

	//Multiply given DataSeries
	public void ScaleData(string series, double scale, double timestamp) {
		ManipulateData (series, scale, 0, 0, timestamp);
		
	}

	//Absolute offset given DataSeries
	//Absoule offset removes the specified value from the results (original_data - absolute_offsets).
	public void AbsoluteOffsetData(string series, double offset, double timestamp) {
		ManipulateData (series, 1, offset,0, timestamp);
	}
		
	//Relative offset given DataSeries
	//A relative offset removes a certain percentage of the original data from the result (result - originaldata * relativeoffset). 
	public void RelativeOffsetData(string series, double offset, double timestamp) {
		ManipulateData (series, 1, 0,offset, timestamp);
	}

	//TODO: Manipulate given DataSeries
	public int ManipulateData(string series, double scale, double absoluteOffset, double relativeOffset, double timestamp) {

		DataManipulator ManipulationPoint = FindManipulationPoint (series);

		

		if (ManipulationPoint == null)
			return -1;

		//Create manipulation 
		Manipulation m = new Manipulation();

		m.SetRateCounter (scale, absoluteOffset, relativeOffset);


		//Add to manipulation point
		ManipulationPoint.Manipulations.Add(m);

		//Add to own tracking list
		Manipulations.Add(m);

		m.Activate(timestamp);


		return Manipulations.Count-1;
	}

	public DataManipulator FindManipulationPoint(string name){
		DataManipulator[] manipulations = FindObjectsOfType(typeof(DataManipulator)) as DataManipulator[];
		foreach (DataManipulator manipulation in manipulations) {
			if (manipulation.name == name)
				return manipulation;
				
		}

		return null;
	}



    //New month test
    void NewMonthTest() {
        int months = _timeMgr.GetNewMonths();
        if(months > 0) {
            AddCash(cashSalary * months);
            AddComfort(comfortSalary * months);
			_interface.OnResourcesReceived("" + cashSalary);
        }
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

		return json;
	}

	//
	public void DeserializeFromJSON(JSONClass json) {
		if(json != null) {
			cash = json["money"].AsInt;
			comfort = json["comfort"].AsInt;

			DeserializeDataseries(json["electricity"].AsArray, electricityOutcome);
			DeserializeDataseries(json["gas"].AsArray, gasOutcome);
		}
	}

	public void Test() {

		//print (FindManipulationPoint ("Heating"));

		double now = _timeMgr.time;
		ManipulateData ("Heating", 1, -100, 0, now);




	}
}