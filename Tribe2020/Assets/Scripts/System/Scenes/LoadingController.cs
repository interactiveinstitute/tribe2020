using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoadingController : MonoBehaviour {
	private LocalisationManager _localMgr;
	private SaveManager _saveMgr;

	public Text loadingText;

	// Use this for initialization
	void Start () {
		_localMgr = LocalisationManager.GetInstance();
		_saveMgr = SaveManager.GetInstance();

		_localMgr.SetLanguage(_saveMgr.GetData("language"));

		loadingText.text = _localMgr.GetPhrase("Interface", "loading");
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
