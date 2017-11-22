using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class LineGraph : MonoBehaviour {
	public GameObject linePrefab;
	public Color color = Color.red;

	private float _stepSize;
	public int maxValue = 100;
	public int minValue = 0;
	public float sampleStep = 1;
	private float _timeCount = 0;
	public List<float> values;

	public ElectricMeter electricMeter;
	public DataSeriesBuffer timeSeries;

	private List<GameObject> _lines;

	// Use this for initialization
	void Start () {
		_lines = new List<GameObject>();

		Refresh();
	}
	
	// Update is called once per frame
	void Update () {
		if(_timeCount > sampleStep) {
			if(timeSeries != null) {
				PushValue((float)timeSeries.GetCurrentValue());
			} else {
				PushValue(electricMeter.GetPower());
			}
			_timeCount = 0;
		}

		_timeCount += Time.deltaTime;
		Refresh();
	}

	//
	public void PushValue(float value) {
		values.RemoveAt(_lines.Count - 1);
		values.Insert(0, value);
	}

	//
	public void Init() {
		for(int i = 0; i < values.Count - 1; i++) {
			GameObject newLine = Instantiate(linePrefab);
			newLine.GetComponent<RawImage>().color = color;
			newLine.transform.SetParent(transform, false);
			_lines.Add(newLine);
		}
	}

	//
	public void Refresh() {
		RectTransform prevTrans = null;
		_stepSize = GetComponent<RectTransform>().rect.width / (values.Count - 1);

		for(int i = 0; i < values.Count - 1; i++) {
			if(_lines.Count <= i || _lines[i] == null) {
				Init();
			}
			GameObject newLine = _lines[i];

			RectTransform newTrans = newLine.GetComponent<RectTransform>();
			if(!float.IsNaN(values[i])) {
				float scaledValue = values[i] / (maxValue - minValue) * GetComponent<RectTransform>().rect.height;
				newTrans.localPosition = new Vector2(i * _stepSize, scaledValue);
			} else {
				newTrans.localPosition = new Vector2(i * _stepSize, 0);
			}

			if(prevTrans != null) {
				//Set line length of previous line
				prevTrans.sizeDelta = new Vector2(Vector2.Distance(prevTrans.localPosition, newTrans.localPosition), 2);

				//Set line angle of previous line
				Vector2 v2 = newTrans.localPosition - prevTrans.localPosition;
				float angle = Mathf.Atan2(v2.y, v2.x) * Mathf.Rad2Deg;
				prevTrans.eulerAngles = new Vector3(0, 0, angle);
				//prevTrans.Rotate(0, 0, angle);
			}

			if(i == values.Count - 2) {
				newTrans.sizeDelta = new Vector2(
					Vector2.Distance(new Vector2(GetComponent<RectTransform>().rect.width, newTrans.localPosition.y),
					newTrans.localPosition), 2);
			}

			prevTrans = newTrans;
		}
	}
}
