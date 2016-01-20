using UnityEngine;
using System.Collections;

public class Thing : MonoBehaviour {
	public string type;

	// Use this for initialization
	void Start(){
	
	}
	
	// Update is called once per frame
	void Update(){
	
	}

	//
	public string Stringify(){
		Vector3 pos = transform.position;
		return "{\"type\":\"" + type +"\", \"pos\":[" + pos.x + "," + pos.y + "," + pos.z + "]}";
	}
}
