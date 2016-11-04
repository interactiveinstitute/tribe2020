using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class PilotController : Controller{
	//Singleton features
	public static PilotController GetInstance(){
		return _instance as PilotController;
	}
	private InputState _curState = InputState.ALL;

	//Access all singleton systems
	private PilotView _view;
	private CameraManager _camMgr;
	private AudioManager _audioMgr;
	private ResourceManager _resourceMgr;
	private NarrationManager _narrationMgr;
	private CustomSceneManager _sceneMgr;
	private SaveManager _saveMgr;

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

	private List<BehaviourAI> _avatars;
	//public GameObject testObj;

	private bool _isLoaded = false;

	//Sort use instead of constructor
	void Awake(){
		_instance = this;
	}

	//
	public void SetToInstance(PilotController oldInstance) {

	}

	// Use this for initialization
	void Start(){
		_view = PilotView.GetInstance();
		_camMgr = CameraManager.GetInstance();
		_audioMgr = AudioManager.GetInstance();
		_resourceMgr = ResourceManager.GetInstance();
		_narrationMgr = NarrationManager.GetInstance();
		_sceneMgr = CustomSceneManager.GetInstance();
		_saveMgr = SaveManager.GetInstance();

		_avatars = new List<BehaviourAI>(Object.FindObjectsOfType<BehaviourAI>());
        _saveMgr.SetAvatars(_avatars);
	}
	
	// Update is called once per frame
	void Update(){
		if(!_isLoaded) {
			_isLoaded = true;
			_saveMgr.Load(SaveManager.currentSlot);
		}

		//Mobile interaction
		//		UpdatePan(_camMgr.camera);
		UpdatePinch();

		//		if(!InspectorUI.activeSelf){
		//if(IsOutsideUI(Input.mousePosition) && !_touchReset) {
		if(!_touchReset) {
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
				_doubleTimer += Time.unscaledDeltaTime;
				if(_doubleTimer > D_TAP_TIMEOUT) {
					OnTap(_startPos);
					_doubleTimer = 0;
					_touchState = IDLE;
				}
			}
		} else {
			_touchReset = false;
		}

		_view.cash.GetComponent<Text>().text = _resourceMgr.cash.ToString();
		_view.comfort.GetComponent<Text>().text = _resourceMgr.comfort.ToString();
		_view.UpdateQuestCount(_narrationMgr.GetQuests().Count);
	}

	//
	private void OnTouchStart(Vector3 pos){
		//Debug.Log("OnTouchStart");
		_touchTimer = 0;
		if(_touchState == IDLE) {
			_startPos = pos;
		}
	}
	
	//
	private void OnTouch(Vector3 pos){
		_camMgr.cameraState = CameraManager.PANNED;
		_touchTimer += Time.unscaledDeltaTime;

		if(Application.platform == RuntimePlatform.Android){
			_camMgr.UpdatePan(Input.GetTouch(0).deltaPosition);
		}
	}
	
	//
	private void OnTouchEnded(Vector3 pos){
		//Debug.Log("ontouchended");
		_camMgr.cameraState = CameraManager.IDLE;
		float dist = Vector3.Distance(_startPos, pos);

		//Touch ended before tap timeout, trigger OnTap
		if(!_touchReset) {
			if(_touchTimer < TAP_TIMEOUT && dist < SWIPE_THRESH) {
				_touchTimer = 0;
				if(_touchState == IDLE) {
					_touchState = TAP;
				} else if(_touchState == TAP) {
					_touchState = IDLE;
					_doubleTimer = 0;
					OnDoubleTap(pos);
				}
			} else if(dist >= SWIPE_THRESH) {
				//Debug.Log("swipe");
				OnSwipe(_startPos, pos);
			}
		}

		
	}
	
	//
	private void OnTap(Vector3 pos){
		_narrationMgr.OnQuestEvent(Quest.QuestEvent.Tapped);
		//if(_curState == InputState.ALL || _curState == InputState.ONLY_TAP) {
		//	Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
		//	RaycastHit hit;
		//}
	}

	//
	private void OnDoubleTap(Vector3 pos){
		//Debug.Log("Double tapped at " + pos);
	}

	//
	private void OnSwipe(Vector3 start, Vector3 end){
		//Debug.Log("cotroller.OnSwipe " + start + " , " + end);
		if(_view.IsAnyOverlayActive()) {
			return;
		}

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

			_narrationMgr.OnQuestEvent(Quest.QuestEvent.Swiped);
			_narrationMgr.OnQuestEvent(Quest.QuestEvent.FindView, _camMgr.GetViewPoint().title);
		}
	}

	//
	public void ResetTouch(){
		_touchTimer = 0;
		_doubleTimer = 0;
		_startPos = Input.mousePosition;
		_touchState = IDLE;
		_touchReset = true;
		//Debug.Log("cotroller.resetSwipe " + _startPos);
	}

	//
	public override void ControlAvatar(string id, string action, Vector3 pos) {
        //Debug.Log("ControlAvatar called in pilotcontroller");
		foreach(BehaviourAI avatar in _avatars) {
			//Debug.Log("ControlAvatar." + avatar.name + " == " + id);
			if(avatar.name == id) {
				avatar.TakeControlOfAvatar();
				avatar.WalkTo(pos);
			}
		}
		//_avatars[0].WalkTo(pos);
	}

	//
	public override void ControlAvatar(string id, Object action) {
		foreach(BehaviourAI avatar in _avatars) {
			if(avatar.name == id) {
				//avatar.TakeControlOfAvatar();
				AvatarActivity newAct = UnityEngine.ScriptableObject.Instantiate<AvatarActivity>(action as AvatarActivity);
				avatar.StartTemporaryActivity(newAct);
			}
		}
	}

	//
	public void MakeAvatarWalkTo(Vector3 target) {
        Debug.Log("MakeAvatarWalkTo called in pilotcontroller");
        _avatars[0].WalkTo(target);
	}

	//
	public void MakeAvatarPerformActivity(AvatarActivity activity) {
		_avatars[0].StartActivity(activity);
	}

	//
	public void OnOkPressed() {
		ResetTouch();

		if(_curState == InputState.ALL || _curState == InputState.ONLY_PROMPT) {
			_view.messageUI.SetActive(false);

			_narrationMgr.OnQuestEvent(Quest.QuestEvent.OKPressed);
		}
	}

	//Open inspector with details of appliance
	public void SetCurrentUI(Appliance appliance) {
		_view.BuildInspector(appliance);
		SetCurrentUI(_view.inspector);

		_narrationMgr.OnQuestEvent(Quest.QuestEvent.ApplianceSelected);
		_narrationMgr.OnQuestEvent(Quest.QuestEvent.ApplianceSelected, appliance.title);
	}

	//Open mail with details of narrative
	public void SetCurrentUI(Quest quest) {
		if(_curState == InputState.ALL || _curState == InputState.ONLY_OPEN_QUEST) { return; }
		_view.BuildMail(quest);
		//SetCurrentUI(_view.mail);

		_narrationMgr.OnQuestEvent(Quest.QuestEvent.QuestOpened);
	}

	//Open user interface
	public void SetCurrentUI(RectTransform ui) {
		if(ui == _view.GetCurrentUI()) {
			HideUI();
		} else {
			_view.SetCurrentUI(ui);
			if(ui == _view.inbox) {
				_view.BuildInbox(_narrationMgr.GetQuests(), _narrationMgr.GetCompletedQuests());
				_narrationMgr.OnQuestEvent(Quest.QuestEvent.QuestListOpened);
			}
		}

		ResetTouch();
	}

	//Hide any open user interface
	public void HideUI() {
		if(_view.GetCurrentUI() == _view.inbox) {
			_narrationMgr.OnQuestEvent(Quest.QuestEvent.ApplianceDeselected);
		}

		_view.SetCurrentUI(null);
	}

	//
	public void OpenMail(Quest quest) {
		if(_curState == InputState.ALL || _curState == InputState.ONLY_OPEN_QUEST) {
			_view.BuildMail(quest);
			ResetTouch();

			_narrationMgr.OnQuestEvent(Quest.QuestEvent.QuestOpened);
		}
	}

	//
	public void OnQuestPressed(Quest quest) {
		if(_curState == InputState.ALL || _curState == InputState.ONLY_OPEN_QUEST) {
			_view.BuildMail(quest);
			//_view.ShowQuest(quest);
			ResetTouch();

			_narrationMgr.OnQuestEvent(Quest.QuestEvent.QuestOpened);
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
			_narrationMgr.OnQuestEvent(Quest.QuestEvent.ResourceHarvested);
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
	public void ApplyEEM(Appliance appliance, EnergyEfficiencyMeasure eem) {
		ResetTouch();
		if(_curState == InputState.ALL) {
			if(_resourceMgr.cash >= eem.cashCost && _resourceMgr.comfort >= eem.comfortCost) {
				_resourceMgr.cash -= eem.cashCost;
				_resourceMgr.comfort -= eem.comfortCost;
				appliance.ApplyEEM(eem);

				_resourceMgr.RefreshProduction();

				//actionObj.SetActive(false);

				_narrationMgr.OnQuestEvent(Quest.QuestEvent.MeasurePerformed, eem.name);
			}
		}
	}

	//
	public void OnAvatarReachedPosition(BehaviourAI avatar, Vector3 pos) {
		//Debug.Log("OnAvatarReachedPosition");
		_narrationMgr.OnQuestEvent(Quest.QuestEvent.AvatarArrived);
		avatar.ReleaseControlOfAvatar();
	}

	//
	public void OnAvatarSessionComplete(string activityState) {
		_narrationMgr.OnQuestEvent(Quest.QuestEvent.AvatarSessionOver, activityState);
	}

	//
	public void OnAvatarActivityComplete(string activity) {
		Debug.Log(name + ": OnAvatarActivityOver " + activity);
		_narrationMgr.OnQuestEvent(Quest.QuestEvent.AvatarActivityOver, activity);
		//avatar.ReleaseControlOfAvatar();
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

		return !_view.IsAnyOverlayActive();
	}

	//
	public override void SetControlState(InputState state) {
		_curState = state;
	}

	//
	public override void SaveGameState() {
		Debug.Log("Game Saved");
		_saveMgr.Save(SaveManager.currentSlot);
	}

	//
	public void LoadScene(string scene) {
		SaveGameState();
		//Debug.Log("LoadScene " + scene);
		_sceneMgr.LoadScene(scene);
	}
}
