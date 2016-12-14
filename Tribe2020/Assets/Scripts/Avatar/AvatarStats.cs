using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class AvatarStats : MonoBehaviour {
	public float conviction;
	public float energy;

    public float maxEfficiencyValue;
	public float lightingEfficiency;
	public float warmingEfficiency;
	public float coolingEfficiency;
	public float deviceEfficiency;

    public enum Efficiencies { Lighting, Warming, Cooling, Device}

    //public List<AvatarAttitude> attitudes;
    public AvatarAttitude attitude;

	// Use this for initialization
	void Start () {
        attitude.Init();
    }
	
	// Update is called once per frame
	void Update () {
	
	}

    private bool RunEfficiencyTest(float value)
    {
        return value >= Random.value * maxEfficiencyValue;
    }

    private bool RunEfficiencyTest(Efficiencies efficiencies)
    {
        float value = 0.0f;
        switch (efficiencies)
        {
            case Efficiencies.Cooling:
                value = coolingEfficiency;
                break;
            case Efficiencies.Device:
                value = deviceEfficiency;
                break;
            case Efficiencies.Lighting:
                value = lightingEfficiency;
                break;
            case Efficiencies.Warming:
                value = warmingEfficiency;
                break;
        }
        return RunEfficiencyTest(value);
    }

    //First argument is always parameter to test, following parameters are success callback with up to 4 arguments
    //Always return success flag
    public bool TestEnergyEfficiency(Efficiencies efficiencies)
    {
        return RunEfficiencyTest(efficiencies);
    }

    public bool TestEnergyEfficiency(Efficiencies efficiencies, System.Action successCallback)
    {
        if (!RunEfficiencyTest(efficiencies))
        {
            return false;
        }

        successCallback();
        return true;
    }

    public bool TestEnergyEfficiency<U1>(Efficiencies efficiencies, System.Action<U1> successCallback, U1 p1)
    {
        if (!RunEfficiencyTest(efficiencies))
        {
            return false;
        }

        successCallback(p1);
        return true;
    }

    public bool TestEnergyEfficiency<U1,U2>(Efficiencies efficiencies, System.Action<U1, U2> successCallback, U1 p1, U2 p2)
    {
        if (!RunEfficiencyTest(efficiencies))
        {
            return false;
        }

        successCallback(p1, p2);
        return true;
    }

    public bool TestEnergyEfficiency<U1,U2,U3>(Efficiencies efficiencies, System.Action<U1,U2,U3> successCallback, U1 p1, U2 p2, U3 p3)
    {
        if (!RunEfficiencyTest(efficiencies))
        {
            return false;
        }

        successCallback(p1, p2, p3);
        return true;
    }

    public bool TestEnergyEfficiency<U1, U2, U3, U4>(Efficiencies efficiencies, System.Action<U1, U2, U3, U4> successCallback, U1 p1, U2 p2, U3 p3, U4 p4)
    {
        if (!RunEfficiencyTest(efficiencies))
        {
            return false;
        }

        successCallback(p1, p2, p3, p4);
        return true;
    }

}
