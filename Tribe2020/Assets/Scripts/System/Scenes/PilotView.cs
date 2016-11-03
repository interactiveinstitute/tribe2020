using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

public class PilotView : View{
	//Singleton features
	public static PilotView GetInstance(){
		return _instance as PilotView;
	}

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
	public GameObject mailUI;
	public Transform mailList;
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

	public RectTransform settingsPanel, energyPanel, comfortPanel, inbox, inspector;
	private bool _settingsIsVisible = false;

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

		_curMenu = null;
		//menus = new List<Transform>();
		//menus.Add(settingsPanel);
		//menus.Add(energyPanel);
		//menus.Add(comfortPanel);
		//menus.Add(inbox);

		//Clear inbox
		RemoveChildren(mailList);
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
	public void BuildEEMInterface(Appliance app, List<EnergyEfficiencyMeasure> eems) {
		RemoveChildren(inspectorEEMContainer);

		foreach(EnergyEfficiencyMeasure eem in eems) {
			EnergyEfficiencyMeasure curEEM = eem;
			GameObject buttonObj = Instantiate(EEMButtonPrefab);
			EEMButton eemProps = buttonObj.GetComponent<EEMButton>();
			Button button = buttonObj.GetComponent<Button>();

			if(eem.callback != null) {
				//button.onClick.AddListener(() => _ctrlMgr.OnAction(app, curEEM, buttonObj));
			} else {
				button.onClick.AddListener(() => _ctrlMgr.SendMessage(eem.callback, eem.callbackArgument));
			}

			eemProps.title.text = _localMgr.GetPhrase("EEM." + eem.category + ":" + curEEM.name + "_Title");
			Debug.Log("EEM." + eem.category + ":" + curEEM.name + "_Title");
			buttonObj.GetComponent<Image>().color = eem.color;
			eemProps.SetCost(eem.cashCost, eem.comfortCost);
			eemProps.SetImpact((int)eem.energyFactor, (int)eem.gasFactor, (int)eem.co2Factor, (int)eem.moneyFactor, (int)eem.comfortFactor);

			buttonObj.transform.SetParent(inspectorEEMContainer, false);
		}
	}

	//
	public void BuildInboxInterface() {
	}

	//
	public void SetActions(Appliance appliance, List<BaseAction> actions){
		RemoveChildren(inspectorActionList);
		
		foreach(BaseAction a in actions){
			BaseAction curAction = a;
			GameObject actionObj;
			actionObj = Instantiate(EEMButtonPrefab, Vector3.zero, Quaternion.identity) as GameObject;
			EEMButton button = actionObj.GetComponent<EEMButton>();

			if(curAction.callback == null || curAction.callback.Equals(string.Empty)) {
				actionObj.GetComponent<Button>().
					onClick.AddListener(() => _ctrlMgr.OnAction(appliance, curAction, actionObj));
			} else {
				actionObj.GetComponent<Button>().
					onClick.AddListener(() => _ctrlMgr.SendMessage(a.callback, a.callbackArgument));
			}

			button.title.text = a.actionName;
			button.SetCost(a.cashCost, a.comfortCost);
			button.SetImpact((int)a.energyFactor, (int)a.gasFactor, (int)a.co2Factor, (int)a.moneyFactor, (int)a.comfortFactor);

			//Text[] texts = actionObj.GetComponentsInChildren<Text>();
			//texts[0].text = a.actionName;
			//texts[1].text = "€" + a.cashCost;
			//texts[2].transform.parent.gameObject.SetActive(false);

			//if(a.cashProduction != 0){
			//	texts[3].text = a.cashProduction + "/s";
			//} else {
			//	texts[3].transform.parent.gameObject.SetActive(false);
			//}

			//if(a.comfortPorduction != 0){
			//	texts[4].text = a.comfortPorduction + "/s";
			//} else {
			//	texts[4].transform.parent.gameObject.SetActive(false);
			//}

			//texts[5].transform.parent.gameObject.SetActive(false);

			actionObj.transform.SetParent(inspectorActionList, false);
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
		bool visibility = action == "show";
		switch(id) {
			case "inspector":
				inspectorUI.SetActive(visibility);
				break;
			case "animation":
				animationUI.gameObject.SetActive(visibility);
				break;
			case "playAnimation":
				animationUI.GetComponent<Animation>().Play(action);
				break;
		}

		//Debug.Log("ControlInterface: " + id + ", " + visibility);
	}

	//
	public void ShowAppliance(Appliance appliance) {
		SetCurrentUI(inspector);

		//inspectorUI.SetActive(true);
		inspector.GetComponentsInChildren<Text>()[0].text = _localMgr.GetPhrase("Appliance:" + appliance.title + "_Title");
		inspector.GetComponentsInChildren<Text>()[2].text = _localMgr.GetPhrase("Appliance:" + appliance.title + "_Description");
		//SetActions(appliance, appliance.GetPlayerActions());
		BuildEEMInterface(appliance, appliance.GetEEMs());
	}

	//
	public void HideAppliance() {
		SetCurrentUI(null);
		//inspectorUI.SetActive(false);
	}

	//
	public override void ShowMessage(string message, bool showAtBottom, bool showOkButton = true) {
		messageUI.SetActive(true);
		messageUI.GetComponentInChildren<Text>().text = _localMgr.GetPhrase(message);

		messageUI.transform.GetChild(1).gameObject.SetActive(showOkButton);

		RectTransform messageTrans = messageUI.transform as RectTransform;
		if(showAtBottom) {
			messageTrans.pivot = Vector2.zero;
			messageTrans.anchoredPosition = Vector3.zero;
			messageTrans.anchorMax = Vector2.zero;
		} else {
			messageTrans.pivot = Vector2.up;
			messageTrans.anchoredPosition = Vector3.zero;
			messageTrans.anchorMax = Vector2.up;
		}
	}

	//
	//public void SendMail(string title, string content) {
	//	////Quest curQuest = quest;
	//	//GameObject questObj = Instantiate(mailButtonPrefab) as GameObject;
	//	////questObj.GetComponent<Button>().onClick.AddListener(() => _ctrlMgr.OnQuestPressed(curQuest));
	//	//Text[] texts = questObj.GetComponentsInChildren<Text>();
	//	//texts[0].text = "";
	//	//texts[1].text = curQuest.title;
	//	//texts[2].text = "some date";
	//	//questObj.transform.SetParent(mailList, false);
	//}

	//
	public void ShowQuestList(List<Quest> quests) {
		//
		RemoveChildren(mailList);

		foreach(Quest quest in quests) {
			Quest curQuest = quest;
			GameObject questObj = Instantiate(mailButtonPrefab) as GameObject;
			questObj.GetComponent<Button>().onClick.AddListener(() => _ctrlMgr.OnQuestPressed(curQuest));
			Text[] texts = questObj.GetComponentsInChildren<Text>();
			texts[0].text = "";
			texts[1].text = curQuest.title;
			texts[2].text = "some date";
			questObj.transform.SetParent(mailList, false);
		}

		_curMenu = inbox;
		//mailUI.SetActive(true);
	}

	public void HideQuestList() {
		//mailUI.SetActive(false);
		_curMenu = null;
	}

	//
	public void ShowQuest(Quest quest) {
		Text[] texts = mailReadUI.GetComponentsInChildren<Text>();
		texts[0].text = quest.title;
		//texts[2].text = quest.message;
		texts[3].text = "Objective: Install a Thermal Jug in a coffee machine";

		mailUI.SetActive(false);
		mailReadUI.SetActive(true);
	}

	//
	public override void ShowCongratualations(string text) {
		ShowFireworks();
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
		mailUI.SetActive(true);
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
