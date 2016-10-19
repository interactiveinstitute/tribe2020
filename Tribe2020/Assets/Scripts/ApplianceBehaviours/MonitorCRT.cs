using UnityEngine;
using System.Collections;
using System;

public class MonitorCRT : ElectricDevice {

	public bool PowerSaving = false;


	public void SetPowerSave(bool setting) {
		PowerSaving = setting;
	}


}
