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
}
