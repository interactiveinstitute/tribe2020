using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class DataSeriesModifier : DataSeries {



	public enum Manipulation{
		sum,
		diff,
		min,
		max,
		mult,
		div
	};

	[Header("DataSeriesModifier properties")]


	public Manipulation operation;

	public List<DataSeries> SourceSeries;

	public BasicDataSeries inspect;
	public List<DataPoint> result;


	public void Test() {
		double now = GameTime.GetInstance ().time;

		result = GetPeriod (now - 3*3600, now);

	}



	override public List<DataPoint> GetPeriod(double From, double To) {

		BasicDataSeriesCollection result = new BasicDataSeriesCollection ();
		BasicDataSeries Series;

		if (SourceSeries.Count == 1)
			return ApplyModifiers (SourceSeries [0].GetPeriod (From, To));

		foreach (DataSeries serie in SourceSeries) {
			Series = new BasicDataSeries ();
			Series.Data = serie.GetPeriod (From, To);
			result.Collection.Add ( Series );
		}

		if (operation == Manipulation.sum) {
			return ApplyModifiers(result.GetStaircaseSumOfSeries().Data);
		}

		return null;

	}






}
