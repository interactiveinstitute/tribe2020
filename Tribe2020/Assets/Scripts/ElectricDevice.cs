using UnityEngine;
using System.Collections;
//using System.Tuple;
using System.Collections.Generic;


public class power{
	private float value;
}



public class ElectricDevice : ElectricMeter {

	public float[] runlevels;
	public int runlevel = 0;
	public int runlevelOn = 0;
	public int runlevelOff = 0;


	//var time_event = new Tuple<long, float>(0,0.0f);

	//public List<Tuple<long, float>> Pattern;

	// Use this for initialization
	void Start () {
		lastupdate = Time.time;
	}
	
	// Update is called once per frame
	void Update () {
		if (continous_updates) {
			update_energy ();
		}
	}

	public void On () {
		IsOn = true;

		SetRunlevel (runlevelOn);
	}

	public void Off () {
		IsOn = false;
		SetRunlevel (runlevelOff);
	}

	public void SetRunlevel(int level) {
		if (level > (runlevels.Length -1 ))
		{
			return;
		}

		//New runlevel
		runlevel = level;

		update_power(runlevels[level]);
	}
		






	public void register() {
		
	}

}
