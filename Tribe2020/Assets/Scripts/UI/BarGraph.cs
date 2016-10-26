using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class BarGraph : MonoBehaviour {
	public GameObject linePrefab;
	public Color color = Color.red;

	public int maxValue = 100;
	public int minValue = 0;
	public float value;
	private float _targetValue;

	public ElectricMeter electricMeter;
	public TimeSeries timeSeries;

	private GameObject _line;

	// Use this for initialization
	void Start () {
		_line = Instantiate(linePrefab);
		_line.GetComponent<RawImage>().color = color;
		_line.GetComponent<RectTransform>().Rotate(0, 180, 90);
		_line.transform.SetParent(transform, false);
		_line.transform.localPosition = Vector2.zero;
	}
	
	// Update is called once per frame
	void Update () {
		if(timeSeries != null) {
			PushValue((float)timeSeries.GetCurrentValue());
		} else {
			PushValue(electricMeter.Power);
		}

		if(Mathf.Abs(value - _targetValue) > 0.1f) {
			value = _targetValue - (_targetValue - value) * 0.75f;
		} else {
			value = _targetValue;
		}

		Refresh();
	}

	//
	public void PushValue(float value) {
		if(!float.IsNaN(value)) {
			_targetValue = value;
		} else {
			_targetValue = 0;
		}
	}

	public void Refresh() {
		float scaledValue = value / (maxValue - minValue) * GetComponent<RectTransform>().rect.height;

		_line.GetComponent<RectTransform>().sizeDelta = new Vector2(scaledValue, GetComponent<RectTransform>().rect.width);
	}
}
