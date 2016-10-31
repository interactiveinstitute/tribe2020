using UnityEngine;
using System.Collections;

public class DataModifier : TimeDataObject {

	[Header("Modifiers")]

	public double TimeOffset=0;
	public double[] Rescales;
	public double[] Offsets;

	public TimeSeries ScaleWithTimeSeries;
	public TimeSeries OffsetWithTimeSeries;

	override public void UpdateAllTargets(DataPoint Data) {

		base.UpdateAllTargets (ApplyModifiers (Data));
	}

	public DataPoint ApplyModifiers(DataPoint point) {
		DataPoint NewPoint;
		double[] ts_offsets=null,ts_scales=null;

		NewPoint = point.Clone ();

		//Apply timeoffset. 
		NewPoint.Timestamp += TimeOffset;

		if (ScaleWithTimeSeries != null) {
			ts_scales = ScaleWithTimeSeries.GetCurrentValues ();
		}

		if (OffsetWithTimeSeries != null) {
			ts_offsets = OffsetWithTimeSeries.GetCurrentValues ();
		}


		for (int i=0;i < NewPoint.Values.Length;i++ ) {

			//Apply local offset
			if ( Offsets != null && (Offsets.Length >= (i+1) )) {

//				Debug.Log (i);
//				Debug.Log (NewPoint.Values.Length);
//				Debug.Log (Offsets.Length);

				NewPoint.Values [i] += Offsets [i];
			}

			//Apply local rescale
			if ( Rescales != null && (Rescales.Length >= (i+1) ))
				NewPoint.Values [i] *= Rescales [i];

			//Apply Timeseries rescale.
			if (ts_scales != null && (ts_scales.Length >= (i+1)))
				NewPoint.Values [i] *= ts_scales [i];


			//Apply Timeseries offset.
			if (ts_offsets != null && (ts_offsets.Length >= (i+1)))
				NewPoint.Values [i] += ts_offsets [i];


		}

		return NewPoint; 

	}
}
