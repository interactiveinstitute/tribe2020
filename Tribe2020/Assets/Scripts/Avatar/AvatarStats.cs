using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AvatarStats : MonoBehaviour {
	public string avatarName;
	public float conviction;
	public float energy;

    public float maxEfficiencyValue;
	public float lightingEfficiency;
	public float warmingEfficiency;
	public float coolingEfficiency;
	public float deviceEfficiency;

	public List<AvatarAttitude> attitudes;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    private bool RunEfficiencyTest(float value)
    {
        return value >= Random.value * maxEfficiencyValue;
    }

    public bool TestLightningEfficiency()
    {
        return RunEfficiencyTest(lightingEfficiency);
    }

    public bool TestWarmingEfficiency()
    {
        return RunEfficiencyTest(warmingEfficiency);
    }

    public bool TestCoolingEfficiency()
    {
        return RunEfficiencyTest(coolingEfficiency);
    }

    public bool TestDeviceEfficiency()
    {
        return RunEfficiencyTest(warmingEfficiency);
    }

}
