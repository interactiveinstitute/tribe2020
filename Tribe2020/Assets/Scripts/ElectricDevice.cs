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
	private int RetainedRunlevel;


	//var time_event = new Tuple<long, float>(0,0.0f);

	//public List<Tuple<long, float>> Pattern;

	// Use this for initialization
	public override void Start () {
		base.Start ();
		lastupdate = Time.time;
	}
	
	// Update is called once per frame
	void Update () {
		if (continous_updates) {
			update_energy ();
		}
	}

	public override void On () {

		SetRunlevel (runlevelOn);
	}

	public override void Off () {
		
		SetRunlevel (runlevelOff);
	}

	public virtual void SetRunlevel(int level) {

		print ("!!");
		
		if (!HasPower) {
			print ("!!!!");
			if (runlevel < 0)
				return;
			else
				level = -1;
		}

		if (level > (runlevels.Length -1 ))
		{
			return;
		}

		//New runlevel
		runlevel = level;

		//Runlevels below 0 means power failure. 
		if (runlevel > 0)
			update_power (runlevels [level]);
		else
			update_power (0);	
		
	}
		
	public override bool powered(bool powered) {

		//If no change return. 
		if (!base.powered (powered))
			return false;

		if (powered) {
			//Reapply the current runlevel or a defaul one. 
			if (RetainsRunlevel)
				SetRunlevel (RetainedRunlevel);
			else
				SetRunlevel (DefaultRunlevel);
		} else {
			RetainedRunlevel = runlevel;
			SetRunlevel (-1);
		}

		return true;

	}
		
	

}
