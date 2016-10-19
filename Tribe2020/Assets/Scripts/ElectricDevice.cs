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

	[Header("Device state")]
	[Space(10)]
	[Tooltip("This is a list of the diffent power states that the device can be in.")]
	//public float[] runlevels;
	//public List<Runlevel> runlevels = new List<Runlevel>();
	public Runlevel[] runlevels;

	[Tooltip("The current runlevel")]
	[ShowOnly] public int runlevel = 0;

	[Space(10)]
	[Tooltip("Does the device continue to use the same power abount of power after a power failure or not.")]
	public bool RetainsRunlevel = false;
	[Tooltip("This is the runlevel that the device assumes after power failure if retain is set to false.")]
	public int DefaultRunlevel=0;
	private int RetainedRunlevel;
	[Tooltip("For backward compability with ElectricMeter.")]
	public int runlevelOn;
	[Tooltip("For backward compability with ElectricMeter.")]
	public int runlevelOff;

	private Material[] default_materials;


	//var time_event = new Tuple<long, float>(0,0.0f);

	//public List<Tuple<long, float>> Pattern;

	// Use this for initialization
	public override void Start () {

		//This is used for replacing a material
		default_materials = gameObject.GetComponent<Renderer> ().sharedMaterials;

		base.Start ();
		lastupdate = _timeMgr.time;

		//Set initial runlevel.
		SetRunlevel(runlevel);
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


	public void ApplyEffects() {

		//New copy 
		Material[] runlevel_materials = (Material[]) default_materials.Clone();

		//Replace only the materials that are not null.
		for (int f=0; f < runlevels [runlevel].materials.Length; f++) {
			if (runlevels [runlevel].materials [f] != null)
				runlevel_materials [f] = runlevels [runlevel].materials [f];
		}

		gameObject.GetComponent<Renderer>().sharedMaterials= runlevel_materials ;
			
	}

//	public void VisualizeOn() {
//		int i = (int) UnityEngine.Random.Range (0.0F, screen_shots.Length);
//		on_materials[MaterialIndex] = screen_shots[i];
//		gameObject.GetComponent<Renderer>().sharedMaterials= on_materials ;
//	}
//
//	public void VisualizeOff() {
//		gameObject.GetComponent<Renderer> ().sharedMaterials=off_materials;
//	}


	public virtual void SetRunlevel(int level) {



		if (!HasPower) {

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
			update_power (runlevels [level].Power);
		else
			update_power (0);	

		ApplyEffects ();
	}


	public virtual void SetRunlevel(string level) {

		int newrunlevel = 0;
		
		SetRunlevel(newrunlevel);
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
