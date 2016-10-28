using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

[System.Serializable]  
public class DataPoint  {
	public double Timestamp;
	public double[] Values;
	public string[] Texts;

	public DataPoint Clone() {
		DataPoint clone = new DataPoint();

		if (Values != null)
			clone.Values = (double[])Values.Clone ();

		if (Texts != null)
			clone.Texts = (string[])Texts.Clone ();
		
		clone.Timestamp = Timestamp;
		return clone;
	}
}
	




//public class CompareDataPoint : IComparer<DataPoint>
//{
//	static IComparer<DataPoint> comparer = new CompareDataPoint();
//
//	public int Compare(DataPoint x, DataPoint y)
//	{
//		if (x == y)    return 0;
//		if (x == null) return -1;
//		if (y == null) return 1;
//		if (x.Timestamp > y.Timestamp)
//			return -1;
//		if (x.Timestamp < y.Timestamp)
//			return 1;
//
//		return 0;
//	}
//
//}