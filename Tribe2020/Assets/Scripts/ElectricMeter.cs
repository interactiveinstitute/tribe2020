using UnityEngine;
using System.Collections;

public class ElectricMeter : MonoBehaviour {

	public bool IsOn = false;

	public ElectricMeter PowerFrom;
	public ElectricMeter[] Powering;


	public float Power = 0;
	public double Energy = 0;
	protected float lastupdate;

	public bool continous_updates = false;


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


	public float update_energy() {
		//Calculate energy for the period
		float delta,now;
		now = Time.time;

		update_energy (now);

		return now;

	}

	public void update_energy(float now) {
		//Calculate energy for the period
		float delta;

		delta = now - lastupdate;
		Energy = Energy + ((Power * delta)/3600);

		lastupdate = now;

	}

	public void update_power(float new_power)
	{
		float now;
		now = Time.time;

		update_power (now, new_power);
	}

	public void update_power(float ts,float new_power)
	{
		float change;

		update_energy (ts);
	
		change = new_power - Power; 
		Power = new_power;

		if (PowerFrom) {
			ElectricMeter em = PowerFrom.GetComponent<ElectricMeter>();
			em.add_to_power (ts, change);
		}
	}

	public void add_to_power(float ts,float change){
		update_energy (ts);
		Power = Power + change;
	}

	public void reset_energy() {
		Energy = 0;
		lastupdate = Time.time;
	}


}
