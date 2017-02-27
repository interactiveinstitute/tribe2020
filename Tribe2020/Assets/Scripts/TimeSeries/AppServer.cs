using UnityEngine;
using System.Collections;
using SocketIO;
using System.Collections.Generic; 



public class AppServer : SocketIOComponentMod {

	// Use this for initialization
	void Start () {
		//Debug = Subscribers;

		On("open", DoOnOpen);
		On("mqtt", DoOnMqtt);
		On("error", DoOnError);
		On("close", DoOnClose);
		Debug.Log ("Starting: " + NodeName);

        

		base.Start();
	}
		    
	// Update is called once per frame
	void Update () {
		base.Update();
	}

    public bool MQTTsubscribe(string topic) {
        //JSONObject json = new JSONObject("\"topic\":\"" + topic + "\"");

        if (!IsConnected)
            return false;

        Dictionary<string, string> data = new Dictionary<string, string>();
        data["topic"] = topic;
        
        Emit("subscribe", new JSONObject(data));

        return true;
    }

    override public bool SubscribeTopic(string Topic)
    {
        return MQTTsubscribe(Topic);
    }



    public void DoOnOpen(SocketIOEvent e)
	{
   
        //Debug.Log("[SocketIO] Open received: " + e.name + " " + e.data);
        //MQTTsubscribe("test/signal");

        OnConnect();
    }

	public void DoOnMqtt(SocketIOEvent e)
	{
        string name = NodeName;

        //Debug.Log(NodeName + ": [SocketIO] Mqtt received: " + e.name + " " + e.data);

  


        UpdateAllTargets(e.name, e.data);


        if (e.data == null) { return; }

		Debug.Log(
			"#####################################################" +
			"THIS: " + e.data.GetField("this").str +
			"#####################################################"
		);


        //DataPoint Data = new DataPoint();

        //Data.Texts = e.data;

       



    }

	public void DoOnError(SocketIOEvent e)
	{
		//Debug.Log("[SocketIO] Error received: " + e.name + " " + e.data);
	}

	public void DoOnClose(SocketIOEvent e)
	{	
		Debug.Log(NodeName + ": [SocketIO] Close received: " + e.name + " " + e.data);
	}


}
