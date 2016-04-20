using UnityEngine;
using System.Collections;
using System;

public class MonitorCRT : ElectricDevice {

	public bool PowerSaving = false;
	public Material[] screen_shots;
	private Material[] off_materials;
	private Material[] on_materials;

	// Use this for initialization
	public void Start () {
		//rend = GetComponent<Renderer>();
		off_materials = gameObject.GetComponent<Renderer> ().sharedMaterials;

		on_materials = (Material[]) off_materials.Clone();
		print (on_materials.Length);

		base.Start ();

	}
	
	// Update is called once per frame
//	void Update () {
		
	
//	}

	public void TurnOnScreen() {
		int i = (int) UnityEngine.Random.Range (0.0F, screen_shots.Length);
		on_materials[1] = screen_shots[i];
		gameObject.GetComponent<Renderer>().sharedMaterials= on_materials ;
	}

	public void TurnOffScreen() {
		gameObject.GetComponent<Renderer> ().sharedMaterials=off_materials;
	}

	public override void SetRunlevel(int level) {
		base.SetRunlevel(level);

		if (runlevel >= runlevelOn)
			TurnOnScreen();
		else
			TurnOffScreen();
	}

	public void SetPowerSave(bool setting){

		PowerSaving = setting;
	}


}
