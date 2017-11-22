using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DevicePanel : MonoBehaviour {
	private PilotView _pilotView;

	public Text deviceTitle;
	public Text deviceDescription;
	public Text devicePowerValue;
	public Text deviceEEMTitle;
	public Transform deviceEEMContainer;
	public Image deviceEfficiencyLabel;

	public ElectricDevice currentDevice = null;

    // Use this for initialization
    void Start() {
        _pilotView = PilotView.GetInstance();
    }

    // Update is called once per frame
    void Update() {

    }

	//
    public void BuildPanel(Appliance appliance) {
        currentDevice = appliance.GetComponent<ElectricDevice>();

        ElectricDevice ed = appliance.GetComponent<ElectricDevice>();
        if (ed) {
			SetPowerValue(ed.GetPower());
            SetEnergyEffeciency(ed.energyEffeciency);
        } else {
            SetPowerValueNotApplicable();
            SetEnergyEffeciency(EnergyEffeciencyLabels.Name.AAAA);
        }
        
    }

	//
    public void OnClose() {
        currentDevice = null;
    }

	//
    void SetPowerValue(float value) {
        _pilotView.devicePowerValue.text = value + "W";
    }

	//
    void SetPowerValueNotApplicable() {
        _pilotView.devicePowerValue.text = "-";
    }

	//
    void SetEnergyEffeciency(EnergyEffeciencyLabels.Name eeName) {
        _pilotView.deviceEfficiencyLabel.GetComponent<Image>().sprite = _pilotView.EELabels[(int)eeName];
    }
}
