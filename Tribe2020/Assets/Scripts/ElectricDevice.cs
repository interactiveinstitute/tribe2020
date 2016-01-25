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
	public int runlevelOn = 0;
	public int runlevelOff = 0;

	//var time_event = new Tuple<long, float>(0,0.0f);

	//public List<Tuple<long, float>> Pattern;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}