using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class PilotController : MonoBehaviour{
	//Singleton features
	private static PilotController _instance;
	public static PilotController GetInstance(){
		if(_instance != null) {
			return _instance;
		}
		return null;
	}

	//Interaction limitation states
	public enum InputState {
		ALL, ONLY_PROMPT, ONLY_SWIPE, ONLY_TAP, ONLY_APPLIANCE_SELECT, ONLY_APPLIANCE_DESELECT,
		ONLY_OPEN_QUEST_LIST, ONLY_OPEN_QUEST
	};
	private InputState _curState = InputState.ALL;

	//Access all singleton systems
	private PilotView _view;
	private CameraManager _camMgr;
	private AudioManager _audioMgr;
	private ResourceManager _resourceMgr;
	private QuestController _questController;

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
		_view = PilotView.GetInstance();
		_camMgr = CameraManager.GetInstance();
		_audioMgr = AudioManager.GetInstance();
		_resourceMgr = ResourceManager.GetInstance();
		_questController = QuestController.GetInstance();
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

		_view.UpdateQuestCount(_questController.GetQuests().Count);
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
		} else if(dist >= SWIPE_THRESH){
			OnSwipe(_startPos, pos);
		}
	}
	
	//
	private void OnTap(Vector3 pos){
		//if(_curState == InputState.ALL || _curState == InputState.ONLY_TAP) {
		//	Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
		//	RaycastHit hit;
		//}
	}

	//
	private void OnDoubleTap(Vector3 pos){
		Debug.Log("Double tapped at " + pos);
	}

	//
	private void OnSwipe(Vector3 start, Vector3 end){
		if(_curState == InputState.ALL || _curState == InputState.ONLY_SWIPE) {
			float dir = Mathf.Atan2(end.y - start.y, end.x - start.x);
			dir = (dir * Mathf.Rad2Deg + 360) % 360;
			float dist = Vector3.Distance(start, end);

			float dirMod = (dir + 90) % 360;
			if(dirMod > 45 && dirMod <= 135) {
				_camMgr.GotoLeftView();
			} else if(dir > 45 && dir <= 135) {
				_camMgr.GotoLowerView();
			} else if(dir > 135 && dir <= 225) {
				_camMgr.GotoRightView();
			} else if(dir > 225 && dir <= 315) {
				_camMgr.GotoUpperView();
			}

			_questController.OnQuestEvent(Quest.QuestEvent.Swiped);
			_questController.OnQuestEvent(Quest.QuestEvent.FindView, _camMgr.GetViewPoint().title);
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
		if(_curState == InputState.ALL || _curState == InputState.ONLY_PROMPT) {
			_view.messageUI.SetActive(false);

			_questController.OnQuestEvent(Quest.QuestEvent.OKPressed);
		}
	}

	//
	public void OnApplianceSelected(Appliance appliance) {
		if(_curState == InputState.ALL || _curState == InputState.ONLY_APPLIANCE_SELECT) {
			_view.ShowAppliance(appliance);
			_audioMgr.PlaySound("button");
			ResetTouch();

			_questController.OnQuestEvent(Quest.QuestEvent.ApplianceSelected);
			_questController.OnQuestEvent(Quest.QuestEvent.ApplianceSelected, appliance.title);
		}
	}

	//
	public void OnApplianceClosed() {
		if(_curState == InputState.ALL || _curState == InputState.ONLY_APPLIANCE_DESELECT) {
			_view.HideAppliance();
			ResetTouch();

			_questController.OnQuestEvent(Quest.QuestEvent.ApplianceDeselected);
		}
	}

	//
	public void OnQuestListOpenend() {
		if(_curState == InputState.ALL || _curState == InputState.ONLY_OPEN_QUEST_LIST) {
			_view.ShowQuestList(_questController.GetQuests());
			ResetTouch();

			_questController.OnQuestEvent(Quest.QuestEvent.QuestListOpened);
		}
	}

	//
	public void OnQuestListClosed() {
		if(_curState == InputState.ALL) {
			_view.HideQuestList();
			ResetTouch();

			_questController.OnQuestEvent(Quest.QuestEvent.QuestListClosed);
		}
	}

	//
	public void OnQuestPressed(Quest quest) {
		if(_curState == InputState.ALL || _curState == InputState.ONLY_OPEN_QUEST) {
			_view.ShowQuest(quest);
			ResetTouch();

			_questController.OnQuestEvent(Quest.QuestEvent.QuestOpened);
		}
	}
	
	//
	public void OnQuestClosed() {
		_view.HideQuest();
		ResetTouch();
	}

	//
	public void OnHarvestTap(GameObject go) {
		if(_curState == InputState.ALL) {
			_resourceMgr.cash += 10;
			_view.CreateFeedback(go.transform.position, "+" + 10 + "€");
			go.SetActive(false);

			ResetTouch();
			_questController.OnQuestEvent(Quest.QuestEvent.ResourceHarvested);
		}
	}

	//
	public void OnPinchIn(){
		if(_curState == InputState.ALL) {
			_camMgr.GotoUpperView();
		}
	}

	//
	public void OnPinchOut(){
		if(_curState == InputState.ALL) {
			_camMgr.GotoLowerView();
		}
	}

	//
	public void OnPinching(float magnitude){
	}

	//
	public void OnAction(Appliance appliance, BaseAction action, GameObject actionObj){
		if(_curState == InputState.ALL) {
			if(_resourceMgr.cash >= action.cashCost && _resourceMgr.comfort >= action.comfortCost) {
				_resourceMgr.cash -= action.cashCost;
				_resourceMgr.comfort -= action.comfortCost;
				appliance.PerformAction(action);

				_resourceMgr.RefreshProduction();

				actionObj.SetActive(false);

				_questController.OnQuestEvent(Quest.QuestEvent.MeasurePerformed, action.actionName);
			}
		}
	}

	//
	public void OnAvatarReachedPosition(BehaviourAI avatar, Vector3 pos) {
		_questController.OnQuestEvent(Quest.QuestEvent.AvatarArrived);
	}

	//
	public void OnAvatarSessionComplete(string activityState) {
		_questController.OnQuestEvent(Quest.QuestEvent.AvatarSessionOver, activityState);
	}

	//
	public void OnAvatarActivityComplete(string activity) {
		_questController.OnQuestEvent(Quest.QuestEvent.AvatarActivityOver, activity);
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
		if(_view.inspectorUI.activeSelf) {
			outsideInspector =
				pos.x > Screen.width * 0.2f ||
				pos.x < Screen.width - Screen.width * 0.2f ||
				pos.y > Screen.height * 0.12f ||
				pos.y < Screen.height - Screen.height * 0.12f;
		}
		outsideInspector = !_view.inspectorUI.activeSelf;
		outsideMailButton =
			pos.x < Screen.width - Screen.width * 0.2f ||
			pos.y < Screen.height - Screen.height * 0.12f;

		return outsideInspector && outsideMailButton && !_view.mailUI.activeSelf;
	}

	//
	public void SetControlState(InputState state) {
		_curState = state;
	}
}
