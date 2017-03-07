using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DevicePanel : MonoBehaviour {

    PilotView _pilotView;

    // Use this for initialization
    void Start() {
        _pilotView = PilotView.GetInstance();
    }

    // Update is called once per frame
    void Update() {

    }

    public void BuildPanel(Appliance appliance) {
        ElectricDevice ed = appliance.GetComponent<ElectricDevice>();
        if (ed) {
            SetPowerValue(ed.Power);
            SetEnergyEffeciency(appliance.energyEffeciency);
        }
        else {
            SetPowerValueNotApplicable();
            SetEnergyEffeciency(1.0f);
        }
        
    }

    void SetPowerValue(float value) {
        _pilotView.devicePowerValue.text = value + "W";
    }

    void SetPowerValueNotApplicable() {
        _pilotView.devicePowerValue.text = "-";
    }

    void SetEnergyEffeciency(float value) {
        int nLabels = _pilotView.EELabels.Count;
        int index = Mathf.Min(Mathf.FloorToInt((1.0f - value) * nLabels), nLabels - 1);
        _pilotView.deviceEfficiencyLabel.GetComponent<Image>().sprite = _pilotView.EELabels[index];
    }
}
