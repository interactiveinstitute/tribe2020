using UnityEngine;
using System.Collections;
using System.Collections.Generic;

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

public class TimeSeries : Subscriber {
	[Header("General")]
	public string Name;
	[Space(10)]
	public double StartTime;
	public double StopTime;
	public bool Absolute = true;
	[Space(10)]
	public string ValueUnit;
	public string IntegralUnit;
	[Space(10)]
	public int BufferMaxSize;
	public bool isAsync = false;
	public bool hasIntegral = false;
	public bool isTextSeries = false;
	public int ReloadLimit = 0;


	[Header("Data manipulation")]
	public double ValueOffset = 0;
	public double ValueScaleFactor = 1;
	public double TimeOffset = 0;

	[Header("Status")]
	public bool UpdateStatus;
	[Space(10)]
	public bool BufferValid = false;
	public int CurrentIndex;
	public double CurrentValue;
	public double CurrentIntegral; 
	public string CurrentText;
	public int CurrentSize;

	[Header("Buffer")]
	public List<DataPoint> DataPoints = new List<DataPoint>();





	private GameTime TTime = null;



	// Use this for initialization
	void Start () {

		//Auto set if not set allready.
		if (Server == null)
			Server = MQTT.GetInstance ();
		TTime = GameTime.GetInstance ();
		RequestData ();


		//DataPoints [0] = new DataPoint ();
	}
	
	// Update is called once per frame
	void Update () {
		if (UpdateStatus == true) {
			CurrentValue = GetCurrentValue ();
			CurrentIndex = GetCurrentIndex (TTime.time);
		}
	}

	void Data_Update(DataPoint[] DataSet) {

		int index;

		foreach(var item in DataSet )
		{
			//Skip if not inside buffer. 
			if (item.Timestamp < this.getStartTime() || item.Timestamp > this.getStopTime())
				continue;

			index = DataPoints.BinarySearch (item, new CompareDataPoint() );

			//Insert
			if (index < 0)
			{
				DataPoints.Insert(~index, item);
			}
			//Replace
			else if (index > 0)
			{
					DataPoints.RemoveAt(index);
					DataPoints.Insert(index, item);
			}

		}

		//Maintain size limitation. 
		if (DataPoints.Count > BufferMaxSize)
			DataPoints.RemoveRange (BufferMaxSize, BufferMaxSize - DataPoints.Count);
	}

	public double getStartTime() {
		if (Absolute)
			return StartTime;

		return TTime.time + StartTime;
		
	}

	public double getStopTime() {
		if (Absolute)
			return StopTime;

		return TTime.time + StopTime; 
	}


	public bool RequestData () {
		if (Server == null)
			return false;

		Server.Get(Name,StartTime,Absolute,BufferMaxSize);

		return true;
	}

	//Get index based on a timestamp
	public int GetCurrentIndex(double ts) {

		for (int i = DataPoints.Count - 1; i >= 0 ; i--)
		{
			//print(TimeStamps [i].ToString ("F4") + " > " + ts.ToString ("F4"));

			if (DataPoints[i].Timestamp < ts)
			{
				return i;
			}
		}

		return -1;
	}
		
	//Get the current value form the timeseries based on the current gametime. 
	public double GetCurrentValue () {
		double now = TTime.time;

		int i = GetCurrentIndex (now);

		if (i == -1)
			return double.NaN;

		return DataPoints[i].Value;
	}


	//TODO 
	public double InterpolateCurrentValue() {
		double now = (double)TTime.time;

		return DataPoints[GetCurrentIndex(now)].Value;
	}

}
