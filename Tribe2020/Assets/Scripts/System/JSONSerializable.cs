using UnityEngine;
using System.Collections;
using SimpleJSON;

public class JSONSerializable : MonoBehaviour {
	public string jsonKey;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	// Save function
	public JSONClass Encode() {
		JSONClass jsonRoot = new JSONClass();

		JSONArray json = new JSONArray();
		jsonRoot.Add(jsonKey, json);

		return jsonRoot;
	}

	// Load function
	public void Decode(JSONClass questStateJSON) {
	}
}
