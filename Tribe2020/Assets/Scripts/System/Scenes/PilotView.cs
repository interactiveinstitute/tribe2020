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
	private ResourceManager _resourceMgr;

	[Header("Header")]
	public Transform cash;
	public Transform comfort;
	public Transform title;
	public Transform date;

	[Header("Satisfaction")]
	public Text satisfactionTitle;

	[Header("Energy")]
	public Text energyTitle;
	public Transform power;
	public Text energyCounter;
	public RectTransform timeBar;
	public RectTransform deviceContainer;

    [Header("Energy Effeciency Labels")]
    public List<Sprite> EELabels;

    [Header("Inbox")]
	public Text inboxTitle;

	[Header("Apocalypsometer")]
	public Text apocalypsometerTitle;
	public RectTransform apocalypsePointer;
	public Text apocalypsePercent;

	[Header("Menu")]
	public Text menuTitle;
	public Text settingsButtonText;
	public Text quitButtonText;

	[Header("Character Interface")]
	public Text avatarTitle;
	public Text avatarDescription;
	public Image avatarMood;
	public Text avatarTemperatureTitle;
	public Text avatarTemperature;
	public Text avatarEfficiencyTitle;
	public Image avatarEfficiency;
    public Image avatarEfficiencyLabel;
    public Text avatarSatisfactionTitle;
	public Slider avatarSatisfaction;
	public Text avatarKnowledgeTitle;
	public Slider avatarKnowledge;
	public Text avatarAttitudeTitle;
	public Slider avatarAttitude;
	public Text avatarNormTitle;
	public Slider avatarNorm;
	public Text avatarEEMTitle;
	public Transform avatarEEMContainer;

	[Header("Device Interface")]
	public Text deviceTitle;
	public Text deviceDescription;
    public Text devicePowerValue;
	public Text deviceEEMTitle;
	public Transform deviceEEMContainer;
    public Image deviceEfficiencyLabel;

	[Header("Quest UI")]
	public GameObject inboxUI;
	public Transform inboxList;
	//public GameObject mailReadUI;
	public Text mailCountText;

	[Header("View Guide")]
	public Transform viewpointGuideUI;
	//public Image overviewIcon;
	public Sprite defaultIcon;
	public Sprite lockIcon;
	public Color currentColor;

	[Header("Overlay")]
	public GameObject messageUI;
	public GameObject messageButton;
	public GameObject victoryUI;
	public Text victoryText;

	public Transform animationUI;

	[Header("Generated UI Prefabs")]
	public GameObject actionButtonPrefab;
	public GameObject viewGuideRowPrefab;
	public GameObject viewpointIconPrefab;
	public GameObject mailButtonPrefab;
	public GameObject EEMButtonPrefab;
	public GameObject PilotEEMButtonPrefab;
	public GameObject EnergyPanelDevice;

	public GameObject FeedbackNumber;

	private bool _showSettings;
	private RectTransform _curMenu;
	//public List<Transform> menus;

	//public RectTransform settingsPanel, energyPanel, comfortPanel, inbox, apocalypsometer, characterPanel, devicePanel;
	private Dictionary<string, RectTransform> _uiPanels;
	private string _curUIPanel;

	private bool _settingsIsVisible = false;
    #endregion

    public CharacterPanel characterPanel;
    public DevicePanel devicePanel;
	public PilotPanel pilotPanel;
	//public UIPanel pilotPanel;

	//Sort use instead of constructor
	void Awake(){
		_instance = this;
	}

	//Use this for initialization
	void Start(){
		_controller = PilotController.GetInstance();
        _resourceMgr = ResourceManager.GetInstance();

        //Clear interface
        _curMenu = null;
		_curUIPanel = "";
		_uiPanels = new Dictionary<string, RectTransform>();
		foreach(UIPanel uiPanel in GetComponentsInChildren<UIPanel>()) {
			_uiPanels.Add(uiPanel.name, uiPanel.transform as RectTransform);
		}

		apocalypsePointer.localEulerAngles = -90 * Vector3.forward;
		apocalypsePercent.text = "0 %";

        characterPanel = GetComponentInChildren<CharacterPanel>();
        devicePanel = GetComponentInChildren<DevicePanel>();

        //Clear inbox
        //RemoveChildren(inboxList);
    }
	
	//Update is called once per frame
	void Update(){
		if(_uiPanels == null) {
			Debug.Log("here");
		}
		//Lerp ui panels into position
		foreach(RectTransform uiPanel in _uiPanels.Values) {
			if(!uiPanel) {
				Debug.Log("there");
				Debug.Log(uiPanel.name);
			}

			if(_curUIPanel != "" && _uiPanels[_curUIPanel] == uiPanel) {
				LerpTowards(uiPanel, uiPanel.GetComponent<UIPanel>().targetPosition);
			} else {
				LerpTowards(uiPanel, uiPanel.GetComponent<UIPanel>().originalPosition);
			}
		}

		//Lerp game menu separately from other ui panels
		if(_showSettings) {
			LerpTowards(GetUIPanel("Menu"), GetUIPanel("Menu").GetComponent<UIPanel>().targetPosition);
		} else {
			LerpTowards(GetUIPanel("Menu"), GetUIPanel("Menu").GetComponent<UIPanel>().originalPosition);
		}

		//Character panel
		BehaviourAI currentAvatar = characterPanel.currentAvatar;
		if(currentAvatar != null) {
			if(currentAvatar.GetComponent<AvatarMood>().IsUpdated() || currentAvatar.GetComponent<AvatarStats>().IsUpdated()) {
				BuildAvatarPanel(currentAvatar.GetComponent<Appliance>());
			}
		}

		//Device panel
		ElectricDevice currentDevice = devicePanel.currentDevice;
		if(currentDevice != null) {
			if(currentDevice.IsUpdated()) {
				BuildDevicePanel(currentDevice.GetComponent<Appliance>());
			}
		}
    }

	//
	public string GetCurrentUIKey() {
		return _curUIPanel;
	}

	//
	public void SetUIPanel(string key) {
		if(_curUIPanel == key) {
			_curUIPanel = "";
		} else {
			_curUIPanel = key;
		}
	}

	//
	public void LerpTowards(RectTransform t, Vector2 target) {
		if(Vector2.Distance(target, t.anchoredPosition) < 0.1f) {
			t.anchoredPosition = target;
		} else {
			float curX = target.x + (t.anchoredPosition.x - target.x) * 0.75f;
			float curY = target.y + (t.anchoredPosition.y - target.y) * 0.75f;
			t.anchoredPosition = new Vector2(curX, curY);
		}
	}

	//
	public RectTransform GetUIPanel(string key) {
		return _uiPanels[key];
	}

    //
    public void SetController(PilotController controller) {
		_controller = controller;
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

	//TODO: Make pilot name dyanmic
	public void UpdateViewpointTitle(string viewTitle) {
		title.GetComponent<Text>().text = _controller.GetPhrase("Viewpoints." + "San Pablo", viewTitle);
	}

	//TODO: Make pilot name dyanmic
	public void UpdateViewpointTitle(Viewpoint view) {
		title.GetComponent<Text>().text = _controller.GetPhrase("Viewpoints." + "San Pablo", view.name);
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

		//if(overview) {
		//	overviewIcon.color = currentColor;
		//} else {          
		//	overviewIcon.color = Color.white;
		//}
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
	public void TranslateInterface() {
		TranslateText(satisfactionTitle);
		TranslateText(energyTitle);
		TranslateText(inboxTitle);
		TranslateText(apocalypsometerTitle);

		//Settings menu
		TranslateText(menuTitle);
		TranslateText(settingsButtonText);
		TranslateText(quitButtonText);

		//Avatar panel
		TranslateText(characterPanel.avatarTemperatureTitle);
		TranslateText(characterPanel.avatarEfficiencyTitle);
		TranslateText(characterPanel.avatarSatisfactionTitle);
		TranslateText(characterPanel.avatarKnowledgeTitle);
		TranslateText(characterPanel.avatarAttitudeTitle);
		TranslateText(characterPanel.avatarNormTitle);
		TranslateText(characterPanel.avatarEEMTitle);

		//Device panel
		TranslateText(deviceEEMTitle);

		//Pilot panel
		TranslateText(pilotPanel.panelHeading);
		TranslateText(pilotPanel.infoHeading);
		TranslateText(pilotPanel.temperatureHeading);
		TranslateText(pilotPanel.indoorLabel);
		TranslateText(pilotPanel.outdoorLabel);
		TranslateText(pilotPanel.climateHeading);
		TranslateText(pilotPanel.heatingLabel);
		TranslateText(pilotPanel.coolingLabel);
	}

	//Given a UI text, look for translation using object name as key
	public void TranslateText(Text text) {
		text.text = _controller.GetPhrase("Interface", text.name);
	}

	//Fill INSPECTOR with details and eem options for selected appliance
	public void BuildInspector(string title, string description, Appliance app) {
		if(title == "") { title = app.title + "!"; }
		if(description == "") { description = app.description + "!"; }

		avatarTitle.text = title;
		avatarDescription.text = description;

		BuildEEMInterface(app);
	}

    //Prepare and show interface for inspecting and avatar
    public void BuildAvatarPanel(Appliance app) {
        avatarTitle.text = app.title;
        avatarDescription.text = _controller.GetPhrase("Avatars", app.title, 0);

        characterPanel.BuildPanel(app.gameObject);

        BuildEEMInterface(avatarEEMContainer, app, EEMButtonPrefab);
    }

	//Prepare and show interface for inspecting a device
	public void BuildDevicePanel(Appliance app) {
        deviceTitle.text = _controller.GetPhrase("Content.Appliances", app.title);
        deviceDescription.text = _controller.GetPhrase("Content.Appliances", app.title, 0);

        devicePanel.BuildPanel(app);

        BuildEEMInterface(deviceEEMContainer, app, EEMButtonPrefab);
	}

	//
	public void BuildPilotPanel() {
		BuildEEMInterface(pilotPanel.eemContainer, _controller.GetPilotAppliance(), PilotEEMButtonPrefab);
	}

	//Fill EEM CONTAINER of inspector with relevant eems for selected appliance
	public void BuildEEMInterface(Transform container, Appliance app, GameObject buttonPrefab) {
		RemoveChildren(container);
		//Render list of eems in this appliance
        foreach (EnergyEfficiencyMeasure eem in app.GetEEMs()) {
			if(ShouldRenderEEMButton(app, eem)) {
				GameObject buttonObj = BuildEEMButton(app, eem, buttonPrefab);
				buttonObj.transform.SetParent(container, false);
			}
        }
		//Append eems from appliances in slots of this appliance
		foreach(Transform child in app.transform) {
			if(child.GetComponent<ApplianceSlot>()) {
				Appliance slotApp = child.GetChild(0).GetComponent<Appliance>();
				foreach(EnergyEfficiencyMeasure eem in slotApp.GetEEMs()) {
					if(ShouldRenderEEMButton(slotApp, eem)) {
						GameObject buttonObj = BuildEEMButton(slotApp, eem, buttonPrefab);
						buttonObj.transform.SetParent(container, false);
					}
				}
			}
		}
	}

	//Generate eem button given the appliance, the eem and a button prefab
	private GameObject BuildEEMButton(Appliance app, EnergyEfficiencyMeasure eem, GameObject buttonPrefab) {
		EnergyEfficiencyMeasure curEEM = eem;
		GameObject buttonObj = Instantiate(buttonPrefab);

		//Set buttons colors depending on eem type
		EEMButton eemButton = buttonObj.GetComponent<EEMButton>();
		eemButton.title.text = _controller.GetPhrase("EEM." + eem.category, eem.name);
		eemButton.buttonImage.color = _resourceMgr.CanAfford(eem.cashCost,eem.comfortCost) ? eem.color : Color.gray;
		ColorBlock cb = eemButton.button.colors;
		cb.normalColor = eem.color;
		cb.highlightedColor = eem.color + new Color(0.3f, 0.3f, 0.3f);
		eemButton.button.colors = cb;

		//Render button decorations like resource icons and costs
		if(eem.icon) { eemButton.eemIcon.sprite = eem.icon; }
		RenderEEMProperty(eemButton.comfortIcon, eemButton.comfortCost, eem.comfortCost);
		RenderEEMProperty(eemButton.moneyIcon, eemButton.moneyCost, eem.cashCost);
		RenderEEMProperty(eemButton.efficiencyIcon, eemButton.efficiencyEffect, eem.energyFactor);

		Button button = buttonObj.GetComponent<Button>();
		if(!app.appliedEEMs.Contains(curEEM) && _resourceMgr.CanAfford(eem.cashCost, eem.comfortCost)) {
			button.onClick.AddListener(() => _controller.ApplyEEM(app, curEEM));
		} else {
			button.interactable = false;
		}

		return buttonObj;
	}

	//Render element of an eem button, such as a resource cost
	private void RenderEEMProperty(Image icon, Text text, float value) {
		if(icon) {
			icon.gameObject.SetActive(value != 0);
			text.gameObject.SetActive(value != 0);
			text.text = value.ToString();
		}
	}

	//Check whether appliance fullfills eems conditions for being rendered
	private bool ShouldRenderEEMButton(Appliance app, EnergyEfficiencyMeasure eem) {
		//Check if should be rendered
		if(eem.shouldRenderCallback != "") {
			CallbackResult result = new CallbackResult();
			app.SendMessage(eem.shouldRenderCallback, result);
			return result.result;
		}
		return true;
	}

	//Fill EEM CONTAINER of inspector with relevant eems for selected appliance
	public void BuildEEMInterface(Appliance app) {
		//RemoveChildren(inspectorEEMContainer);
		//List<EnergyEfficiencyMeasure> eems = app.GetEEMs();

		//foreach(EnergyEfficiencyMeasure eem in eems) {
		//	EnergyEfficiencyMeasure curEEM = eem;
		//	GameObject buttonObj = Instantiate(EEMButtonPrefab);
		//	EEMButton eemProps = buttonObj.GetComponent<EEMButton>();
		//	Button button = buttonObj.GetComponent<Button>();

		//	if(!app.appliedEEMs.Contains(curEEM)){
		//		if(eem.callback == "") {
		//			button.onClick.AddListener(() => _controller.ApplyEEM(app, curEEM));
		//		} else {
		//			button.onClick.AddListener(() => _controller.SendMessage(eem.callback, eem.callbackArgument));
		//		}
		//		eemProps.SetCost(eem.cashCost, eem.comfortCost);
		//	} else {
		//		button.interactable = false;
		//	}

		//	string eemTitle = _controller.GetPhrase("EEM." + eem.category, curEEM.name, 0);
		//	if(eemTitle == "") { eemTitle = curEEM.name + "!"; }
		//	eemProps.title.text = eemTitle;

		//	buttonObj.GetComponent<Image>().color = eem.color;
			
		//	eemProps.SetImpact((int)eem.energyFactor, (int)eem.gasFactor, (int)eem.co2Factor, (int)eem.moneyFactor, (int)eem.comfortFactor);

		//	buttonObj.transform.SetParent(inspectorEEMContainer, false);
		//}
	}

	//Show message UI given a message, eventual portrait of messenger as well as orientaion properties of UI
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

	//Hide the message UI
	public void HideMessage() {
		messageUI.SetActive(false);
	}

	//Load device appliances into energy panel interface for current view
	public void BuildEnergyPanel(List<ElectricDevice> devices) {
		RemoveChildren(deviceContainer);

		foreach(ElectricDevice device in devices) {
			GameObject deviceCell = Instantiate(EnergyPanelDevice) as GameObject;
			deviceCell.GetComponent<EnergyPanelDevice>().device = device;
			deviceCell.transform.SetParent(deviceContainer, false);
		}
	}

	//Fill INBOX interface with ongoing and completed narratives
	public void BuildInbox(List<Narrative> active, List<Narrative> archive) {
		RemoveChildren(inboxList);

		foreach(Narrative narrative in active) {
			if(narrative.HasChecklist()) {
				BuildMailButton(narrative, false, Color.white);
			}
		}

		foreach(Narrative narrative in archive) {
			if(narrative.HasChecklist()) {
				BuildMailButton(narrative, true, Color.gray);
			}
		}
	}

	//
	public void DestroyInbox() {
		RemoveChildren(inboxList);
	}

	//
	public void BuildMailButton(Narrative narrative, bool isCompleted, Color color) {
		GameObject mailButtonObj = Instantiate(mailButtonPrefab) as GameObject;

		Mail mail = mailButtonObj.GetComponent<Mail>();
		mail.content = mailButtonObj.transform.GetChild(1).gameObject;
		mail.content.SetActive(false);

		mailButtonObj.GetComponentInChildren<Button>().onClick.AddListener(() => BuildMailContent(mail, narrative));

		Image[] images = mailButtonObj.GetComponentsInChildren<Image>();
		images[1].color = color;
		if(isCompleted) {
			images[1].gameObject.SetActive(false);
		} else {
			images[2].gameObject.SetActive(false);
		}

		Text[] texts = mailButtonObj.GetComponentsInChildren<Text>();
		//_controller.GetPhrase(...)
		texts[0].text = narrative.title;
		mailButtonObj.transform.SetParent(inboxList, false);
	}

	//
	public void BuildMailContent(Mail mail, Narrative narrative) {
		Transform contentTrans = mail.transform.GetChild(1);
		Text title = contentTrans.GetComponentsInChildren<Text>()[0];
		//Text description = contentTrans.GetComponentsInChildren<Text>()[1];
		Text steps = contentTrans.GetComponentsInChildren<Text>()[1];

		//_controller.GetPhrase(...)
		//title.text = narrative.title;
		//_controller.GetPhrase(...)
		title.text = narrative.description;

		string stepConcat = "";
		for(int i = 0; i < narrative.GetCurrentStepIndex(); i++) {
			if(narrative.steps[i].inChecklist) {
				//_controller.GetPhrase(...)
				stepConcat += "¤ " + narrative.steps[i].description + "\n";
			}
		}
		//foreach(Narrative.Step ns in narrative.steps) {
		//	if(ns.inChecklist) {
				
		//	}
		//}
		steps.text = stepConcat;

		mail.content.SetActive(!mail.content.activeSelf);

		//mailObj.transform.SetParent(inboxList, false);
	}

	//
	public override void ShowCongratulations(string text) {
		victoryUI.SetActive(true);
		victoryText.text = text;
	}

	//
	public override void ClearView() {
		messageUI.SetActive(false);
		victoryUI.SetActive(false);
	}

	//
	public void ToggleMenu() {
		_showSettings = !_showSettings;
	}

	//
	public RectTransform GetCurrentUI() {
		if(_curUIPanel != "") {
			return _uiPanels[_curUIPanel];
		}
		return null;
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
		return _curUIPanel != "";
	}

	//
	public void PlayUIAnimation(string animation) {
		GetComponent<Animator>().Play(animation, 0, 0);
	}

	//
	public void OnAnimationEvent(string animationEvent){
		_controller.OnAnimationEvent(animationEvent);
	}
}
