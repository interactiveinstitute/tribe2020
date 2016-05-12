using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

public class PilotView : MonoBehaviour{
	//Singleton features
	private static PilotView _instance;
	public static PilotView GetInstance(){
		return _instance;
	}

	private PilotController _ctrlMgr;

	public Transform title;
	public Transform date;

	public Transform cash;
	public Transform comfort;
	public Transform temperature;
	public Transform power;
	public Transform co2;

	public Transform inspectorAction;
	public GameObject actionButtonPrefab;

	public GameObject inspectorUI;
	public GameObject mailUI;
	public Transform questList;
	public GameObject mailReadUI;
	public GameObject messageUI;
	public GameObject tutorialUI;

	public GameObject congratsPanel;
	public Text congratsText;

	public Text questCountText;

	public Transform viewpointGuide;
	public GameObject viewpointIconPrefab;
	public GameObject mailButtonPrefab;

	public GameObject FeedbackNumber;
	public ParticleSystem fireworks;

	//Sort use instead of constructor
	void Awake(){
		_instance = this;
	}

	//Use this for initialization
	void Start(){
		_ctrlMgr = PilotController.GetInstance();
	}
	
	//Update is called once per frame
	void Update(){
	}

	//
	public void SetActions(Appliance appliance, List<BaseAction> actions){
		RemoveChildren(inspectorAction);
		
		foreach(BaseAction a in actions){
			BaseAction curAction = a;
			GameObject actionObj;
			actionObj = Instantiate(actionButtonPrefab, Vector3.zero, Quaternion.identity) as GameObject;
			if(curAction.callback == null || curAction.callback.Equals(string.Empty)) {
				actionObj.GetComponent<Button>().
					onClick.AddListener(() => _ctrlMgr.OnAction(appliance, curAction, actionObj));
			} else {
				actionObj.GetComponent<Button>().
					onClick.AddListener(() => _ctrlMgr.SendMessage(a.callback, a.callbackArgument));
			}

			Text[] texts = actionObj.GetComponentsInChildren<Text>();
			texts[0].text = a.actionName;
			texts[1].text = "€" + a.cashCost;
			texts[2].transform.parent.gameObject.SetActive(false);

			if(a.cashProduction != 0){
				texts[3].text = a.cashProduction + "/s";
			} else {
				texts[3].transform.parent.gameObject.SetActive(false);
			}

			if(a.comfortPorduction != 0){
				texts[4].text = a.comfortPorduction + "/s";
			} else {
				texts[4].transform.parent.gameObject.SetActive(false);
			}

			texts[5].transform.parent.gameObject.SetActive(false);

			actionObj.transform.SetParent(inspectorAction, false);
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
		if(viewCount != viewpointGuide.childCount) {
			RemoveChildren(viewpointGuide);
			for(int i = 0; i < viewCount; i++) {
				GameObject iconObj = Instantiate(viewpointIconPrefab) as GameObject;
				iconObj.transform.SetParent(viewpointGuide, false);

				if(i == viewIndex) {
					iconObj.GetComponent<Image>().color = Color.blue;
				}
			}
		} else {
			for(int i = 0; i < viewCount; i++) {
				Transform curIcon = viewpointGuide.GetChild(i);
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
		questCountText.text = "" + questCount;
	}

	//
	public void ShowAppliance(Appliance appliance) {
		inspectorUI.SetActive(true);
		inspectorUI.GetComponentsInChildren<Text>()[0].text = appliance.title;
		inspectorUI.GetComponentsInChildren<Text>()[2].text = appliance.description;
		SetActions(appliance, appliance.GetPlayerActions());
	}

	//
	public void HideAppliance() {
		inspectorUI.SetActive(false);
	}

	//
	public void ShowMessage(string message, bool showAtBottom, bool showOkButton = true) {
		messageUI.SetActive(true);
		messageUI.GetComponentInChildren<Text>().text = message;

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
	public void ShowQuestList(List<Quest> quests) {
		//
		RemoveChildren(questList);

		foreach(Quest quest in quests) {
			Quest curQuest = quest;
			GameObject questObj = Instantiate(mailButtonPrefab) as GameObject;
			questObj.GetComponent<Button>().onClick.AddListener(() => _ctrlMgr.OnQuestPressed(curQuest));
			Text[] texts = questObj.GetComponentsInChildren<Text>();
			texts[0].text = "";
			texts[1].text = curQuest.title;
			texts[2].text = "some date";
			questObj.transform.SetParent(questList, false);
		}

		mailUI.SetActive(true);
	}

	public void HideQuestList() {
		mailUI.SetActive(false);
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
	public void ShowCongratualations(string text) {
		ShowFireworks();
		congratsPanel.SetActive(true);
		congratsText.text = text;
	}

	//
	public void ShowFireworks() {
		fireworks.Play();
	}

	//
	public void HideQuest() {
		
		mailReadUI.SetActive(false);
		mailUI.SetActive(true);
	}

	//
	public void AddQuest() {

	}

	//
	public void RemoveQuest() {
	}
}
