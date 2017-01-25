using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class LocalisationController : MonoBehaviour {
	public SaveManager saveManager;
	public CustomSceneManager sceneManager;

	public List<Language> languages;
	public GameObject localisationButtonPrefab;
	public Transform localisationButtonContainer;

	// Use this for initialization
	void Start () {
		saveManager.Load();
		if(saveManager.GetData("language") != null) {
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
		saveManager.SetData("language", language);
		saveManager.Save();

		sceneManager.LoadScene("MovieScene");
	}
}
