using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class UIElectricity : MonoBehaviour {

	Text power_value;
	Text energy_value;

	// Use this for initialization
	void Start () {

		foreach (Transform t in transform) {
			if (t.name == "Power_value") {
				power_value= t.GetComponent<Text> ();
			}
			else if (t.name == "Energy_counter") {
				energy_value= t.GetComponent<Text> ();
			}

		}


	}
	
	// Update is called once per frame
	void Update () {

		power_value.text = " " + MainMeter.GetInstance().Power + " W";

		energy_value.text = " " + (int)(MainMeter.GetInstance().Energy/1000) + " kWh";
	}
}
