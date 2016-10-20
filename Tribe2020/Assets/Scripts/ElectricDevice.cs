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

	[Header("Device states")]
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
	[Space(10)]
	[Tooltip("For backward compability with ElectricMeter.")]
	public int runlevelOn;
	[Tooltip("For backward compability with ElectricMeter.")]
	public int runlevelOff;
	[Tooltip("For backward compability with ElectricMeter.")]
	public int runlevelUnpowered = 0;

	//[Header("Sound")]
	private AudioSource noisesource; 

	


	//var time_event = new Tuple<long, float>(0,0.0f);

	//public List<Tuple<long, float>> Pattern;

	// Use this for initialization
	public override void Start () {

		//This is used for replacing a material
		foreach (Runlevel rl in runlevels) {
			//Materials 
			Renderer rend = rl.Target;

			if (rend == null)
				rend = gameObject.GetComponent<Renderer> ();

			if (rend != null)
				rl.default_materials = (Material[]) rend.sharedMaterials;
		}


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

		int rl = runlevel;

		if (rl < 0)
			rl = runlevelUnpowered;

		if (runlevels.Length == 0)
			return;

		//Materials
		if (runlevels [rl].Target != null) {

		Material[] runlevel_materials = (Material[]) runlevels [rl].default_materials.Clone();



			int len = runlevels [rl].materials.Length;

			//Replace only the materials that are not null.
			for (int f = 0; f < len; f++) {

				if (runlevels [rl].materials [f] != null)
					runlevel_materials [f] = runlevels [rl].materials [f];
			}

			runlevels [rl].Target.sharedMaterials = runlevel_materials;
		}


		//Lights
		foreach (Light l in runlevels [rl].LightsOn) {
			l.enabled = true;
		}

		foreach (Light l in runlevels [rl].LightsOff) {
			l.enabled = false;
		}

		if(GetComponentInParent<Room>()) {
			GetComponentInParent<Room>().UpdateLighting();
		}

		//Sound
		//TODO

			  
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
