using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AcopalypseBargraph : MonoBehaviour {
	public Text [] newText ;
	public Image [] Bars ;

	public double[] Outcomes;
	public double[] Baselines;

	public DataSeries Outcome;
	public DataSeries Baseline;

	public double UpdateTime = 0;
	public double UpdateIntervall = 5;

	public double MaxValue = 0;


	// Use this for initialization
	void Start () {

		Outcomes = new double[7];
		Baselines = new double[7];

		newText = GetComponentsInChildren<Text> ();
		Bars = GetComponentsInChildren<Image> ();

		newText [0].text = "Wenesday";
		newText [1].text = "Thursday";
		newText [2].text = "Friday";
		newText [3].text = "Saturday";
		newText [4].text = "Sunday";
		newText [5].text = "Yesterday";
		newText [6].text = "Today";

		UpdateValues ();

	}
	
	// Update is called once per frame
	void Update () {

		double now = Time.unscaledTime;

		if (now - UpdateTime < UpdateIntervall)
			return;

		UpdateTime = now;

		UpdateValues ();
		CalculateMax ();

	}


	void UpdateValues() {
		for (int i = 0; i < 7; i++) {
			Outcomes [i] = Outcome.InterpolateDailyConsumption (i*-1);
		}
		for (int i = 0; i < 7; i++) {
			Baselines [i] = Baseline.InterpolateDailyConsumption (i*-1);
		}
	}

	void CalculateMax() {

		double max=0;

		for (int i = 0; i < 7; i++) {
			if (Baselines [i] > max)
				max = Baselines [i]
		}

		MaxValue = max;
	}
}


