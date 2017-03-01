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

    public void BuildPanel(GameObject goDevice) {
        ElectricDevice ed = goDevice.GetComponent<ElectricDevice>();
        if (ed) {
            SetPowerValue(ed.Power);
        }
        else {
            SetPowerValueNotApplicable();
        }
        SetEnergyEffeciency(goDevice.GetComponent<Appliance>().energyEffeciency);
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
