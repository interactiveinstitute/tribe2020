using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ImageBar : MonoBehaviour {
	public int maxValue;
	private int _lastMaxValue = 0;

	public int value;
	private int _lastValue = 0;

	public GameObject imagePrefab;
	public Sprite sprite;
	public Color tint;
	//public GameObject imagePrefab;

	// Use this for initialization
	void Start () {
		foreach(Transform child in transform) {
			Destroy(child.gameObject);
		}
	}
	
	// Update is called once per frame
	void Update () {
		//Refresh max values (number of images)
		if(_lastMaxValue != maxValue) {
			if(maxValue > _lastMaxValue) {
				for(int i = maxValue - _lastMaxValue; i > 0; i--) {
					if(imagePrefab) {
						InstantiateImage();
					} else {
						CreateImage();
					}
				}
			} else {
				for(int i = _lastMaxValue - maxValue; i > 0; i--) {
					Destroy(transform.GetChild(i).gameObject);
				}
			}
			_lastMaxValue = maxValue;
		}

		//Refresh values (number of colored images)
		if(_lastValue != value) {
			for(int i = 0; i < maxValue; i++) {
				Image valueImg = transform.GetChild(i).GetComponent<Image>();
				Animator changeAnimator = transform.GetChild(i).GetComponent<Animator>();
				if(i < value && i > _lastValue) {
					valueImg.color = tint;
					changeAnimator.Play("Change");
				} else if(i >= value && i < _lastValue) {
					valueImg.color = Color.black;
					changeAnimator.Play("Change");
				}
			}
			_lastValue = value;
		}
	}

	//
	public void InstantiateImage() {
		GameObject newImgGo = Instantiate(imagePrefab, transform);
		Image newImg = newImgGo.GetComponent<Image>();
		newImg.color = tint;
	}

	//
	public void CreateImage() {
		GameObject newImgGO = Instantiate(new GameObject(), transform);
		Image newImg = newImgGO.AddComponent<Image>();
		newImg.sprite = sprite;
		newImg.color = tint;
		newImg.preserveAspect = true;
	}
}
