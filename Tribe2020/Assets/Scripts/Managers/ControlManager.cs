using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class ControlManager : MonoBehaviour{
	//Singleton features
	private static ControlManager _instance;
	public static ControlManager GetInstance(){
		return _instance;
	}

	public GameObject TitleUI;
	public GameObject InspectorUI;
	public GameObject MailUI;

	private CameraManager _camMgr;
	private ViewManager _viewMgr;

	private AudioManager _audioMgr;
	private ResourceManager _resourceMgr;
	private TutorialManager _tutMgr;
	private QuestManager _questMgr;

	//Interaction props
	private string _touchState = IDLE;
	private float _touchTimer = 0;
	private float _doubleTimer = 0;
	private Vector3 _startPos;
	private bool _isPinching = false;
	private bool _touchReset = false;

	//Interaction consts
	private const string IDLE = "idle";
	private const string TAP = "tap";
	public const float TAP_TIMEOUT = 0.1f;
	public const float D_TAP_TIMEOUT = 0.2f;
	public const float SWIPE_THRESH = 50;

	//Sort use instead of constructor
	void Awake(){
		_instance = this;
	}

	// Use this for initialization
	void Start(){
		_camMgr = CameraManager.GetInstance();
		_viewMgr = ViewManager.GetInstance();
		_audioMgr = AudioManager.GetInstance();
		_resourceMgr = ResourceManager.GetInstance();
		_tutMgr = TutorialManager.GetInstance();
		_questMgr = QuestManager.GetInstance();

		//InspectorUI.SetActive(true);
	}
	
	// Update is called once per frame
	void Update(){
		//Mobile interaction
//		UpdatePan(_camMgr.camera);
		UpdatePinch();

		//		if(!InspectorUI.activeSelf){
		if(IsOutsideUI(Input.mousePosition) && !_touchReset) {
			//Touch start
			if(Input.GetMouseButtonDown(0)) {
				OnTouchStart(Input.mousePosition);
			}

			//Touch ongoing
			if(Input.GetMouseButton(0)) {
				OnTouch(Input.mousePosition);
			}

			//Touch end
			if(Input.GetMouseButtonUp(0)) {
				OnTouchEnded(Input.mousePosition);
			}

			if(_touchState == TAP) {
				_doubleTimer += Time.deltaTime;
				if(_doubleTimer > D_TAP_TIMEOUT) {
					OnTap(_startPos);
					_doubleTimer = 0;
					_touchState = IDLE;
				}
			}
		} else {
			_touchReset = false;
		}

		_viewMgr.UpdateQuestCount(_questMgr.GetQuests().Count);
	}

	//
	private void OnTouchStart(Vector3 pos){
		_touchTimer = 0;
		if(_touchState == IDLE) {
			_startPos = pos;
		}
	}
	
	//
	private void OnTouch(Vector3 pos){
		_camMgr.cameraState = CameraManager.PANNED;
		_touchTimer += Time.deltaTime;

		if(Application.platform == RuntimePlatform.Android){
			_camMgr.UpdatePan(Input.GetTouch(0).deltaPosition);
		}
	}
	
	//
	private void OnTouchEnded(Vector3 pos){
		_camMgr.cameraState = CameraManager.IDLE;
		float dist = Vector3.Distance(_startPos, pos);

		//Touch ended before tap timeout, trigger OnTap
		if(_touchTimer < TAP_TIMEOUT && dist < SWIPE_THRESH){
			_touchTimer = 0;
			if(_touchState == IDLE){
				_touchState = TAP;
			} else if(_touchState == TAP){
				_touchState = IDLE;
				_doubleTimer = 0;
				OnDoubleTap(pos);
			}
//			OnTap(pos);
		} else if(dist >= SWIPE_THRESH){
			OnSwipe(_startPos, pos);
		}
	}
	
	//
	private void OnTap(Vector3 pos){
		Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
		RaycastHit hit;
	}

	//
	private void OnDoubleTap(Vector3 pos){
		Debug.Log("Double tapped at " + pos);
	}

	//
	private void OnSwipe(Vector3 start, Vector3 end){
		float dir = Mathf.Atan2(end.y - start.y, end.x - start.x);
		dir = (dir * Mathf.Rad2Deg + 360) % 360;
		float dist = Vector3.Distance(start, end);

		float dirMod = (dir + 90) % 360;
		if(dirMod > 45 && dirMod <= 135){
			_camMgr.GotoLeftView();
		} else if(dir > 45 && dir <= 135){
			_camMgr.GotoLowerView();
		} else if(dir > 135 && dir <= 225){
			_camMgr.GotoRightView();
		} else if(dir > 225 && dir <= 315){
			_camMgr.GotoUpperView();
		}
	}

	//
	public void ResetTouch(){
		_touchTimer = 0;
		_doubleTimer = 0;
		_startPos = Input.mousePosition;
		_touchState = IDLE;
		_touchReset = true;
	}

	//
	public void OnOkPressed() {
		_viewMgr.messageUI.SetActive(false);
		_tutMgr.NextStep();
	}

	//
	public void OnApplianceSelected(Appliance appliance) {
		_viewMgr.ShowAppliance(appliance);
		_audioMgr.PlaySound("button");
		ResetTouch();
	}

	//
	public void OnApplianceClosed() {
		_viewMgr.HideAppliance();
		ResetTouch();
	}

	//
	public void OnQuestListOpenend() {
		_viewMgr.ShowQuestList(_questMgr.GetQuests());
		ResetTouch();
	}

	//
	public void OnQuestListClosed() {
		_viewMgr.HideQuestList();
		ResetTouch();
	}

	//
	public void OnQuestPressed(Quest quest) {
		_viewMgr.ShowQuest(quest);
		ResetTouch();
	}
	
	//
	public void OnQuestClosed() {
		_viewMgr.HideQuest();
		ResetTouch();
	}

	//
	public void OnHarvestTap(GameObject go) {
		_resourceMgr.cash += 10;
		_viewMgr.CreateFeedback(go.transform.position, "+" + 10 + "€");
		go.SetActive(false);

		ResetTouch();
	}

	//
	public void OnPinchIn(){
		_camMgr.GotoUpperView();
	}

	//
	public void OnPinchOut(){
		_camMgr.GotoLowerView();
	}

	//
	public void OnPinching(float magnitude){
	}

	//
	public void OnAction(Appliance appliance, BaseAction action, GameObject actionObj){
		Debug.Log(appliance.name + ": " + action.cashCost + ", " + action.comfortCost);

		if(_resourceMgr.cash >= action.cashCost && _resourceMgr.comfort >= action.comfortCost) {
			_resourceMgr.cash -= action.cashCost;
			_resourceMgr.comfort -= action.comfortCost;
			appliance.PerformAction(action);
			//action.performed = true;
	
			//_uiMgr.CreateFeedback(action.gameObject.transform.position, "-" + action.cashCost);
			_resourceMgr.RefreshProduction();

			actionObj.SetActive(false);
		}
	}
	
	//
	public void UpdatePinch(){
		if(Input.touchCount == 2){
			_isPinching = true;

			// Store both touches.
			Touch touchZero = Input.GetTouch(0);
			Touch touchOne = Input.GetTouch(1);
			
			// Find the position in the previous frame of each touch.
			Vector2 touchZeroPrevPos = touchZero.position - touchZero.deltaPosition;
			Vector2 touchOnePrevPos = touchOne.position - touchOne.deltaPosition;
			
			// Find the magnitude of the distance between the touches in each frame.
			float prevTouchDeltaMag = (touchZeroPrevPos - touchOnePrevPos).magnitude;
			float touchDeltaMag = (touchZero.position - touchOne.position).magnitude;
			
			// Find the difference in the distances between each frame.
			float deltaMagnitudeDiff = prevTouchDeltaMag - touchDeltaMag;

			OnPinching(deltaMagnitudeDiff);
		} else if(_isPinching){
			_isPinching = false;

			// Store both touches.
			Touch touchZero = Input.GetTouch(0);
			Touch touchOne = Input.GetTouch(1);

			// Find the position in the previous frame of each touch.
			Vector2 touchZeroPrevPos = touchZero.position - touchZero.deltaPosition;
			Vector2 touchOnePrevPos = touchOne.position - touchOne.deltaPosition;
			
			// Find the magnitude of the distance between the touches in each frame.
			float prevTouchDeltaMag = (touchZeroPrevPos - touchOnePrevPos).magnitude;
			float touchDeltaMag = (touchZero.position - touchOne.position).magnitude;
			
			// Find the difference in the distances between each frame.
			float deltaMagnitudeDiff = prevTouchDeltaMag - touchDeltaMag;
			if(deltaMagnitudeDiff > 0){
				OnPinchOut();
			} else {
				OnPinchIn();
			}
		}
	}

	//
	public bool IsOutsideUI(Vector3 pos){
		bool outsideInspector = true;
		bool outsideMailButton = true;
		if(InspectorUI.activeSelf) {
			outsideInspector =
				pos.x > Screen.width * 0.2f ||
				pos.x < Screen.width - Screen.width * 0.2f ||
				pos.y > Screen.height * 0.12f ||
				pos.y < Screen.height - Screen.height * 0.12f;
		}
		outsideInspector = !InspectorUI.activeSelf;
		outsideMailButton =
			pos.x < Screen.width - Screen.width * 0.2f ||
			pos.y < Screen.height - Screen.height * 0.12f;

		//Debug.Log("pointer:" + pos);

		return outsideInspector && outsideMailButton && !MailUI.activeSelf;
	}
}
