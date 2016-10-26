using UnityEngine;
using System.Collections;

public class ServerObject : MonoBehaviour {

	[Header("General")]
	public string Name;

	[Header("Local clients")]
//	public Subscription[] Subscriptions;
	public string[] Payloads;


	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}


	public string Get(string Name,double StartTime,bool Absolute,int BufferSize)
	{
		//Add to query list. 

		//Test
		//ts.Values = new double[4] {1.0,2.0,3.0,4.0};
		//ts.TimeStamps = new double[4] {1452691843.0,1452691849.0,1452691858.0,1452691890.0};
		//ts.BufferValid = true;
		//ts.CurrentSize = BufferSize;
		return "TODO";
	}

	public int Subscribe(TimeSeries ts,string Name,double StartTime,bool Absolute,int BufferSize) {
		return -1;
	}

	//MQTT subscribe to the last incomming message. 
	public int Subscribe(TimeSeries ts,string Name) {
		return Subscribe( ts, Name, 0, false, 1);
	}

	public void Unsubscribe(int subscriptionID) {
		
	}

}
