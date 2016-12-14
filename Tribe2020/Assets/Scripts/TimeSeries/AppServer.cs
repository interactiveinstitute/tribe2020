using UnityEngine;
using System.Collections;
using SocketIO;



public class AppServer : SocketIOComponentMod {

	public string[] topics;

	// Use this for initialization
	void Start () {
		//Debug = Subscribers;

		On("open", DoOnOpen);
		On("mqtt", DoOnMqtt);
		On("error", DoOnError);
		On("close", DoOnClose);
		Debug.Log ("Starting");
		base.Start();
	}
		
	// Update is called once per frame
	void Update () {
		base.Update();
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

	public int Subscribe(DataSeriesBuffer ts,string Name,double StartTime,bool Absolute,int BufferSize) {
		return -1;
	}

	//MQTT subscribe to the last incomming message. 
	public int Subscribe(DataSeriesBuffer ts,string Name) {
		return Subscribe( ts, Name, 0, false, 1);
	}

	public void Unsubscribe(int subscriptionID) {

	}

	public void DoOnOpen(SocketIOEvent e)
	{
		//Debug.Log("[SocketIO] Open received: " + e.name + " " + e.data);

	}

	public void DoOnMqtt(SocketIOEvent e)
	{
		Debug.Log("[SocketIO] Mqtt received: " + e.name + " " + e.data);

		if (e.data == null) { return; }

		Debug.Log(
			"#####################################################" +
			"THIS: " + e.data.GetField("this").str +
			"#####################################################"
		);
	}

	public void DoOnError(SocketIOEvent e)
	{
		//Debug.Log("[SocketIO] Error received: " + e.name + " " + e.data);
	}

	public void DoOnClose(SocketIOEvent e)
	{	
		Debug.Log("[SocketIO] Close received: " + e.name + " " + e.data);
	}


}
