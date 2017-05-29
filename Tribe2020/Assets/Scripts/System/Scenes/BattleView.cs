using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class BattleView : MonoBehaviour {
	//Singleton features
	private static BattleView _instance;
	public static BattleView GetInstance() {
		return _instance as BattleView;
	}

	public GameObject RisingNumberPrefab;

	public GameObject dialogueUI;
	public GameObject barsUI;
	public GameObject actionsUI;
	public GameObject levelUpUI;

	public Text foeName;
	public Text foeCPNumber;
	public Image foeCPBar;

	public Text allyCPNumber;
	public Image allyCPBar;

	public Text levelUpName;
	public Slider avatarSatisfaction;
	public Slider avatarKnowledge;
	public Slider avatarAttitude;
	public Slider avatarNorm;

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
		_instance.fireworks.Play();
	}

	//
	public void ClearView() {
	}

	//
	public void ControlInterface(string id, string action) {
	}
}
