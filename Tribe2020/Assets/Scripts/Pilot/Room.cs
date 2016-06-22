using UnityEngine;
using System.Collections.Generic;

public class Room : MonoBehaviour {
	[SerializeField]
	private List<Appliance> _devices;

	public float temperature;
	public float airQuality;
	public int personCount;

	// Use this for initialization
	void Start () {
		foreach(Appliance device in GetComponentsInChildren<Appliance>()) {
			_devices.Add(device);
		}
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	//
	void OnTriggerEnter(Collider other) {
		if(other.GetComponent<BehaviourAI>()) {
			personCount++;
		}
	}

	//
	void OnTriggerExit(Collider other) {
		if(other.GetComponent<BehaviourAI>()) {
			personCount--;
		}
	}
}
