using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class DataManipulator : DataModifier
{


    public List<Manipulation> Manipulations;
    [Header("Debug")]
    public int selected = 0;

    override public void UpdateAllTargets(DataPoint Data)
    {
        base.UpdateAllTargets(ApplyModifiers(Data));
    }

    public DataPoint ApplyModifiers(DataPoint point)
    {
		point = base.ApplyModifiers (point);

        if (Manipulations.Count == 0)
            return point;

        DataPoint newpoint = point.Clone();

        foreach (Manipulation manipulation in Manipulations)
        {
            manipulation.AddOffsets(point, newpoint);
        }

        return newpoint;
    }

    public void Activate() {

        if (selected < Manipulations.Count) {
            double now = GameTime.GetInstance().time;
            Manipulations[selected].Activate(now);


        }
    }

    public void Deactivate()
    {
        if (selected < Manipulations.Count)
            Manipulations[selected].DeActivate(GameTime.GetInstance().time);
    }

	public void Add(Manipulation manipulation)
	{
		Manipulations.Add (manipulation);

		double now = GameTime.GetInstance ().time;

		//TODO add keypoint action to gametime when the maipulation becomes active. 
		if (!manipulation.isActive (now))
			return;

		//Already active so we update LastValue. 
		if (manipulation.Type == Manipulation.DataType.RateCounter)
			CreateCounterRateInterpolation (now, manipulation.TimeFactor);
		else {

			LastData.Timestamp = now;
			UpdateAllTargets (LastData);
		}


	}


}
