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

	private PilotController _controller;

	[Header("Header")]
	public Transform cash;
	public Transform comfort;
	public Transform title;
	public Transform date;

	[Header("Energy")]
	public Transform power;
	public Text energyCounter;
	public RectTransform timeBar;
	public RectTransform apocalypsePointer;
	public Text apocalypsePercent;

	[Header("Character Interface")]
	public Text characterTitle;
	public Text characterDescription;
	public Text characterMood;
	public Image characterMoodImage;
	//public Text characterSatisfaction;
	public Slider characterSatisfactionSlider;
	//public Text characterTmperature;
	public Slider characterTmperatureSlider;
	//public Text characterKnowledge;
	public Slider characterKnowledgeSlider;
	//public Text characterAttitude;
	public Slider characterAttitudeSlider;

	[Header("Quest UI")]
	public GameObject inboxUI;
	public Transform inboxList;
	//public GameObject mailReadUI;
	public Text mailCountText;

	[Header("View Guide")]
	public Transform viewpointGuideUI;
	public Image overviewIcon;
	public Sprite defaultIcon;
	public Sprite lockIcon;
	public Color currentColor;

	[Header("Overlay")]
	public GameObject inspectorUI;
	public Transform inspectorEEMContainer;
	public Transform inspectorActionList;

	public GameObject messageUI;
	public GameObject messageButton;
	public GameObject victoryUI;
	public Text victoryText;

	public Transform animationUI;
	public ParticleSystem fireworks;

	[Header("Generated UI Prefabs")]
	public GameObject actionButtonPrefab;
	public GameObject viewGuideRowPrefab;
	public GameObject viewpointIconPrefab;
	public GameObject mailButtonPrefab;
	public GameObject EEMButtonPrefab;

	public GameObject FeedbackNumber;

	private bool _showSettings;
	private RectTransform _curMenu;
	public List<Transform> menus;

	public RectTransform settingsPanel, energyPanel, comfortPanel, inbox, inspector, apocalypsometer, characterPanel;
	private bool _settingsIsVisible = false;
	#endregion

	//Sort use instead of constructor
	void Awake(){
		_instance = this;
	}

	//Use this for initialization
	void Start(){
		_controller = PilotController.GetInstance();

		//Clear interface
		_curMenu = null;

		apocalypsePointer.localEulerAngles = -90 * Vector3.forward;
		apocalypsePercent.text = "0 %";

		//Clear inbox
		//RemoveChildren(inboxList);
	}
	
	//Update is called once per frame
	void Update(){
		if(_showSettings) {
			Vector2 target = settingsPanel.GetComponent<UIPanel>().targetPosition;
			if(settingsPanel.anchoredPosition.x != target.x || settingsPanel.anchoredPosition.y != target.y) {
				float curX = target.x + (settingsPanel.anchoredPosition.x - target.x) * 0.75f;
				float curY = target.y + (settingsPanel.anchoredPosition.y - target.y) * 0.75f;
				settingsPanel.anchoredPosition = new Vector2(curX, curY);
			}
		} else {
			Vector2 origin = settingsPanel.GetComponent<UIPanel>().originalPosition;
			if(settingsPanel.anchoredPosition.x != origin.x || settingsPanel.anchoredPosition.y != origin.y) {
				float curX = origin.x + (settingsPanel.anchoredPosition.x - origin.x) * 0.75f;
				float curY = origin.y + (settingsPanel.anchoredPosition.y - origin.y) * 0.75f;
				settingsPanel.anchoredPosition = new Vector2(curX, curY);
			}
		}

		foreach(RectTransform menu in menus) {
			if(menu == _curMenu) {
				Vector2 target = menu.GetComponent<UIPanel>().targetPosition;
				if(menu.anchoredPosition.x != target.x || menu.anchoredPosition.y != target.y) {
					float curX = target.x + (menu.anchoredPosition.x - target.x) * 0.75f;
					float curY = target.y + (menu.anchoredPosition.y - target.y) * 0.75f;
					menu.anchoredPosition = new Vector2(curX, curY);
				}
			} else {
				Vector2 origin = menu.GetComponent<UIPanel>().originalPosition;
				if(menu.anchoredPosition.x != origin.x || menu.anchoredPosition.y != origin.y) {
					float curX = origin.x + (menu.anchoredPosition.x - origin.x) * 0.75f;
					float curY = origin.y + (menu.anchoredPosition.y - origin.y) * 0.75f;
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
	public void UpdateViewpointTitle(string viewTitle) {
		title.GetComponent<Text>().text = viewTitle;
	}

	//
	public void UpdateViewpointGuide(Viewpoint[][] viewMatrix, Viewpoint curView, bool overview = false) {
		RemoveChildren(viewpointGuideUI);

		for(int y = viewMatrix.Length - 1; y >= 0; y--) {
			GameObject viewRow = Instantiate(viewGuideRowPrefab) as GameObject;
			viewRow.transform.SetParent(viewpointGuideUI, false);
			for(int x = 0; x < viewMatrix[y].Length; x++) {
				GameObject viewCell = Instantiate(viewpointIconPrefab) as GameObject;
				viewCell.transform.SetParent(viewRow.transform, false);
				if(curView.coordinates.x == x && curView.coordinates.y == y) {
					viewCell.GetComponent<Image>().color = currentColor;
					viewCell.GetComponent<Image>().sprite = defaultIcon;
				} else if(viewMatrix[y][x].locked) {
					viewCell.GetComponent<Image>().sprite = lockIcon;
				} else {
					viewCell.GetComponent<Image>().sprite = defaultIcon;
				}
			}
		}

		if(overview) {
			overviewIcon.color = currentColor;
		} else {          
			overviewIcon.color = Color.white;
		}
	}

	//
	public void UpdateViewpointGuide(int aboveCount, int viewCount, int viewIndex) {
		Transform above = viewpointGuideUI.GetChild(0).transform;
		Transform center = viewpointGuideUI.GetChild(1).transform;
		Transform below = viewpointGuideUI.GetChild(2).transform;

		RemoveChildren(above);
		for(int i = 0; i < aboveCount; i++) {
			GameObject iconObj = Instantiate(viewpointIconPrefab) as GameObject;
			iconObj.transform.SetParent(above, false);
		}

		if(viewCount != center.childCount) {
			RemoveChildren(center);
			for(int i = 0; i < viewCount; i++) {
				GameObject iconObj = Instantiate(viewpointIconPrefab) as GameObject;
				iconObj.transform.SetParent(center, false);

				if(i == viewIndex) {
					iconObj.GetComponent<Image>().color = Color.blue;
				}
			}
		} else {
			for(int i = 0; i < viewCount; i++) {
				Transform curIcon = center.GetChild(i);
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
	public void UpdateTime(float timeFraction) {
		//Debug.Log(timeFraction);
		//float scaledValue = value / (maxValue - minValue) * GetComponent<RectTransform>().rect.height;

		//timeBar.sizeDelta = new Vector2(276 * timeFraction, 0);
		timeBar.offsetMax = new Vector2(-307 + 307 * timeFraction, 0);
	}

	//
	public override void ControlInterface(string id, string action) {
		AnimationUI animation = animationUI.GetComponent<AnimationUI>();

		bool visibility = action == "show";
		switch(id) {
			case "inspector":
				_controller.HideUI();
				GetComponent<Animator>().Play("IdleCanvas");
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
				GetComponent<Animator>().Play(action);
				break;
		}
	}

	//Fill INSPECTOR with details and eem options for selected appliance
	public void BuildInspector(string title, string description, Appliance app) {
		if(title == "") { title = app.title + "!"; }
		if(description == "") { description = app.description + "!"; }

		characterTitle.text = title;
		characterDescription.text = description;

		//inspector.GetComponentsInChildren<Text>()[0].text = title;
		//inspector.GetComponentsInChildren<Text>()[2].text = description;
		BuildEEMInterface(app);
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
					button.onClick.AddListener(() => _controller.ApplyEEM(app, curEEM));
				} else {
					button.onClick.AddListener(() => _controller.SendMessage(eem.callback, eem.callbackArgument));
				}
				eemProps.SetCost(eem.cashCost, eem.comfortCost);
			} else {
				button.interactable = false;
			}

			string eemTitle = _controller.GetPhrase("EEM." + eem.category, curEEM.name, 0);
			if(eemTitle == "") { eemTitle = curEEM.name + "!"; }
			eemProps.title.text = eemTitle;

			buttonObj.GetComponent<Image>().color = eem.color;
			
			eemProps.SetImpact((int)eem.energyFactor, (int)eem.gasFactor, (int)eem.co2Factor, (int)eem.moneyFactor, (int)eem.comfortFactor);

			buttonObj.transform.SetParent(inspectorEEMContainer, false);
		}
	}

	//
	public void ShowMessage(string message, Sprite portrait, bool showAtBottom, bool showOkButton = true) {
		messageUI.SetActive(true);
		messageUI.GetComponentInChildren<Text>().text = message;
		if(portrait) {
			messageUI.transform.GetChild(0).GetComponentInChildren<Image>().sprite = portrait;
			messageUI.transform.GetChild(0).gameObject.SetActive(true);
		} else {
			messageUI.transform.GetChild(0).gameObject.SetActive(false);
		}

		messageButton.SetActive(showOkButton);
	}

	//Fill INBOX interface with ongoing and completed narratives
	public void BuildInbox(List<Quest> currentQuests, List<Quest> completedQuests) {
		RemoveChildren(inboxList);

		foreach(Quest quest in currentQuests) {
			Quest curQuest = quest;
			GameObject mailButtonObj = Instantiate(mailButtonPrefab) as GameObject;

			Mail mail = mailButtonObj.GetComponent<Mail>();
			mail.content = mailButtonObj.transform.GetChild(1).gameObject;
			mail.content.SetActive(false);

			mailButtonObj.GetComponentInChildren<Button>().onClick.AddListener(() => BuildMail(mail, curQuest, 0));

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

			Mail mail = mailButtonObj.GetComponent<Mail>();
			mail.content = mailButtonObj.transform.GetChild(1).gameObject;
			mail.content.SetActive(false);

			mailButtonObj.GetComponentInChildren<Button>().onClick.AddListener(() => BuildMail(mail, curQuest, 0));

			Image[] images = mailButtonObj.GetComponentsInChildren<Image>();
			images[1].color = Color.gray;
			images[2].gameObject.SetActive(false);

			Text[] texts = mailButtonObj.GetComponentsInChildren<Text>();
			texts[0].text = curQuest.title;
			texts[1].text = curQuest.date;
			mailButtonObj.transform.SetParent(inboxList, false);
		}
	}

	//
	public void BuildMail(Mail mail, Quest quest, int index) {
		Transform contentTrans = mail.transform.GetChild(1);
		Text title = contentTrans.GetComponentsInChildren<Text>()[0];
		Text description = contentTrans.GetComponentsInChildren<Text>()[1];
		Text steps = contentTrans.GetComponentsInChildren<Text>()[2];

		title.text = quest.title;
		description.text = quest.description;

		string stepConcat = "";
		foreach(Quest.NarrativeCheck checkStep in quest.checkList) {
			stepConcat += checkStep.description + "\n";
		}
		steps.text = stepConcat;

		mail.content.SetActive(!mail.content.activeSelf);

		//mailObj.transform.SetParent(inboxList, false);
	}

	////Full MAIL with quest data including quest steps and whether they have been completed or not
	//public void BuildMail(Quest quest) {
	//	Text title = mailReadUI.GetComponentsInChildren<Text>()[0];
	//	Text description = mailReadUI.GetComponentsInChildren<Text>()[2];
	//	Text steps = mailReadUI.GetComponentsInChildren<Text>()[4];

	//	title.text = quest.title;
	//	description.text = quest.description;

	//	string stepConcat = "";
	//	foreach(Quest.NarrativeCheck checkStep in quest.checkList) {
	//		stepConcat += checkStep.description + "\n";
	//	}
	//	steps.text = stepConcat;

	//	//string stepConcat = "";
	//	//for(int i = 0; i < quest.questSteps.Count; i++) {
	//	//	if(quest.questSteps[i].condition != Quest.QuestEvent.EMPTY) {
	//	//		if(i < quest.GetCurrentStepIndex()) {
	//	//			stepConcat += " --";
	//	//		} else {
	//	//			stepConcat += "¤ ";
	//	//		}
	//	//		stepConcat += quest.questSteps[i].title + "\n";
	//	//	}
	//	//}
	//	//steps.text = stepConcat;

	//	mailReadUI.SetActive(true);
	//}

	//
	public void EnableEnergyPanel() {
		energyPanel.gameObject.SetActive(true);
	}

	//
	public void EnableComfortPanel() {
		comfortPanel.gameObject.SetActive(true);
	}

	//
	public void DisableEnergyPanel() {
		energyPanel.gameObject.SetActive(false);
	}

	//
	public void DisableComfortPanel() {
		comfortPanel.gameObject.SetActive(false);
	}

	//
	public void ShowApocalypsometer() {
		SetCurrentUI(apocalypsometer);
	}

	//
	public void HideApocalypsometer() {
		SetCurrentUI(null);
	}

	//
	public override void ShowCongratualations(string text) {
		ShowFireworks();
		victoryUI.SetActive(true);
		victoryText.text = text;
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

	////
	//public void HideQuest() {
		
	//	mailReadUI.SetActive(false);
	//	inboxUI.SetActive(true);
	//}

	//
	public void ToggleMenu() {
		_showSettings = !_showSettings;
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
