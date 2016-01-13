using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class InputManager : MonoBehaviour {
	//Singleton features
	private static InputManager _instance;
	public static InputManager GetInstance(){
		return _instance;
	}

	//Useful managers
	private ESManager _simMgr;
	private BuildManager _buildMgr;
	private UIManager _uiMgr;
	private SaveManager _saveMgr;

	public Collider groundPlane;
	public GameObject ground;
	public GameObject okOption, cancelOption;

	private GameObject _marker, _marker2, _selectArea, _outline, _curMarked;
	private Vector3 _prevPosition;

	private BoundsOctree<GameObject> _collisions;

	//Orientation interaction parameters
	public GameObject cameraHolder;
	private Camera _camera;
	public float perspectiveZoomSpeed = 0.25f;
	public float orthoZoomSpeed = 0.25f;

	//Interaction parameters
	private int _curLevel;
	private GameObject _curNode = null;
	private GameObject _curEdge = null;

	//Energy Visualiser
	private EnergyVisualiser _eVis;
	private bool _eVisToggle = true;

	public const string IDLE = "state_idle";
	public const string START = "state_start";
	public const string SPOT = "state_spot";
	public const string CONFIRM = "state_confirm";
	public const string AREA = "state_area";
	public const string LINE = "state_line";
	public const string MARK = "marked_mesh";

	private string _state;

	//Sort use instead of constructor
	void Awake(){
		_instance = this;
	}

	//Start is called once for initialization
	void Start(){
		Debug.Log(Screen.width + ", " + Screen.height);
		_instance = this;

		_marker = Load("UI/Marker");
		_marker2 = Load("UI/Marker2");
		_selectArea = Load("UI/Selection");
		_outline = Load("UI/Outline");
		_prevPosition = Vector3.zero;

		Vector3 center = new Vector3(372.5f, 372.5f, 372.5f);
		_collisions = new BoundsOctree<GameObject>(745, center, 1, 1.25f);
		
		_curLevel = 0;

		ground = GameObject.FindWithTag("ent_ground") as GameObject;
		groundPlane = ground.GetComponent<Collider>();

		_simMgr = GameObject.FindWithTag("managers").GetComponent<ESManager>();
		_buildMgr = BuildManager.GetInstance();
		_uiMgr = UIManager.GetInstance();
		_saveMgr = SaveManager.GetInstance();

		cameraHolder = GameObject.FindWithTag("camera_holder") as GameObject;
		_camera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
		
		//Set inital game state
		SetState (IDLE);
	}

	//Update is called once per frame
	void Update(){
		UpdatePan(_camera);
		UpdatePinch(_camera);

		if(_curNode != null) {
			_uiMgr.selectInfo.text = _curNode.GetComponent<Node>().ToString();
			_curNode.transform.position = _marker.transform.position;
		}

		//Position marker according to grid
		Vector3 pos = PointOnGround(Input.mousePosition, groundPlane);
		pos.x = Mathf.Floor(pos.x / 5);
		pos.y = _curLevel;
		pos.z = Mathf.Floor(pos.z / 5);

		Vector3 newPos = pos * 5;
		newPos += new Vector3(0f, 2.5f, 0f);

		_marker.transform.position = newPos;

//		switch(_state){
//		case IDLE:
//			_marker.transform.position = newPos;
//			break;
//		case SPOT:
//		case CONFIRM:
//			_marker.transform.position = newPos;
//			break;
//		case START:
//			_marker.transform.position = newPos;
//			break;
//		case AREA:
//			if(Input.GetMouseButton(0) && Input.mousePosition.x < Screen.width - 100){
//				_marker2.transform.position = newPos;
//				DrawSelectionArea(_marker.transform.position, _marker2.transform.position);
//			}
//			break;
//		case LINE:
//			if(Input.GetMouseButton(0) && Input.mousePosition.x < Screen.width - 100){
//				Vector3 basePos = _marker.transform.position;
//				if(Mathf.Abs(basePos.x - newPos.x) >
//				   Mathf.Abs(basePos.z - newPos.z)){
//					_marker2.transform.position = new Vector3(newPos.x, basePos.y, basePos.z);
//				} else {
//					_marker2.transform.position = new Vector3(basePos.x, basePos.y, newPos.z);
//				}
//				DrawSelectionArea(basePos, _marker2.transform.position);
//			}
//			break;
//		case MARK:
//			_marker.transform.position = newPos;
//			break;
//		default:
//			break;
//		}

		//
		if(Input.GetMouseButton(0)){
			OnDrag();
		}

		//
		if((newPos.x != _prevPosition.x || newPos.z != _prevPosition.z) && _curNode != null){
			_curNode.GetComponent<Node>().Refresh();
			_prevPosition = newPos;
		}

		//If mouse click, add or delete block depending on if space vacant
		if(Input.GetMouseButtonDown(0)){
			OnTouchStart(_marker.transform.position);
		}

		//
		if(Input.GetMouseButtonUp(0)){
			OnTouchEnded(_marker.transform.position);
		}

		//
		if(Input.GetMouseButtonDown(1) && _curNode != null){
			_buildMgr.UpdateCollision(_curNode);
			_curNode = _buildMgr.AddNode(_curNode.transform.position, _curNode.GetComponent<Node>());
		}

		//Debug info
		_uiMgr.debug1.text = "x: " + Input.mousePosition.x;
		_uiMgr.debugY.text = "y: " + Input.mousePosition.y;
		_uiMgr.debug2.text = "z: " + (Screen.height - 30);
		_uiMgr.debug3.text = _state;
	}

	//
	private void OnTouchStart(Vector3 touch){
		if(!PointerIsOutsideGUI()){
			return;
		}

		int x = (int)touch.x;
		int z = (int)touch.z;
		int y = _curLevel;

		Bounds markerBounds = _marker.GetComponent<Collider>().bounds;
		GameObject[] result = _buildMgr.GetCollidingNode(markerBounds);
		GameObject firstNode = null;
		GameObject firstEdge = null;

		foreach(GameObject go in result){
			if(go.GetComponent<Node>() != null){
				firstNode = go;
			}

			if(go.GetComponent<Edge>() != null){
				firstEdge = go;
			}
		}

		if(firstNode != null){
			_curNode = firstNode;
			Debug.Log("got node");
		} else if(firstEdge != null){
			_curEdge = firstEdge;
			Debug.Log("got edge");
		}

		if(_curNode == null && _curEdge == null){
			_buildMgr.CreateRoom(_marker.transform.position, 3);
		}
	}

	//
	private void OnTouchEnded(Vector3 lastTouch){
		if(!PointerIsOutsideGUI()){
			return;
		}

		//Released node
		if(_curNode != null){
			_buildMgr.UpdateCollision(_curNode);
			Bounds curNodeBounds = _curNode.GetComponent<Collider>().bounds;
			GameObject[] result = _buildMgr.GetCollidingNode(curNodeBounds);
			GameObject secondNode = null;
			GameObject secondEdge = null;

			foreach(GameObject go in result){
				if(go.GetComponent<Node>() != null && go != _curNode){
					secondNode = go;
				}
				
				if(go.GetComponent<Edge>() != null){
					secondEdge = go;
				}
			}

			if(secondNode != null){
				_buildMgr.ConnectNodes(
					secondNode.GetComponent<Node>(), _curNode.GetComponent<Node>());
			} else if(_curNode.GetComponent<Node>().IsNotConnected() && secondEdge != null){
				_buildMgr.SplitEdge(
					secondEdge.GetComponent<Edge>(), lastTouch, _curNode.GetComponent<Node>());
			}

//			foreach(GameObject go in result){
//				if(go.GetComponent<Node>() != null && go != _curNode){
//					_buildMgr.ConnectNodes(go.GetComponent<Node>(), _curNode.GetComponent<Node>());
//					_curEdge = null;
//					_curNode = null;
//					Debug.Log("Node put on other node");
//					return;
//
//				}
//				else if(go.GetComponent<Edge>() != null){
////					_buildMgr.SplitEdge(go.GetComponent<Edge>(), lastTouch, _curNode.GetComponent<Node>());
//					_buildMgr.SplitEdge(go.GetComponent<Edge>(), lastTouch);
//					_curEdge = null;
//					_curNode = null;
//					Debug.Log("Node put on edge");
//					return;
//				}
//			}
//			_curNode = null;

		//Split edge with new node
		} else if(_curEdge != null){
			_buildMgr.SplitEdge(_curEdge.GetComponent<Edge>(), lastTouch);
		}
		
		_curEdge = null;
		_curNode = null;
	}

	//
	private void OnTap(){
	}

	//
	private void OnButtonTap(string button){
	}

	//
	private void OnDoubleTap(){
	}

	//
	private void OnDrag(){
		if(_curNode != null) {
			_curNode.transform.position = _marker.transform.position;
		}
	}

	//
	public void SetState(string newState){
		_state = newState;

		switch(_state){
		case IDLE:
			SetActive(okOption, false);
			SetActive(cancelOption, false);

			SetVisible(_marker2, false);
			SetVisible(_selectArea, false);
			SetVisible(_outline, false);
			break;
		case SPOT:
			SetActive(okOption, false);
			SetActive(cancelOption, true);
			break;
		case CONFIRM:
			SetActive(okOption, true);
			SetActive(cancelOption, true);

			SetVisible(_marker2, true);
			break;
		case START:
			SetActive(okOption, true);
			SetActive(cancelOption, true);
			break;
		case AREA:
			SetActive(okOption, true);
			SetActive(cancelOption, true);

			SetVisible(_marker2, true);
			SetVisible(_selectArea, true);
			break;
		case MARK:
			SetActive(okOption, true);
			SetActive(cancelOption, true);

			SetVisible(_outline, true);
			break;
		default:
			break;
		}
	}

	//Help method for toggling interaction with buttons
	private void SetActive(GameObject go, bool mode){
		go.GetComponent<CanvasGroup>().interactable = mode;
	}

	//Help method for toggling visibility of game objects
	private void SetVisible(GameObject go, bool mode){
		go.GetComponent<Renderer>().enabled = mode;
	}

	//
	public void UpdatePan(Camera camera){
		Vector3 camPos = cameraHolder.transform.position;
		Transform camTransform = cameraHolder.transform;
		
		float tSpeed = 0.1F;
		
		Vector3 tmpPos = ground.transform.position;
		tmpPos.y = _curLevel * 5;
		ground.transform.position = tmpPos;

		#if UNITY_ANDROID
		if ((Input.touchCount == 1 && Input.GetTouch(0).phase == TouchPhase.Moved && _state == IDLE)
		    || (Input.touchCount == 2 && Input.GetTouch(0).phase == TouchPhase.Moved)) {
			// Get movement of the finger since last frame
			Vector2 touchDeltaPosition = Input.GetTouch(0).deltaPosition;
			// Move object across XY plane
			camTransform.Translate(-touchDeltaPosition.x * tSpeed, -touchDeltaPosition.y * tSpeed, 0);
		} else if(Input.touchCount == 1 && Input.GetTouch(0).phase == TouchPhase.Ended){
			
		}
		#endif
		#if UNITY_EDITOR || UNITY_WEBPLAYER
		if(Input.GetKey ("w")){
			camPos.z += 100 * Time.deltaTime;
		} else if(Input.GetKey ("s")){
			camPos.z -= 100 * Time.deltaTime;
		} else if(Input.GetKey ("a")){
			camPos.x -= 100 * Time.deltaTime;
		} else if(Input.GetKey ("d")){
			camPos.x += 100 * Time.deltaTime;
		}
		
		cameraHolder.transform.position = camPos;
		#endif
	}

	//
	public void UpdatePinch(Camera camera){
		if(Input.touchCount == 2){
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
			
			// Zoom differently depending on ortho or perspective
			if (camera.orthographic){
				camera.orthographicSize += deltaMagnitudeDiff * orthoZoomSpeed;
				camera.orthographicSize = Mathf.Max(camera.orthographicSize, 0.1f);
			} else {
				camera.fieldOfView += deltaMagnitudeDiff * perspectiveZoomSpeed;
				camera.fieldOfView = Mathf.Clamp(camera.fieldOfView, 0.1f, 179.9f);
			}
		}
	}

	//OnPress events for the interface buttons
	public void OnSquarePressed(){
		SetState(START);
	}

	//
	public void OnCirclePressed(){
		SetState(START);
	}

	//
	public void OnItemPush(string tag){
		GameObject newObj = _buildMgr.CreateItem(Vector3.zero, tag);
		_curNode = newObj;
	}

	//
	public void OnSavePush(){
		_saveMgr.Save();
	}

	//
	public void OnLoadPush(){
		_saveMgr.Load();
	}

	//
	public void OnOKPressed(){

	}

	//
	public void OnCancelPressed(){
		switch (_state) {
		case AREA:
			SetState(IDLE);
			break;
		case MARK:
			SetState(IDLE);
			break;
		default:
			break;
		}
	}

	//
	public void OnUpPressed(){
		_curLevel++;
	}

	//
	public void OnDownPressed(){
		_curLevel--;
	}

	//
	public void OnCheckboxChanged(bool value){
	}

	//
	private void DrawSelectionArea(Vector3 basePos, Vector3 edgePos){
		Vector3 pos = basePos + (edgePos - basePos) / 2;
		pos.y = _curLevel + 0.2f;
		_selectArea.transform.position = pos;
		Vector3 tmpScale = _selectArea.transform.localScale;
		tmpScale.x = (edgePos.x - basePos.x) / 10 + 0.5f * Mathf.Sign(edgePos.x - basePos.x);
		tmpScale.z = (edgePos.z - basePos.z) / 10 + 0.5f * Mathf.Sign(edgePos.z - basePos.z);
		_selectArea.transform.localScale = tmpScale;
	}
	
	//
	private List<Vector3> StoreSelection(Vector3 startPos, Vector3 endPos){
		int startX = (int)(Mathf.Min(startPos.x, endPos.x));
		int startZ = (int)(Mathf.Min(startPos.z, endPos.z));
		int endX = (int)(Mathf.Max(startPos.x, endPos.x));
		int endZ = (int)(Mathf.Max(startPos.z, endPos.z));
		int width = endX - startX + 1;
		int height = endZ - startZ + 1;
		List<Vector3> storedCells = new List<Vector3> ();
		
		for (int x = 0; x < width; x++) {
			for(int z = 0; z < height; z++){
				storedCells.Add(new Vector3(startX + x, 1, startZ + z));
			}
		}
		return storedCells;
	}
	
	//
	private Vector3 PointOnGround(Vector2 screenCoord, Collider plane){
		Ray ray = Camera.main.ScreenPointToRay(screenCoord);
		RaycastHit hit;
		
		if(plane.Raycast(ray, out hit, 10000.0f)){
			return ray.GetPoint(hit.distance);
		}
		return new Vector3();
	}

	//
	private bool PointerIsOutsideGUI(){
		bool leftOfRightBar = Input.mousePosition.x < Screen.width - Screen.width * 0.1f;
		bool outsideSelectionBox = Input.mousePosition.x > Screen.width * 0.1f ||
			Input.mousePosition.y < Screen.height - Screen.height * 0.12f;

		return leftOfRightBar && outsideSelectionBox;
	}

	//
	public GameObject Load(string path){
		GameObject prefab = Resources.Load(path) as GameObject;
		GameObject go = Instantiate(prefab, Vector3.zero, Quaternion.identity) as GameObject;
		return go;
	}

	//
	public void AddCollision(GameObject obj){
		_collisions.Add(obj, obj.GetComponent<Collider>().bounds);
	}

	//
	public void RemoveCollision(GameObject obj){
		_collisions.Remove(obj);
	}
}
