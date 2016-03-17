using UnityEngine;
using System.Collections;


public class MonitorCRT : ElectricDevice {

	public bool PowerSaving = false;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void TurnOn(){
		gameObject.GetComponent<ElectricDevice> ().On ();

		gameObject.GetComponent<Renderer>().materials[1].SetColor("_MainColor", Color.red);

		//Renderer rend = GetComponent<Renderer>();
		//rend.material.shader = Shader.Find("Specular");
		//rend.material.SetColor("_SpecColor", Color.red);

	}

	public void TurnOff(){
		gameObject.GetComponent<ElectricDevice> ().Off ();
		gameObject.GetComponent<Renderer>().materials[1].SetColor("_MainColor", Color.gray);
	}

	public void SetPowerSave(bool setting){

		PowerSaving = setting;
	}
}
