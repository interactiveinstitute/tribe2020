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

	private GameObject _marker;
//		, _marker2, _selectArea, _outline, _curMarked;
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
	private float _touchTimer = 0;
	public const float TAP_TIMEOUT = 0.25f;

	//States
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

		//Load UI objects from resources
		_marker = Load("UI/Marker");
//		_marker2 = Load("UI/Marker2");
//		_selectArea = Load("UI/Selection");
//		_outline = Load("UI/Outline");

		//Init interaction properties
		_prevPosition = Vector3.zero;
		_curLevel = 0;

		//Init octree collisions
		Vector3 center = new Vector3(372.5f, 372.5f, 372.5f);
		_collisions = new BoundsOctree<GameObject>(745, center, 1, 1.25f);

		//Get instance of managers
//		_simMgr = GameObject.FindWithTag("managers").GetComponent<ESManager>();
		_buildMgr = BuildManager.GetInstance();
		_uiMgr = UIManager.GetInstance();
		_saveMgr = SaveManager.GetInstance();

		//Ref to ground object
		ground = GameObject.FindWithTag("ent_ground") as GameObject;
		groundPlane = ground.GetComponent<Collider>();

		//Ref to camera
		cameraHolder = GameObject.FindWithTag("camera_holder") as GameObject;
		_camera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
		
		//Set inital game state
//		SetState (IDLE);
	}

	//Update is called once per frame
	void Update(){
		//Mobile interaction
		UpdatePan(_camera);
		UpdatePinch(_camera);

		//Position marker according to grid
		Vector3 cursorPoint = PointOnGround(Input.mousePosition, groundPlane);
		cursorPoint.x = Mathf.Floor(cursorPoint.x / 1) * 1;
		cursorPoint.y = _curLevel + 0.5f;
		cursorPoint.z = Mathf.Floor(cursorPoint.z / 1) * 1;
		_marker.transform.position = cursorPoint;

		//Update position of current node if cursor moved
		if((cursorPoint.x != _prevPosition.x || cursorPoint.z != _prevPosition.z)
		   && _curNode != null){
			_uiMgr.selectInfo.text = _curNode.GetComponent<Node>().ToString();
			_curNode.transform.position = _marker.transform.position;

			_curNode.GetComponent<Node>().Refresh();
			_prevPosition = cursorPoint;
		}

		//Touch start
		if(Input.GetMouseButtonDown(0)){
			OnTouchStart(_marker.transform.position);
		}

		//Touch ongoing
		if(Input.GetMouseButton(0)){
			OnTouch();
		}

		//Touch end
		if(Input.GetMouseButtonUp(0)){
			OnTouchEnded(_marker.transform.position);
		}

		//TODO: old continously add new nodes, remove?
//		if(Input.GetMouseButtonDown(1) && _curNode != null){
//			_buildMgr.UpdateCollision(_curNode);
//			_curNode = _buildMgr.AddNode(_curNode.transform.position, _curNode.GetComponent<Node>());
//		}

		//Debug info
		_uiMgr.debug1.text = "x: " + Input.mousePosition.x;
		_uiMgr.debugY.text = "y: " + Input.mousePosition.y;
		_uiMgr.debug2.text = "z: " + (Screen.height - 30);
		_uiMgr.debug3.text = _state;
	}

	//
	private void OnTouchStart(Vector3 touch){
		_touchTimer = 0;

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

		//Select first node, colliding with cursor
		if(firstNode != null && _curNode == null){
			_curNode = firstNode;
		}
		//No available node, try for edge
		else if(firstEdge != null){
			_curEdge = firstEdge;
		}
	}

	//
	private void OnTouch(){
		_touchTimer += Time.deltaTime;
	}

	//
	private void OnTouchEnded(Vector3 lastTouch){
		//Touch ended before tap timeout, trigger OnTap
		if(_touchTimer < TAP_TIMEOUT){
			OnTap(lastTouch);
		}

		//Stop if inside GUI
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

			//Released node on other room's node, connect
			if(secondNode != null){
				_buildMgr.ConnectNodes(
					secondNode.GetComponent<Node>(), _curNode.GetComponent<Node>());
			} 
			//Release thing node on edge, split edge around node thing
			else if(_curNode.GetComponent<Node>().IsNotConnected() && secondEdge != null){
				_buildMgr.SplitEdge(
					secondEdge.GetComponent<Edge>(), lastTouch, _curNode.GetComponent<Node>());
			}
		}
		
		_curEdge = null;
		_curNode = null;
	}

	//
	private void OnTap(Vector3 touchPos){
		//Check that tap happend outside GUI
		if(PointerIsOutsideGUI()){
			//Tapping on ground, create new room
			if(_curNode == null && _curEdge == null){
				_buildMgr.CreateRoom(_marker.transform.position, 3);
			}

			//Tapped on edge, split edge with new node
			if(_curNode == null && _curEdge != null){
				_buildMgr.SplitEdge(_curEdge.GetComponent<Edge>(), touchPos);
			}

			//Release thing node on edge, split edge around node thing
			if(_curNode != null && _curEdge != null &&
			   _curNode.GetComponent<Node>().IsNotConnected()){
				_buildMgr.SplitEdge(
					_curEdge.GetComponent<Edge>(), touchPos, _curNode.GetComponent<Node>());
			}
			
			_curEdge = null;
			_curNode = null;
		}
	}

	//
	private void OnButtonTap(string button){
	}

	//
	private void OnDoubleTap(){
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
		tmpPos.y = _curLevel;
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

	//
	public void OnItemPush(string tag){
		GameObject newObj = _buildMgr.AddThing(Vector3.zero, tag);
		_curNode = newObj;
	}

	//
	public void OnSavePush(){
		_saveMgr.Save(_uiMgr.GetFileName());
	}

	//
	public void OnLoadPush(){
		_saveMgr.Load(_uiMgr.GetFileName());
	}

	//
	public void OnUpPressed(){
		_curLevel++;
		_buildMgr.UpdateViewLevel(_curLevel);
	}

	//
	public void OnDownPressed(){
		_curLevel--;
		_buildMgr.UpdateViewLevel(_curLevel);
	}

	//
//	private void DrawSelectionArea(Vector3 basePos, Vector3 edgePos){
//		Vector3 pos = basePos + (edgePos - basePos) / 2;
//		pos.y = _curLevel + 0.2f;
//		_selectArea.transform.position = pos;
//		Vector3 tmpScale = _selectArea.transform.localScale;
//		tmpScale.x = (edgePos.x - basePos.x) / 10 + 0.5f * Mathf.Sign(edgePos.x - basePos.x);
//		tmpScale.z = (edgePos.z - basePos.z) / 10 + 0.5f * Mathf.Sign(edgePos.z - basePos.z);
//		_selectArea.transform.localScale = tmpScale;
//	}
	
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

	//Help method to check whether touch is happening outside overlay interface
	private bool PointerIsOutsideGUI(){
		bool leftOfRightBar = Input.mousePosition.x < Screen.width - Screen.width * 0.1f;
		bool outsideSelectionBox = Input.mousePosition.x > Screen.width * 0.1f ||
			Input.mousePosition.y < Screen.height - Screen.height * 0.12f;

		return leftOfRightBar && outsideSelectionBox;
	}

	//Help method for instantiating object from resources
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
