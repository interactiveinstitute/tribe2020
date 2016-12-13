using UnityEngine;
using System.Collections;

public class MainMeter : ElectricMeter {

	private static MainMeter _instance;

	void Awake() {
		_instance = this;
		continous_updates = true;
		base.Awake ();
	}

	public static MainMeter GetInstance() {
		return _instance;
	}
}
