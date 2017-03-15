using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugView : MonoBehaviour {

    public bool Enabled;

    [Space(10)]

    public DataSeries CO2EmissionSeries, CO2EmissionElectricitySeries, CO2EmissionGasSeries;
    public DataSeries BaselineCO2EmissionSeries, BaselineCO2EmissionElectricitySeries, BaselineCO2EmissionGasSeries;
    public DataSeries CO2ChangeSeries;

    [Space(10)]

    public double CO2Emission;
    public double CO2EmissionElectricity;
    public double CO2EmissionGas;
    public double BaselineCO2Emission;
    public double BaselineCO2EmissionElectricity;
    public double BaselineCO2EmissionGas;
    public double CO2EmissionChange;

   

    // Use this for initialization
    void Start () {
       
		
	}
	
	// Update is called once per frame
	void Update () {
        if (!Enabled)
            return;

        //GameTime
	}
}
