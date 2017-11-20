using UnityEngine;
using System.Collections;
using System;

public class MainMeter : ElectricMeter {

	private static MainMeter _instance;


	public void Awake() {

		base.Awake ();

		_instance = this;
		//continous_updates = true;

	}

	override public void Start () {
		base.Start();
	}

	public static MainMeter GetInstance() {
		return _instance;
	}


}
