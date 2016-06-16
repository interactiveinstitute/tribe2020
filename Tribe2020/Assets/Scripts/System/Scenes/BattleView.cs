using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class BattleView : View {
	//Singleton features
	private static BattleView _instance;
	public static BattleView GetInstance() {
		return _instance;
	}

	public GameObject RisingNumberPrefab;

	public Text foeName;
	public Text foeCPNumber;
	public Text foeEPNumber;
	public Image foeCPBar;
	public Image foeEPBar;

	public Text allyCPNumber;
	public Text allyEPNumber;
	public Image allyCPBar;
	public Image allyEPBar;

	public Text question;
	public Text[] answers;

	public GameObject congratsPanel;
	public Text congratsText;

	public GameObject FeedbackNumber;
	public ParticleSystem fireworks;

	//Sort use instead of constructor
	void Awake() {
		_instance = this;
	}

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	//
	public GameObject CreateFeedback(Vector3 pos, string feedback) {
		GameObject fb = Instantiate(RisingNumberPrefab, pos, Quaternion.identity) as GameObject;
		fb.GetComponent<TextMesh>().text = feedback;
		return fb;
	}

	//
	public void ShowCongratualations(string text) {
		ShowFireworks();
		congratsPanel.SetActive(true);
		congratsText.text = text;
	}

	//
	public void ShowFireworks() {
		fireworks.Play();
	}
}
