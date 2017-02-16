using UnityEngine;
using System.Collections;

public class MenuView : View {
	private LocalisationManager _localMgr;
	private SaveManager _saveMgr;

	public Animator animator;

	// Use this for initialization
	void Start () {
		_localMgr = LocalisationManager.GetInstance();
		_saveMgr = SaveManager.GetInstance();
	}
	
	// Update is called once per frame
	void Update () {
		//animator.SetFloat("Speed", 100);
	}

	//
	public void OnLanguagePicked(string language) {
		_localMgr.SetLanguage(language);

		_saveMgr.SetData("language", _localMgr.curLanguage.name);
		_saveMgr.Save();
	}
}
