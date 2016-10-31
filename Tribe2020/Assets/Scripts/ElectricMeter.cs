using UnityEngine;
using System.Collections.Generic;

public class ElectricMeter : TimeDataObject {
	[Header("Electric upward connection")]
	[Space(10)]
	[Tooltip("The object from where the meterpoint gets its power.")]
	public ElectricMeter PowerSource;
	[Tooltip("If this is checked and the the PowerSouce is empty the Electric meter will seach up the gameobjects hierarchy for a power source.")]
	public bool AutomaticallyFindSource = false;
	[Space(10)]
	[Tooltip("Makes the meterpoint powered regardless of state of power source.")]
	public bool AlwaysPowered=false;




	[Header("Electric downward connection")]

	[Tooltip("Makes the meterpoint conduct power to the conneced devices or not")]
	[Space(10)]
	public bool GivesPower = true;
	protected GameTime _timeMgr;


	[ShowOnly] public List<ElectricMeter> Powering = new List<ElectricMeter>();

	[Header("Readings")]
	[Tooltip("Indicates if the device is connected to power or not.")]
	[ShowOnly] public bool HasPower = false;
	[ShowOnly] public float Power = 0;
	[ShowOnly] public double Energy = 0;
	[ShowOnly] public double lastupdate;

	[Header("Debug tools")]
	public bool continous_updates = false;

	// Use this for initialization
	public virtual void Start () {

		_timeMgr = GameTime.GetInstance();
		lastupdate = _timeMgr.time;

		//Auto
		if (PowerSource == null && AutomaticallyFindSource)
			AutoFindPowerSource ();


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

	public void AutoFindPowerSource () {
		ElectricMeter source;
		GameObject parentObject;
		MainMeter _mm;

		if (transform.parent == null) {
			
			_mm = MainMeter.GetInstance ();

			if (_mm != null)
				PowerSource = _mm;

			return;
		}
		parentObject = this.transform.parent.gameObject;

		while (true) {

			//No more parents and nothing found 
			if (parentObject == null) {
				_mm = MainMeter.GetInstance ();

				if (_mm != null)
					PowerSource = _mm;
				return;
			}

			source = parentObject.GetComponent<ElectricMeter> ();

			//If there is a hit use the first one. 
			if (source != null) {

				if (this != source) {
					PowerSource = source;
					return;
					}
			}

			parentObject = parentObject.transform.parent.gameObject;
		}
	}	

	//Connects the meter to another meter. 
	public void Connect(ElectricMeter meter) {

		//print ("Connecting to " + meter);

		if (meter != PowerSource) {
			Disconnect ();
			PowerSource = meter;
		}

		//Check if source has electricity?
		if (AlwaysPowered)
			powered (true);
			
		if (PowerSource != null) {

			if (!PowerSource.Powering.Contains (this))
				PowerSource.Powering.Add (this);
		
			powered (PowerSource.HasPower && PowerSource.GivesPower);
		}

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


	public double update_energy() {
		//Calculate energy for the period
		double now;
		now = _timeMgr.time;

		update_energy (now);

		return now;

	}

	public void update_energy(double now) {
		//Calculate energy for the period
		double delta;

		delta = now - lastupdate;
		Energy = Energy + ((Power * delta)/3600);
		//Energy += Power * Time.deltaTime;

		lastupdate = now;

	}

	public void update_power(float new_power)
	{
		double now;
		now = _timeMgr.time;

		update_power (now, new_power);
	}

	public void update_power(double ts,float new_power)
	{
		float change;	
		change = new_power - Power; 
		add_to_power (ts, change);

		foreach (Connection Conn in Targets) {
			if (Conn == null)
				continue;
			DataPoint Data = new DataPoint ();
			Data.Timestamp = ts;
			Data.Values = new double[2];
			Data.Values [0] = new_power;
			Data.Values [1] = Energy;
			Conn.Target.TimeDataUpdate (Conn, Data);
		}
	}

	public void add_to_power(double ts,float change){
		update_energy (ts);
		Power = Power + change;

		if (PowerSource)
			PowerSource.add_to_power (ts, change);
	}

	public void reset_energy() {
		Energy = 0;
		lastupdate = _timeMgr.time;
	}

	//Set the meter node to powered or unpowered state. Returns true of a new stated was initiated by the call. 
	public virtual bool powered(bool powered) {
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

	public virtual void On () {
		powering (true);
	}

	public virtual void Off () {
		powering (false);
	}

	public void Toggle () {
		powering (!GivesPower);
	}

	override public void TimeDataUpdate(Connection Con,DataPoint data) {
		//Debug.Log ("Got data!");
		update_power(data.Timestamp,(float)data.Values[0]);
	}

}
