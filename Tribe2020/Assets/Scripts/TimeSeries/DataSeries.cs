using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

//
public class DataSeries : DataModifier {
	public GameTime TTime = null;


	[Header("Interpolation parameters")]
	public int meterindex = 1;
	public int rateindex = 0;
	public double RateMeterConversionFactor = 1 / 3600;

	//
	virtual public List<DataPoint> GetPeriod(double From, double To) {
		return null;
	}

	//
	public virtual List<DataPoint> GetData() {
		return null;
	}

	//
	public virtual void InsertData(DataPoint datapoint) {
		
	}

	public virtual void InsertData(List<DataPoint> datapoint) {
		
	}

	public double InterpolateDailyConsumption(int day) 
	{
		//Calculate first and last time on the day.
		double Starts,Ends,StartValue,EndValue;

		GameTime TTime;
		TTime = GameTime.GetInstance();

		Starts = TTime.GetFirstTimeOfDay(day);
		Ends = TTime.GetFirstTimeOfDay(day+1);

		StartValue = InterpolateValueAt (Starts);
		EndValue = InterpolateValueAt (Ends);

		return EndValue - StartValue;
	}

	public double InterpolateValueAt(double time)
	{
		DataPoint data = GetDataAt (time);

		if (data.Timestamp == time)
			return data.Values [meterindex];

		double DeltaTime = time - data.Timestamp;

		return data.Values [meterindex] + DeltaTime * data.Values [rateindex] * RateMeterConversionFactor;

	}


	//
	virtual public DataPoint GetDataAt(double ts) {
		return null;
	}

	//
	public double[] GetCurrentValues() {
		double now = TTime.time;

		return GetDataAt(now).Values;
	}

	//
	public double[] GetValuesAt(double ts) {
		return GetDataAt(ts).Values;
	}

	//
	public List<DataPoint> ApplyModifiers(List<DataPoint> points) {
		List<DataPoint> modified_data;

		modified_data = new List<DataPoint>();

		//if (TimeOffset == 0)
		//	return rawdata;

		foreach(DataPoint point in points) {
			modified_data.Add(ApplyModifiers(point));
		}

		return modified_data;
	}
}
