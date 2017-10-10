using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Legend : MonoBehaviour {
	GameTime time;
	public Text [] newText ;
	double UpdateTime=0,UpdateIntervall=1;

	// Use this for initialization
	void Start () {

		newText = GetComponentsInChildren<Text> ();
		time = GameTime.GetInstance ();

		UpdateLegend ();
		
	}

	
	// Update is called once per frame
	void Update () {
		double now = Time.unscaledTime;

		if (now - UpdateTime < UpdateIntervall)
			return;

		UpdateTime = now;

		UpdateLegend ();
		
	}


	void UpdateLegend(){

		newText [0].text = time.GetDay (-6);
		newText [1].text = time.GetDay (-5);
		newText [2].text = time.GetDay (-4);
		newText [3].text = time.GetDay (-3);
		newText [4].text = time.GetDay (-2);
		newText [5].text = "Yesterday";
		newText [6].text = "Today";

	}
}
