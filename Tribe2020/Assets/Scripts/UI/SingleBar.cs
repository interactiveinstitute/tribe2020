using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SingleBar : MonoBehaviour {

	public double Value=70;
	public double Scale=1;
	public double Height=100;
	public double Duration=1;

	public Image Bar;
	public double EaseTo;
	double eStart;
	double eDiff;
	double eStartValue;
	bool easing = false;




	// Use this for initialization
	void Start () {
		Bar = GetComponents<Image> ()[0];

		var BarRectTransform = Bar.transform as RectTransform;
		Height = BarRectTransform.sizeDelta.y;
	}
	
	// Update is called once per frame
	void Update () {
		double now = Time.unscaledTime;

		Ease (now);
		
	}

	void SetValue(double Value){
		EaseTo = Value;

	}

	void Ease(double now){

		if (EaseTo == Value) {
			easing = false;
			return;
		}

		//Init new ease?
		if (easing == false) {
			eStart = now;
			eDiff = EaseTo - Value;
			eStartValue = Value;
			easing = true;
		}

		double delta = now - eStart;

		if (delta > Duration)
		{
			Value = EaseTo;
			easing = false;
			return;
		}

		Value = eStartValue + eDiff * ParametricBlend (delta / Duration);

		//Set rect
		var BarRectTransform = Bar.transform as RectTransform;
		BarRectTransform.sizeDelta = new Vector2 (BarRectTransform.sizeDelta.x,(float)(Value*Scale*Height));

	}

	double ParametricBlend(double t)
	{ 
		double sqt = t*t;
		return sqt / (2.0 * (sqt - t) + 1.0);
	}
}
