using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class LocalisationController : MonoBehaviour {
	private SaveManager _saveMgr;
	public CustomSceneManager sceneManager;
	private LocalisationManager _localMgr;

	public List<Language> languages;
	public GameObject localisationButtonPrefab;
	public Transform localisationButtonContainer;

	// Use this for initialization
	void Start () {
		_saveMgr = SaveManager.GetInstance();
		_localMgr = LocalisationManager.GetInstance();

		//_localMgr.DeserializeFromJSON(_saveMgr.GetData("language"));

		if(_saveMgr.GetData("language") != null) {
			sceneManager.LoadScene("MovieScene");
		}

		foreach(Language lang in languages) {
			Language curLang = lang;
			GameObject newButton = Instantiate(localisationButtonPrefab);
			newButton.transform.SetParent(localisationButtonContainer, false);
			newButton.GetComponent<Image>().sprite = curLang.flagSprite;
			newButton.GetComponentInChildren<Button>().onClick.AddListener(() => OnLanguagePicked(curLang.name));
		}
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	//
	public void OnLanguagePicked(string language) {
		_localMgr.SetLanguage(language);

		_saveMgr.SetData("language", _localMgr.curLanguage.name);
		//_saveMgr.SetData("language", language);
		_saveMgr.Save();

		sceneManager.LoadScene("MovieScene");
	}
}
