﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DevicePanel : MonoBehaviour {

    PilotView _pilotView;
    public ElectricDevice currentDevice = null;

    // Use this for initialization
    void Start() {
        _pilotView = PilotView.GetInstance();
    }

    // Update is called once per frame
    void Update() {

    }

    public void BuildPanel(Appliance appliance) {

        currentDevice = appliance.GetComponent<ElectricDevice>();

        ElectricDevice ed = appliance.GetComponent<ElectricDevice>();
        if (ed) {
            SetPowerValue(ed.Power);
            //SetEnergyEffeciency(appliance.energyEffeciency);
            SetEnergyEffeciency(ed.energyEffeciency);
        }
        else {
            SetPowerValueNotApplicable();
            //SetEnergyEffeciency(1.0f);
            SetEnergyEffeciency(EnergyEffeciencyLabels.Name.AAAA);
        }
        
    }

    public void OnClose() {
        currentDevice = null;
    }

    void SetPowerValue(float value) {
        _pilotView.devicePowerValue.text = value + "W";
    }

    void SetPowerValueNotApplicable() {
        _pilotView.devicePowerValue.text = "-";
    }

    void SetEnergyEffeciency(EnergyEffeciencyLabels.Name eeName) {
        _pilotView.deviceEfficiencyLabel.GetComponent<Image>().sprite = _pilotView.EELabels[(int)eeName];
    }
}