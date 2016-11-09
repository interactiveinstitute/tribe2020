using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

public class PilotView : View{
	//Singleton features
	public static PilotView GetInstance(){
		return _instance as PilotView;
	}

	#region Fields
	public bool debug = false;

	private PilotController _ctrlMgr;
	private GameTime _timeMgr;
	private MainMeter _energyMgr;
	private ResourceManager _resourceMgr;
	private LocalisationManager _localMgr;

	[Header("Header")]
	public Transform cash;
	public Transform comfort;
	public Transform title;
	public Transform date;

	[Header("Energy")]
	public Transform power;
	public Text energyCounter;

	[Header("Quest UI")]
	public GameObject inboxUI;
	public Transform inboxList;
	public GameObject mailReadUI;
	public Text mailCountText;

	[Header("View Guide")]
	public Transform viewpointGuideUI;

	[Header("Overlay")]
	public GameObject inspectorUI;
	public Transform inspectorEEMContainer;
	public Transform inspectorActionList;

	public GameObject messageUI;
	public GameObject victoryUI;
	public Text victoryText;

	public Transform animationUI;
	public ParticleSystem fireworks;

	[Header("Generated UI Prefabs")]
	public GameObject actionButtonPrefab;
	public GameObject viewpointIconPrefab;
	public GameObject mailButtonPrefab;
	public GameObject EEMButtonPrefab;

	public GameObject FeedbackNumber;

	private RectTransform _curMenu;
	public List<Transform> menus;

	public RectTransform settingsPanel, energyPanel, comfortPanel, inbox, mail, inspector;
	private bool _settingsIsVisible = false;
	#endregion

	//Sort use instead of constructor
	void Awake(){
		_instance = this;
	}

	//Use this for initialization
	void Start(){
		_ctrlMgr = PilotController.GetInstance();
		_timeMgr = GameTime.GetInstance();
		_energyMgr = MainMeter.GetInstance();
		_resourceMgr = ResourceManager.GetInstance();
		_localMgr = LocalisationManager.GetInstance();

		//Clear interface
		_curMenu = null;

		//Clear inbox
		RemoveChildren(inboxList);
	}
	
	//Update is called once per frame
	void Update(){
		date.GetComponent<Text>().text = _timeMgr.CurrentDate;

		power.GetComponent<Text>().text = Mathf.Floor(_energyMgr.Power) + " W";

		float energy = (float)_energyMgr.Energy;
		if(energy < 1) {
			energyCounter.text = Mathf.Floor(energy * 1000) + " Wh";
		} else {
			energyCounter.text = Mathf.Floor(energy) + " kWh";
		}

		foreach(RectTransform menu in menus) {
			Vector2 origPos = menu.GetComponent<UIPanel>().originalPosition;
			Vector2 targetPos = menu.GetComponent<UIPanel>().targetPosition;

			if(menu == _curMenu) {
				if(menu.anchoredPosition.x != targetPos.x || menu.anchoredPosition.y != targetPos.y) {
					float curX = targetPos.x + (menu.anchoredPosition.x - targetPos.x) * 0.75f;
					float curY = targetPos.y + (menu.anchoredPosition.y - targetPos.y) * 0.75f;
					//curX = 0 + (menu.anchoredPosition.x + 0) * 0.75f;
					menu.anchoredPosition = new Vector2(curX, curY);
				}
			} else {
				if(menu.anchoredPosition.x != origPos.x || menu.anchoredPosition.y != origPos.y) {
					float curX = origPos.x + (menu.anchoredPosition.x - origPos.x) * 0.75f;
					float curY = origPos.y + (menu.anchoredPosition.y - origPos.y) * 0.75f;
					//curX = origPos.x + (menu.anchoredPosition.x - origPos.x) * 0.75f;
					menu.anchoredPosition = new Vector2(curX, curY);
				}
			}
		}
	}

	//
	public void RemoveChildren(Transform parent){
		List<Transform> children = new List<Transform>();
		foreach(Transform child in parent){
			children.Add(child);
		}
		children.ForEach(child => Destroy(child.gameObject));
	}

	//
	public GameObject CreateFeedback(Vector3 pos, string feedback){
		GameObject fb = Instantiate(FeedbackNumber, pos, Quaternion.identity) as GameObject;
		fb.GetComponent<TextMesh>().text = feedback;
		return fb;
	}

	//
	public void UpdateViewpointGuide(int viewCount, int viewIndex) {
		if(viewCount != viewpointGuideUI.childCount) {
			RemoveChildren(viewpointGuideUI);
			for(int i = 0; i < viewCount; i++) {
				GameObject iconObj = Instantiate(viewpointIconPrefab) as GameObject;
				iconObj.transform.SetParent(viewpointGuideUI, false);

				if(i == viewIndex) {
					iconObj.GetComponent<Image>().color = Color.blue;
				}
			}
		} else {
			for(int i = 0; i < viewCount; i++) {
				Transform curIcon = viewpointGuideUI.GetChild(i);
				if(i == viewIndex) {
					curIcon.GetComponent<Image>().color = Color.blue;
				} else {
					curIcon.GetComponent<Image>().color = Color.white;
				}
			}
		}
	}

	//
	public void UpdateQuestCount(int questCount) {
		mailCountText.text = "" + questCount;
	}

	//
	public override void ControlInterface(string id, string action) {
		AnimationUI animation = animationUI.GetComponent<AnimationUI>();

		bool visibility = action == "show";
		switch(id) {
			case "inspector":
				_ctrlMgr.HideUI();
				//inspectorUI.SetActive(visibility);
				break;
			case "animation":
				if(debug) { Debug.Log(name + ": ControlAnimation " + visibility); }
				foreach(Transform t in animationUI.transform) {
					t.gameObject.SetActive(visibility);
				}
				//animationUI.gameObject.SetActive(visibility);
				break;
			case "playAnimation":
				if(debug) { Debug.Log(name + ": PlayAnimation " + animationUI.GetComponent<Animation>().GetClip(action)); }
				animationUI.GetComponent<Animation>().Play(action);
				break;
		}
	}

	//Fill INSPECTOR with details and eem options for selected appliance
	public void BuildInspector(Appliance appliance) {
		string title = _localMgr.GetPhrase("Appliance:" + appliance.title + "_Title");
		string description = _localMgr.GetPhrase("Appliance:" + appliance.title + "_Description");

		if(title == "") { title = appliance.title + "!"; }
		if(description == "") { description = appliance.description + "!"; }

		inspector.GetComponentsInChildren<Text>()[0].text = title;
		inspector.GetComponentsInChildren<Text>()[2].text = description;
		BuildEEMInterface(appliance);
	}

	//Fill EEM CONTAINER of inspector with relevant eems for selected appliance
	public void BuildEEMInterface(Appliance app) {
		RemoveChildren(inspectorEEMContainer);
		List<EnergyEfficiencyMeasure> eems = app.GetEEMs();

		foreach(EnergyEfficiencyMeasure eem in eems) {
			EnergyEfficiencyMeasure curEEM = eem;
			GameObject buttonObj = Instantiate(EEMButtonPrefab);
			EEMButton eemProps = buttonObj.GetComponent<EEMButton>();
			Button button = buttonObj.GetComponent<Button>();

			if(!app.appliedEEMs.Contains(curEEM)){
				if(eem.callback == "") {
					button.onClick.AddListener(() => _ctrlMgr.ApplyEEM(app, curEEM));
				} else {
					button.onClick.AddListener(() => _ctrlMgr.SendMessage(eem.callback, eem.callbackArgument));
				}
				eemProps.SetCost(eem.cashCost, eem.comfortCost);
			} else {
				button.interactable = false;
			}

			string eemTitle = _localMgr.GetPhrase("EEM." + eem.category + ":" + curEEM.name + "_Title");
			if(eemTitle == "") { eemTitle = curEEM.name + "!"; }
			eemProps.title.text = eemTitle;

			buttonObj.GetComponent<Image>().color = eem.color;
			
			eemProps.SetImpact((int)eem.energyFactor, (int)eem.gasFactor, (int)eem.co2Factor, (int)eem.moneyFactor, (int)eem.comfortFactor);

			buttonObj.transform.SetParent(inspectorEEMContainer, false);
		}
	}

	//
	public override void ShowMessage(string message, bool showAtBottom, bool showOkButton = true) {
		messageUI.SetActive(true);
		messageUI.GetComponentInChildren<Text>().text = message;

		messageUI.transform.GetChild(1).GetChild(1).gameObject.SetActive(showOkButton);

		//RectTransform messageTrans = messageUI.transform as RectTransform;
		//if(showAtBottom) {
		//	messageTrans.pivot = Vector2.zero;
		//	messageTrans.anchoredPosition = Vector3.zero;
		//	messageTrans.anchorMax = Vector2.zero;
		//} else {
		//	messageTrans.pivot = Vector2.up;
		//	messageTrans.anchoredPosition = Vector3.zero;
		//	messageTrans.anchorMax = Vector2.up;
		//}
	}

	//Fill INBOX interface with ongoing and completed narratives
	public void BuildInbox(List<Quest> currentQuests, List<Quest> completedQuests) {
		RemoveChildren(inboxList);

		foreach(Quest quest in currentQuests) {
			Quest curQuest = quest;
			GameObject mailButtonObj = Instantiate(mailButtonPrefab) as GameObject;
			mailButtonObj.GetComponent<Button>().onClick.AddListener(() => _ctrlMgr.SetCurrentUI(curQuest));

			Image[] images = mailButtonObj.GetComponentsInChildren<Image>();
			images[2].gameObject.SetActive(false);

			Text[] texts = mailButtonObj.GetComponentsInChildren<Text>();
			texts[0].text = curQuest.title;
			texts[1].text = curQuest.date;
			mailButtonObj.transform.SetParent(inboxList, false);
		}

		foreach(Quest quest in completedQuests) {
			Quest curQuest = quest;
			GameObject mailButtonObj = Instantiate(mailButtonPrefab) as GameObject;

			mailButtonObj.GetComponent<Button>().onClick.AddListener(() => _ctrlMgr.SetCurrentUI(curQuest));

			Image[] images = mailButtonObj.GetComponentsInChildren<Image>();
			images[0].color = Color.gray;
			images[1].gameObject.SetActive(false);

			Text[] texts = mailButtonObj.GetComponentsInChildren<Text>();
			texts[0].text = curQuest.title;
			texts[1].text = curQuest.date;
			mailButtonObj.transform.SetParent(inboxList, false);
		}
	}

	//Full MAIL with quest data including quest steps and whether they have been completed or not
	public void BuildMail(Quest quest) {
		Text title = mailReadUI.GetComponentsInChildren<Text>()[0];
		Text description = mailReadUI.GetComponentsInChildren<Text>()[2];
		Text steps = mailReadUI.GetComponentsInChildren<Text>()[4];

		title.text = quest.title;
		description.text = quest.description;

		string stepConcat = "";
		for(int i = 0; i < quest.questSteps.Count; i++) {
			if(quest.questSteps[i].condition != Quest.QuestEvent.EMPTY) {
				if(i < quest.GetCurrentStepIndex()) {
					stepConcat += " --";
				} else {
					stepConcat += "¤ ";
				}
				stepConcat += quest.questSteps[i].title + "\n";
			}
		}
		steps.text = stepConcat;

		mailReadUI.SetActive(true);
	}

	public void HideQuestList() {
		mailReadUI.SetActive(false);
		_curMenu = null;
	}

	//
	public override void ShowCongratualations(string text) {
		ShowFireworks();
		_ctrlMgr.PlaySound("fireworks");
		victoryUI.SetActive(true);
		victoryText.text = _localMgr.GetPhrase(text);
	}

	//
	public void ShowFireworks() {
		fireworks.Play();
	}

	//
	public override void ClearView() {
		messageUI.SetActive(false);
		victoryUI.SetActive(false);
	}

	//
	public void HideQuest() {
		
		mailReadUI.SetActive(false);
		inboxUI.SetActive(true);
	}

	//
	public void ShowSettings() {
		_curMenu = settingsPanel;
	}

	//
	public void HideSettings() {
		_curMenu = null;
	}

	//
	public void SetCurrentUI(RectTransform ui) {
		if(ui && ui.GetComponent<UIPanel>().toggleButton) {
			ui.GetComponent<UIPanel>().toggleButton.Rotate(new Vector3(0, 0, 180));
		}
		if(_curMenu == ui) {
			_curMenu = null;
		} else {
			if(_curMenu && _curMenu.GetComponent<UIPanel>().toggleButton) {
				_curMenu.GetComponent<UIPanel>().toggleButton.Rotate(new Vector3(0, 0, 180));
			}
			_curMenu = ui;
		}
	}

	//
	public RectTransform GetCurrentUI() {
		return _curMenu;
	}

	//
	public void AddQuest() {

	}

	//
	public void RemoveQuest() {
	}

	//
	//public bool AllUIsClosed() {
	//	return _curMenu == null && !inspectorUI.activeSelf;
	//}

	//
	public bool IsAnyOverlayActive() {
		return _curMenu != null;
	}
}
