using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SurveyProgress : MonoBehaviour {
	public Color curColor;
	public GameObject dotPrefab;
	private List<Image> _progressDots;

	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update () {
	}

	//
	public void Init(int size) {
		_progressDots = new List<Image>();

		for(int i = 0; i < size; i++) {
			GameObject newDot = Instantiate(dotPrefab);
			newDot.transform.SetParent(transform, false);
			string hej = "" + _progressDots.Count;
			_progressDots.Add(newDot.GetComponent<Image>());
		}
	}

	//
	public void SetCurrent(int index) {
		foreach(Image dotImg in _progressDots) {
			dotImg.color = Color.white;
		}

		_progressDots[index].color = curColor;
	}
}
