using UnityEngine;
using System.Collections;


public class TimeSeries : MonoBehaviour {

	public string Name;
	public double StartTime;
	public bool Absolute = true;

	public int BufferSize;
	public bool isAsync = false;

	public bool BufferValid = false;

	public bool Debug;
	public int CurrentIndex;
	public double CurrentValue;


	public double[] Values;
	public double[] TimeStamps;	

	public ServerConnection Server = null;
	private GameTime TTime = null;

	// Use this for initialization
	void Start () {

		Server = ServerConnection.GetInstance ();
		TTime = GameTime.GetInstance ();
		RequestData ();
	}
	
	// Update is called once per frame
	void Update () {
		if (Debug == true) {
			CurrentValue = GetCurrentValue ();
			CurrentIndex = GetCurrentIndex (TTime.time);
		}
	}

	public bool RequestData () {
		if (Server == null)
			return false;

		Server.Request(this,Name,StartTime,Absolute,BufferSize);

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
