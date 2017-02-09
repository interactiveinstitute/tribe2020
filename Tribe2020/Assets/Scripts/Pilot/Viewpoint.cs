using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Viewpoint : MonoBehaviour {
	public bool overview;
	public Vector2 coordinates;
	public int xIndex;
	public int yIndex;
	public string title;

	public List<Room> relatedZones;
	public bool locked;

    public List<GameObject> hideObjects;

	// Use this for initialization
	void Start(){
	
	}
	
	// Update is called once per frame
	void Update(){
	
	}

	//Get appliances contained within all related zones
	public List<Appliance> GetAppliaces() {
		List<Appliance> appliances = new List<Appliance>();
		foreach(Room r in relatedZones) {
			appliances.AddRange(r.GetAppliances());
		}
		return appliances;
	}

	//Get electric devices contained within all related zones
	public List<ElectricDevice> GetElectricDevices() {
		List<ElectricDevice> eDevices = new List<ElectricDevice>();
		foreach(Room r in relatedZones) {
			eDevices.AddRange(r.GetElectricDevices());
		}
		return eDevices;
	}
}
