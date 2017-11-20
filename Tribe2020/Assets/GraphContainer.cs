using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GraphContainer : MonoBehaviour {

	public Graph[] graphs; 
	public double Min,Max;
	public double DataMin, DataMax;
	public bool AutoMax = true,AutoMin= false;

	public enum ScaleTypeEnum {SameScale, MixedScale};

	public ScaleTypeEnum ScaleType;


	// Use this for initialization
	void Start () {
		graphs = gameObject.GetComponentsInChildren<Graph>();
	}
	
	// Update is called once per frame
	void Update () {
		FindMinMax ();

		Ease ();
			
	}

	void FindMinMax(){

		double min = double.PositiveInfinity, max = double.NegativeInfinity;
		
		foreach (Graph graph in graphs) {
			if (graph.DataMin < min)
				min = graph.DataMin;

			if (graph.DataMax > max)
				max = graph.DataMax;

		}

		DataMin = min;
		DataMax = max;

		if (AutoMax)
			Max = DataMax;
	}

	void Ease(){

		if (ScaleType == ScaleTypeEnum.SameScale)
		foreach (Graph graph in graphs) {
				graph.SetMax (Max);
		}

	}

}
