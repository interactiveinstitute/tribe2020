using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ImageBar : MonoBehaviour {
	public int value;
	private int _lastValue = 0;
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
		if(_lastValue != value) {
			if(value > _lastValue) {
				for(int i = value - _lastValue; i > 0; i--) {
					GameObject newImgGO = Instantiate(new GameObject(), transform);
					Image newImg = newImgGO.AddComponent<Image>();
					newImg.sprite = sprite;
					newImg.color = tint;
					newImg.preserveAspect = true;
				}
			} else {
				for(int i = _lastValue - value; i > 0; i--) {
					Destroy(transform.GetChild(i).gameObject);
				}
			}
			_lastValue = value;
		}
	}
}
