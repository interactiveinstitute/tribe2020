using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class CompareDataPoint : IComparer<DataPoint>
{
	static IComparer<DataPoint> comparer = new CompareDataPoint();

	public int Compare(DataPoint x, DataPoint y)
	{
		if (x == y)    return 0;
		if (x == null) return -1;
		if (y == null) return 1;
		if (x.Timestamp > y.Timestamp)
			return -1;
		if (x.Timestamp < y.Timestamp)
			return 1;

		return 0;
	}
}

public class TimeSeries : DataModifier {
	[Header("Timeseries properties")]
	public double StartTime;
	public double StopTime;
	public bool Relative = false;


	[Space(10)]
	public int BufferMaxSize;
	public bool isAsync = false;
	public bool hasIntegral = false;
	public bool isTextSeries = false;
	public int ReloadLimit = 0;

	[Header("Status")]
	public bool Enabled;
	[Space(10)]
	public bool BufferValid = false;
	public int CurrentIndex;
	public double CurrentTimestamp;
	public string CurrentDate;
	public double CurrentValue;
	public double CurrentIntegral; 
	public string CurrentText;
	public int CurrentSize;


	[Header("Buffer")]
//	public int Pointer = 0;
//	private int lastPointer = -1;
//	public List<DataPoint> Viewer = null;
	public List<DataPoint> DataPoints = new List<DataPoint>();


	[Header("CSV file")]
	public TextAsset File;



	private GameTime TTime = null;





	// Use this for initialization
	void Start () {

		CurrentIndex = -1;

		//Auto set if not set allready.
		//if (Server == null)
		//	Server = MQTT.GetInstance ();
		TTime = GameTime.GetInstance ();
		RequestData ();


		//DataPoints [0] = new DataPoint ();
	}
	
	// Update is called once per frame
	void Update () {
		if ( Enabled == true) {
			int index = CurrentIndex;

			CurrentIndex = GetIndex (TTime.time);

			if (CurrentIndex == index)
				return;
				

			CurrentValue = GetCurrentValue ();


			if (CurrentIndex == -1 ) {
				CurrentTimestamp = double.NaN;
				CurrentDate = "Out of range";
				return;

			
			
			}

			CurrentTimestamp = DataPoints [CurrentIndex].Timestamp;
			CurrentDate = TTime.TimestampToDateTime(CurrentTimestamp).ToString("yyyy-MM-dd HH:mm:ss");

			DataPoint Data = DataPoints [CurrentIndex].Clone ();
			Data.Timestamp += TimeOffset;
			UpdateAllTargets (Data);

		}
	}

	void AddPoint(DataPoint Data) {
		int index;

		//Skip if not inside buffer. 
		if (Data.Timestamp < this.getStartTime () || Data.Timestamp > this.getStopTime ()) {
			//If limits are both zero we assume its not usex
			if (StartTime != 0 && StopTime != 0)
				return;

		}

		index = DataPoints.BinarySearch (Data, new CompareDataPoint() );

		//Insert
		if (index < 0)
		{
			DataPoints.Insert(~index, Data);
		}
		//Replace
		else if (index > 0)
		{
			DataPoints.RemoveAt(index);
			DataPoints.Insert(index, Data);
		}

		//Maintain size limitation. 
		if (BufferMaxSize != 0 && DataPoints.Count > BufferMaxSize)
			DataPoints.RemoveRange (BufferMaxSize, BufferMaxSize - DataPoints.Count);

	}


	void AddPoints(DataPoint[] DataSet) {

		foreach(var item in DataSet )
		{
			AddPoint (item);
		}
			
	}

	public double getStartTime() {
		if (!Relative)
			return StartTime;

		return TTime.time + StartTime;
		
	}

	public double getStopTime() {
		if (!Relative)
			return StopTime;

		return TTime.time + StopTime; 
	}


	public bool RequestData () {
		//if (Server == null)
		//	return false;

		//Server.Get(Name,StartTime,Absolute,BufferMaxSize);

		return true;
	}

	//Get index based on a timestamp
	public int GetIndex(double ts) {

		for (int i = DataPoints.Count - 1; i >= 0 ; i--)
		{
			//print(TimeStamps [i].ToString ("F4") + " > " + ts.ToString ("F4"));

			if ((DataPoints[i].Timestamp + TimeOffset) <= ts )
			{
				return i;
			}
		}

		return -1;
	}
		
	//Get the current value form the timeseries based on the current gametime. 
	public double GetCurrentValue () {
		double now = TTime.time;

		int i = GetIndex (now);

		if (i == -1)
			return double.NaN;

		return ApplyModifiers(DataPoints[i]).Values[0];
	}

	public double[] GetCurrentValues () {
		double now = TTime.time;

		int i = GetIndex (now);

		if (i == -1)
			return null;

		return ApplyModifiers(DataPoints[i]).Values;
	}


	//TODO 
	public double InterpolateCurrentValue() {
		double now = (double)TTime.time;

		return DataPoints[GetIndex(now)].Values[0];
	}


	public void LoadFromCVSFile() {

		if (File == null)
			return;

		//string fileData  = System.IO.File.ReadAllText(FileName);
		string[] lines = File.text.Split("\n"[0]);

		Columns = (lines[0].Trim()).Split(","[0]);
		Units = (lines[1].Trim()).Split(","[0]);

		Name = File.name;
		Relative = false;


		//TODO fill in when we have a buffer already... 
		//if (BufferValid == true)
		//	return;

		//Reload everything. 
		DataPoints.Clear ();
		double tsmin = double.PositiveInfinity, tsmax=0;

		for (int i = 2; i < lines.Length; i++) {
			string[] Values = (lines[i].Trim()).Split(","[0]);
			DataPoint data = new DataPoint();
			data.Timestamp = double.Parse( Values[0]) ;



			data.Values = new double[Values.Length-1]; 

			for (int c = 1; c < Values.Length; c++) {
				
//				Debug.Log (data);
//				Debug.Log (data.Values[c]);
//				Debug.Log (Values [c]);

				data.Values[c-1] = double.Parse (Values [c]);
			}

			DataPoints.Add (data);

			//Save min and max. 
			if (data.Timestamp > tsmax)
				tsmax = data.Timestamp;
			if (data.Timestamp < tsmin)
				tsmin = data.Timestamp;
		}

		StartTime = tsmin;
		StopTime = tsmax;

		BufferValid = true;



		
	}

	public bool TsWithinBuffer(double TimeStamp) {
	
		double start, stop;

		start = getStartTime ();
		stop = getStopTime ();

		if (TimeStamp > stop)
			return false;
		if (TimeStamp < start)
			return false;

		return true;
			
	}

	//Make sure that the number of datapoints does not exceed the max buffer size variable. 
	public void TrimDatapoints() {
		int excess;

		//If zero or less then no restrictions apply
		if (BufferMaxSize < 1)
			return;

		excess = DataPoints.Count - BufferMaxSize;

		if (excess > 0)
			DataPoints.RemoveRange (0, excess);
	}

	override public void TimeDataUpdate(Connection Con,DataPoint data) {
		if (TsWithinBuffer(data.Timestamp)) {
			DataPoints.Add(data);
			TrimDatapoints();
			UpdateAllTargets (data);
		}
	}

	public List<DataPoint> GetPeriod(double From, double To) {
		int iFrom, iTo;

		iFrom = GetIndex (From) -1;
		iTo = GetIndex (To);
	

		if (iFrom < 0)
			iFrom = 0;
		if (iTo < 0)
			iTo = 0;

		return GetRange(iFrom, iTo - iFrom + 2);

	}

	public List<DataPoint> GetRange(int index,int count){

		List<DataPoint> rawdata, newdata;

		if (index >= DataPoints.Count)
			index = DataPoints.Count-1;

		if ((count + index ) > DataPoints.Count)
			count = DataPoints.Count - index;

		if (index < 0)
			return new List<DataPoint>(0);


		//Debug.Log ("I: " + index.ToString());
		//Debug.Log ("C: " + count.ToString());

		rawdata = DataPoints.GetRange(index,count);
		newdata = new List<DataPoint>(rawdata.Count);

		//if (TimeOffset == 0)
		//	return rawdata;

		foreach (DataPoint point in rawdata) {
			newdata.Add (ApplyModifiers(point));
		}

		return newdata;

	}

	public void Clear() {
		DataPoints.Clear ();
	}
		
		
}
