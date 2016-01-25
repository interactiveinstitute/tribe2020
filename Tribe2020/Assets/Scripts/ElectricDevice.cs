using UnityEngine;
using System.Collections;
//using System.Tuple;
using System.Collections.Generic;

public class power{
	private float value;
}



public class ElectricDevice : MonoBehaviour {



	public bool On = false;

	public GameObject ConnectedTo;

	public float[] runlevels;
	public int runlevel = 0;
	public int runlevelOn = 0;
	public int runlevelOff = 0;

	public float Power = 0;
	public double Energy = 0;
	private double lastrunlevelchange;

	//var time_event = new Tuple<long, float>(0,0.0f);

	//public List<Tuple<long, float>> Pattern;

	// Use this for initialization
	void Start () {
		lastrunlevelchange = Time.time;
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void TurnOn () {
		On = true;

		SetRunlevel (runlevelOn);
	}

	public void TurnOff () {
		On = false;
		SetRunlevel (runlevelOff);
	}

	public void SetRunlevel(int level) {
		if (level > (runlevels.Length -1 ))
		{
			return;
		}

		runlevel = level;


	/*	if (ConnectedTo != null)
		{
			ConnectedTo.UpdatePower();

			if (ConnectedTo.On == true)
			{
				Power = runlevels[level];
			}
			else 
			{
				Power = 0.0f;
			}
		}
		else 
		{   */
		double delta,now;
		now = Time.time;
		delta = now - lastrunlevelchange;
			Power = runlevels[level];	
		Energy = Energy + (Power * delta);
		lastrunlevelchange = now;
	//	}
			
	}

}
