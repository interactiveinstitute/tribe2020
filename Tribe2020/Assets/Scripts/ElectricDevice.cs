using UnityEngine;
using System.Collections;
//using System.Tuple;
using System.Collections.Generic;


public class power {
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
	[ShowOnly]
	public int runlevel = 0;

	[Space(10)]
	[Tooltip("Does the device continue to use the same power abount of power after a power failure or not.")]
	public bool RetainsRunlevel = false;
	[Tooltip("This is the runlevel that the device assumes after power failure if retain is set to false.")]
	public int DefaultRunlevel = 0;
	private int RetainedRunlevel;
	[Space(10)]
	[Tooltip("For backward compability with ElectricMeter.")]
	public int runlevelOn;
	[Tooltip("For backward compability with ElectricMeter.")]
	public int runlevelOff;
	[Tooltip("For backward compability with ElectricMeter.")]
	public int runlevelUnpowered = 0;

	//Temporary solution for including effect from eems
	[SerializeField]
	private float _energyMod = 1;

	//Removed: a single effency is used instead
	//[Tooltip("What kind of avatar energyefficiency is this device relates to")]
	//public AvatarStats.Efficiencies relatedEfficiency;

	//[Header("Sound")]
	private AudioSource noisesource;
	private Material[] default_materials;


    PilotView _pilotView;


    //var time_event = new Tuple<long, float>(0,0.0f);

    //public List<Tuple<long, float>> Pattern;

    // Use this for initialization
    void Start() {

        _pilotView = PilotView.GetInstance();

        //This is used for replacing a material
        //We initially save the gameobjects materials to the side in order to be able to switch back to them.
        foreach (Runlevel rl in runlevels) {
			//Materials 

			//Maybe we have a specifically targeted renderer (of a gameObject) that we want to affect
			Renderer rend = rl.Target;

			//If target not set, use the gameobjects renderer
			if(rend == null)
				rend = gameObject.GetComponent<Renderer>();


			if(rend == null) {
				//Apparently the object doesn't have a renderer
				continue;
			}
			//Shouldn't be null at this point!
			Debug.Assert(rend != null);

			rl.Default_materials = (Material[])rend.sharedMaterials;
		}

		base.Start();
		//lastupdate = _timeMgr.time;

		//Set initial runlevel.
		SetRunlevel(runlevel);
	}

	// Update is called once per frame
	void Update() {
		if(continous_updates) {
			update_energy();
		}

	}

	public override void On() {

		SetRunlevel(runlevelOn);
	}

	public override void Off() {

		SetRunlevel(runlevelOff);
	}


	public void ApplyEffects() {
		//Debug.Log("Applying effects");

		int rl = runlevel;

		if(rl < 0)
			rl = runlevelUnpowered;

		if(runlevels.Length == 0)
			return;


		//Materials 
		Renderer rend = runlevels[rl].Target;

		//If target not set, apply the effect on the gameobject instead.
		if(rend == null)
			rend = gameObject.GetComponent<Renderer>();


		//If the gameObject doesn't have a renderer, skip material change.
		if(rend != null) {
			//First fill up with the default materials.
			Material[] runlevel_materials = (Material[])runlevels[rl].Default_materials.Clone();
			//Material[] runlevel_materials = (Material[]) default_materials.Clone();


			//how many materials in the material list of this runlevel.
			int len = runlevels[rl].materials.Length;

			//Replace only the materials that are not null.
			for(int f = 0; f < len; f++) {

				if(runlevels[rl].materials[f] != null)
					runlevel_materials[f] = runlevels[rl].materials[f];
			}

			//what the fuck? We were updating sharedMaterials, which affects all objects using this material. I don't think that is what we want to do....
			//Debug.Log("Changing sharedMaterial of object " + this.name + ". Runlevel " + rl);
			//runlevels [rl].Target.sharedMaterials = runlevel_materials;
			rend.materials = runlevel_materials;

        }


		//Lights
		foreach(Light l in runlevels[rl].LightsOn) {
			l.enabled = true;
		}

		foreach(Light l in runlevels[rl].LightsOff) {
			l.enabled = false;
		}

        //Eeeeeeh. This updateLighting function is legacy shit. Let's comment it out! Gunnar
        //if(GetComponentInParent<Room>()) {
        //	GetComponentInParent<Room>().UpdateLighting();
        //}

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
		//Debug.Log("Setting runlevel for " + this.name + ". input parameter is " + level);

		if(!HasPower) {
			//Debug.Log("this device hasn't got power. Can't turn it on." + this.name);
			if(runlevel < 0)
				return;
			else
				level = -1;
		}

		if(level > (runlevels.Length - 1)) {
			return;
		}

		//New runlevel
		runlevel = level;

		//Runlevels below 0 means power failure. 
		if(runlevel > 0)
			update_power(runlevels[level].Power * _energyMod);
		else
			update_power(0);

		ApplyEffects();

        //Update device panel
        Appliance app = GetComponent<Appliance>();
        if (app) {
            _pilotView.BuildDevicePanel(app);
        }

    }

	//
	public void SetEnergyMod(float mod) {
		_energyMod = mod;
	}

	//
	public float GetEnergyMod() {
		return _energyMod;
	}


	//public virtual void SetRunlevel(string level) {

	//	int newrunlevel = 0;

	//	SetRunlevel(newrunlevel);
	//}

	public override bool powered(bool powered) {

		////If no change return. 
		if(!base.powered(powered))
			return false;

		//ElectricDevice me = this;

		////No change?
		//if (HasPower == powered)
		//    return false;

		if(powered) {
			//Reapply the current runlevel or a defaul one. 
			if(RetainsRunlevel)
				SetRunlevel(RetainedRunlevel);
			else
				SetRunlevel(DefaultRunlevel);
		} else {
			RetainedRunlevel = runlevel;
			SetRunlevel(runlevelOff);
			//SetRunlevel (-1);
		}

		return true;

	}



}
