using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ElectricMeter : MonoBehaviour {

	[Tooltip("Makes the meterpoint powered regardless of state of power source.")]
	public bool AlwaysPowered=false;
	[Tooltip("The object from where the meterpoint gets its power.")]
	public ElectricMeter PowerSource;
	[Tooltip("Indicates if the device is connected to power or not. Can also be set manually so that a meter point becomes a power source if the Power From parameter is empty.")]
	[SerializeField]
	private bool HasPower = false;
	[Tooltip("Makes the meterpoint conduct power to the conneced devices or not")]
	public bool GivesPower = true;


	public List<ElectricMeter> Powering = new List<ElectricMeter>();

	public float Power = 0;
	public double Energy = 0;
	protected float lastupdate;

	public bool continous_updates = false;


	// Use this for initialization
	public void Start () {
		lastupdate = Time.time;

		//Check if source has electricity?
		if (AlwaysPowered)
			powered (true);
		else if (PowerSource != null)
			powered (PowerSource.HasPower && PowerSource.GivesPower);
		else
			powered (false);

		//Connect to the specified meter. 
		Connect (PowerSource);
	}

	//Connects the meter to another meter. 
	public void Connect(ElectricMeter meter) {

		//print ("Connecting to " + meter);

		if (meter != PowerSource) {
			Disconnect ();
			PowerSource = meter;
		}

		if (PowerSource == null)
			return;
		
		if (!PowerSource.Powering.Contains(this))
			PowerSource.Powering.Add (this);

		//Check if source has electricity?
		if (AlwaysPowered)
			powered (true);
		else
			powered (PowerSource.HasPower && PowerSource.GivesPower);

	}

	//Disconnects the meter from another meter. And simulates a power outage. 
	void Disconnect() {
		//No more power drain
		update_power (0);
		//Remove from list. 
		PowerSource.Powering.Remove (this);

		if (!AlwaysPowered)
			powered (false);

	}
	
	// Update is called once per frame
	void Update () {
		if (continous_updates) {
			update_energy ();
		}
	}


	public float update_energy() {
		//Calculate energy for the period
		float now;
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

		if (PowerSource) {
			ElectricMeter em = PowerSource.GetComponent<ElectricMeter>();
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

	//Set the meter node to powered or unpowered state. Returns true of a new stated was initiated by the call. 
	public bool powered(bool powered) {
		if (HasPower == powered)
			return false;

		HasPower = powered;

		if (!GivesPower)
			return false;
			
		foreach (ElectricMeter child in Powering) {
			child.powered (HasPower && GivesPower);
		}

		return true;

	}

	public bool powering(bool powering) {
		if (GivesPower == powering)
			return false;

		GivesPower = powering;

		if (!HasPower)
			return false;

		foreach (ElectricMeter child in Powering) {
			child.powered (HasPower && GivesPower);
		}

		return true;
		
	}

	public void On () {
		powering (true);
	}

	public void Off () {
		powering (false);
	}

}
