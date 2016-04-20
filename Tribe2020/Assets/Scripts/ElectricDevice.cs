using UnityEngine;
using System.Collections;
//using System.Tuple;
using System.Collections.Generic;


public class power{
	private float value;
}



public class ElectricDevice : ElectricMeter {

	[Tooltip("Indicates if the device currently us inplugged or not.")]
	[SerializeField]
	protected bool Unplugged = false;
	[Tooltip("This is a list of the diffent power states that the device can be in.")]
	public float[] runlevels;
	[Tooltip("The current runlevel")]
	public int runlevel = 0;
	public int runlevelOn = 0;
	public int runlevelOff = 0;
	[Tooltip("Does the device continue to use the same power abount of power after a power failure or not.")]
	public bool RetainsRunlevel = false;
	[Tooltip("This is the runlevel that the device assumes after power failure if retain is set to false.")]
	public int DefaultRunlevel=0;


	//var time_event = new Tuple<long, float>(0,0.0f);

	//public List<Tuple<long, float>> Pattern;

	// Use this for initialization
	public void Start () {

		print ("start!!!!");
		lastupdate = Time.time;
		base.Start ();
	}
	
	// Update is called once per frame
	void Update () {
		if (continous_updates) {
			update_energy ();
		}
	}

	public void On () {

		SetRunlevel (runlevelOn);
	}

	public void Off () {
		
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
		
	public bool powered(bool powered) {

		//If no change return. 
		if (!base.powered (powered))
			return false;

		if (powered) {
			//Reapply the current runlevel or a defaul one. 
			if (RetainsRunlevel)
				SetRunlevel (runlevel);
			else
				SetRunlevel (DefaultRunlevel);
		} else {
			update_power (0);
		}

		return true;

	}
		
	

}
