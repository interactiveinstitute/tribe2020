using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AcopalypseBargraph : MonoBehaviour {
	public SingleBar[] Bars;

	public double[] Outcomes;
	public double[] Baselines;

	public DataSeries Outcome;
	public DataSeries Baseline;

	public double UpdateTime = 0;
	public double UpdateIntervall = 1;

	public double MaxValue = 0;
	public double VScale = 10000 * 0.1f;
	public double height = 200;

	public double EaseStart = 0;
	public double EaseDiff = 0;
	public double EaseStartValue = 0;
	public double EaseDuration = 5;
	public double EaseTarget = 0;

	public double EaseDebug = 0;

	GameTime time;
	public Text[] newText;

	// Use this for initialization
	void Start() {
		if(Outcome == null)
			Outcome = DataContainer.GetInstance().cO2Outcome;
		if(Baseline == null)
			Baseline = DataContainer.GetInstance().cO2Baseline;

		//print("script was started");

		Outcomes = new double[7];
		Baselines = new double[7];

		//Bars = GetComponentsInChildren<SingleBar>();

		UpdateValues();

		newText = GetComponentsInChildren<Text>();
		time = GameTime.GetInstance();

		UpdateLegend();
	}

	void OnEnable() {
		//print("script was enabled");
		if (Outcome == null)
			return;
		if (Baseline == null)
			return;
			
		double now = Time.unscaledTime;
		
		AutoScale (now);

		UpdateTime = now;

		UpdateValues();
		CalculateMax();
		UpdateLegend();
		UpdateBars();
	}

	// Update is called once per frame
	void Update() {
		double now = Time.unscaledTime;

		AutoScale (now);

		if(now - UpdateTime < UpdateIntervall)
			return;

		UpdateTime = now;

		UpdateValues();
		CalculateMax();
		UpdateLegend();
		UpdateBars();
	}

	//
	void AutoScale(double now) {
		if(MaxValue == VScale)
			return;

		//Init new ease?
		if(EaseTarget != MaxValue) {
			EaseStart = now;
			EaseDiff = MaxValue - VScale;
			EaseStartValue = VScale;
			EaseTarget = MaxValue;
		}

		double delta = now - EaseStart;

		if(delta > EaseDuration) {
			EaseStart = 0;
			VScale = EaseTarget;
			return;
		}

		EaseDebug = ParametricBlend(delta / EaseDuration);

		VScale = EaseStartValue + EaseDiff * ParametricBlend(delta / EaseDuration);
	}

	//Quadratic ease out where t = time, b = startvalue, c = change in value, d = duration:
	double ParametricBlend(double t) {
		double sqt = t * t;
		return sqt / (2.0 * (sqt - t) + 1.0);
	}

	//
	void UpdateValues() {
		for(int i = 0; i < 7; i++) {
			Outcomes[i] = Outcome.InterpolateDailyConsumption(i * -1);
		}
		for(int i = 0; i < 7; i++) {
			Baselines[i] = Baseline.InterpolateDailyConsumption(i * -1);
		}
	}

	//
	void UpdateBars() {
		for(int i = 0; i < 7; i++) {
			if(i > Bars.Length)
				break;

			Bars[i].EaseTo = Outcomes[i] / MaxValue;
		}

		for(int i = 0; i < 7; i++) {
			if(i > Bars.Length)
				break;

			Bars[i + 7].EaseTo = Baselines[i] / MaxValue;
		}
	}

	//
	void CalculateMax() {
		double max = 0;

		for(int i = 0; i < 7; i++) {

			if(Baselines[i] > max)
				max = Baselines[i];
		}

		for(int i = 0; i < 7; i++) {

			if(Outcomes[i] > max)
				max = Outcomes[i];
		}

		MaxValue = max;
	}

	//
	void UpdateLegend() {
		newText[0 + 3].text = time.GetDay(-6);
		newText[1 + 3].text = time.GetDay(-5);
		newText[2 + 3].text = time.GetDay(-4);
		newText[3 + 3].text = time.GetDay(-3);
		newText[4 + 3].text = time.GetDay(-2);
		newText[5 + 3].text = "Yesterday";
		newText[6 + 3].text = "Today";

	}
}


