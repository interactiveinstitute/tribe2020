using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Viewpoint : MonoBehaviour {
	public int xIndex;
	public int yIndex;
	public string title;

	public bool showFloor, showPilot;
	public int floor;
	public bool hideNorth, hideEast, hideSouth, hideWest;
    public List<GameObject> hideObjects;

	// Use this for initialization
	void Start(){
	
	}
	
	// Update is called once per frame
	void Update(){
	
	}
}
