using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class AnimationUI : MonoBehaviour {
	public Sprite arrowImg;
	public Sprite pointingHandImg;
	public Sprite circleImg;

	public Image currrentImage;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	//
	public void SetImage(Sprite sprite) {
		currrentImage.sprite = sprite;
	}
}
