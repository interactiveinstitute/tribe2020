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



	public DataPoint Add(DataPoint augend) {
		DataPoint clone;
		int first,second,largest;
		double firstval,secondval;

		if (augend.Values == null)
			return this;

		if (Values == null)
			return augend;
		
		first = Values.Length;
		second = augend.Values.Length;

		largest = first > second ? first : second;

		clone = new DataPoint();
		clone.Values = new double[largest];

		for (int i = 0; i < largest; i++) {

			if (first <= i)
				firstval = 0.0;
			else
				firstval = Values [i];

			if (firstval == null)
				firstval = 0.0;
			
			if  (second <= i)
				secondval = 0.0;
			else
				secondval = augend.Values [i];

			if (secondval == null)
				secondval = 0.0;

			clone.Values [i] = firstval + secondval;
		}

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