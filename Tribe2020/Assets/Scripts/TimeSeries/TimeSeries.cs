using UnityEngine;
using System.Collections;


public class TimeSeries : MonoBehaviour {
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
	public double[] Values;
	public double[] Integral;
	public double[] TimeStamps;	

	[Header("Server")]
	public ServerObject Server = null;
	private GameTime TTime = null;
	[Space(10)]
	public string Topic;
	public string Subproperty;



	// Use this for initialization
	void Start () {
		//Auto set if not set allready.
		if (Server == null)
			Server = MQTT.GetInstance ();
		TTime = GameTime.GetInstance ();
		RequestData ();
	}
	
	// Update is called once per frame
	void Update () {
		if (UpdateStatus == true) {
			CurrentValue = GetCurrentValue ();
			CurrentIndex = GetCurrentIndex (TTime.time);
		}
	}

	public bool RequestData () {
		if (Server == null)
			return false;

		Server.Request(this,Name,StartTime,Absolute,BufferMaxSize);

		return true;
	}

	//Get index based on a timestamp
	public int GetCurrentIndex(double ts) {

		for (int i = TimeStamps.Length - 1; i >= 0 ; i--)
		{
			//print(TimeStamps [i].ToString ("F4") + " > " + ts.ToString ("F4"));

			if (TimeStamps[i] < ts)
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

		return Values[i];
	}


	//TODO 
	public double InterpolateCurrentValue() {
		double now = (double)TTime.time;

		return Values[GetCurrentIndex(now)];
	}

}
