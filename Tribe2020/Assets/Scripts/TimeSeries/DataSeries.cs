using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

//
public class DataSeries : DataModifier {
	public GameTime TTime = null;

	//
	virtual public List<DataPoint> GetPeriod(double From, double To) {
		return null;
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
