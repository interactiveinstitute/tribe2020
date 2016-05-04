using UnityEngine;
using System.Collections;


public class Flow {

}

public class Heatflux : Flow {

}



public class HeatProperties : MonoBehaviour {

	[Tooltip("The U-value of the material.")]
	public float U;

	[Tooltip("The materials specific heat capacity")]
	public float c;

	[Tooltip("The current termal energy stored in the object")]
	public float Et;

	[Tooltip("The current temperature")]
	public float T;

	[Tooltip("The volume of the space")]
	public float Volume;

	[Tooltip("This will be used if the temperature comes from a predefined data series")]
	public float DataSeries;

	[Tooltip("This are the other object with heating properties that will influence this node")]
	public MonoBehaviour[] Factors;
	[Tooltip("This are the area of the intersection to the coresponding factor in prevous list")]
	public float[] Area;
	[Tooltip("This is the distance from the center point of this object to the listed factors")]
	public float[] Distance;





	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}


	public void InitiateFlowFunctions() {

	}





}

