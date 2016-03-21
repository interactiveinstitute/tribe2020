﻿using UnityEngine;
using System.Collections;

public class TutorialManager : MonoBehaviour {
	//Singleton features
	private static TutorialManager _instance;
	public static TutorialManager GetInstance() {
		return _instance;

	}
	private InteractionManager _ixnMgr;
	private UIManager _uiMgr;

	public Transform handUI;
	public Transform tutorialAnimation;

	private string _state = "tut_init";

	//Sort use instead of constructor
	void Awake() {
		_instance = this;
	}

	// Use this for initialization
	void Start () {
		_ixnMgr = InteractionManager.GetInstance();
		_uiMgr = UIManager.GetInstance();
	}
	
	// Update is called once per frame
	void Update () {
		switch(_state) {
			case "tut_init":
				_state = "swiping";
				_uiMgr.tutorialUI.SetActive(true);
				break;
			case "swiping":
				_uiMgr.ShowMessage("Swipe to change view");
				tutorialAnimation.GetComponent<Animation>().Play("Swipe");
				//handUI.Translate(Vector2.up * 10 * Time.deltaTime);
				_state = "swiping_pending";
				break;
			case "tapping":
				_uiMgr.ShowMessage("Tap on an object to interact with it");
				tutorialAnimation.GetComponent<Animation>().Play("Tap");
				_state = "tap_pending";
				break;
		}
	}

	//
	public void NextStep() {
		switch(_state) {
			case "swiping_pending":
				_state = "tapping";
				break;
			case "tap_pending":
				_state = "end";
				_uiMgr.tutorialUI.SetActive(false);
				break;
			default:
				break;
		}
	}
}
