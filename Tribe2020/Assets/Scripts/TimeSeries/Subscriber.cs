using UnityEngine;
using System.Collections;

public class Subscriber : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}


	virtual public void Data_Update(JSONObject json) {
		print("Unhandled data!");
	}


}


