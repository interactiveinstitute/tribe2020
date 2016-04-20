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

	public void TurnOn(){
		gameObject.GetComponent<ElectricDevice> ().On ();


		//gameObject.GetComponent<Renderer>().sharedMaterials[2] = materials[0];
		//print(gameObject.GetComponent<Renderer>().materials[1]);

		int i = (int) UnityEngine.Random.Range (0.0F, screen_shots.Length);

		//print(on_materials.Length);
		//print (screen_shots.Length);
		//print (i);

		on_materials[1] = screen_shots[i];

		gameObject.GetComponent<Renderer>().sharedMaterials= on_materials ;



		//Renderer rend = GetComponent<Renderer>();
		//rend.material.shader = Shader.Find("Specular");
		//rend.material.SetColor("_SpecColor", Color.red);

	}

	public void TurnOff(){
		gameObject.GetComponent<ElectricDevice> ().Off ();

		gameObject.GetComponent<Renderer> ().sharedMaterials=off_materials;
	}

	public void SetPowerSave(bool setting){

		PowerSaving = setting;
	}
}
