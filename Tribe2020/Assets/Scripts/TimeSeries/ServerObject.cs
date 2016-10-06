using UnityEngine;
using System.Collections;

public class ServerObject : MonoBehaviour {

	public Subscriber[] Subscribers;


	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void Request(TimeSeries ts,string Name,double StartTime,bool Absolute,int BufferSize)
	{
		//Add to query list. 

		//Test
		ts.Values = new double[4] {1.0,2.0,3.0,4.0};
		ts.TimeStamps = new double[4] {1452691843.0,1452691849.0,1452691858.0,1452691890.0};
		ts.BufferValid = true;
		ts.CurrentSize = BufferSize;
	}


}
