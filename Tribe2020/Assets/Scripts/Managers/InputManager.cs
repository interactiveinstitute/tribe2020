using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class InputManager : MonoBehaviour {
	private static InputManager _instance;

	public Collider groundPlane;
	public GameObject ground;
	public GameObject okOption, cancelOption;

	private GameObject _marker, _marker2, _selectArea, _outline, _curMarked;
	private Vector3 _prevPosition;
	private ESManager _simMgr;
	private MeshManager _meshMgr;
	private BuildManager _buildMgr;
	private BoundsOctree<GameObject> _collisions;

	private Text _debug1, _debug2, _debug3, _debug4, _debugY;

	public GameObject cameraHolder;
	private Camera _camera;
	public float perspectiveZoomSpeed = 0.25f;
	public float orthoZoomSpeed = 0.25f;

	private ESManager.Block _curBlock;
	private int _curShape = -1;
	private int _curLevel;
	private GameObject _curNode;
	private GameObject _curEdge;

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

	public static InputManager GetInstance(){
		if(_instance != null) {
			return _instance;
		}
		
		return null;
	}

	// Use this for initialization
	void Start(){
		_instance = this;

		_marker = Load("UI/Marker");
		_marker2 = Load("UI/Marker2");
		_selectArea = Load("UI/Selection");
		_outline = Load("UI/Outline");
		_prevPosition = Vector3.zero;
		Vector3 center = new Vector3(372.5f, 372.5f, 372.5f);
		_collisions = new BoundsOctree<GameObject>(745, center, 1, 1.25f);

		_curBlock = ESManager.Block.Floor;
		_curLevel = 0;

		ground = GameObject.FindWithTag("ent_ground") as GameObject;
		groundPlane = ground.GetComponent<Collider>();

		_simMgr = GameObject.FindWithTag("managers").GetComponent<ESManager>();
		_meshMgr = GameObject.FindWithTag("managers").GetComponent<MeshManager>();
		_buildMgr = GameObject.FindWithTag("managers").GetComponent<BuildManager>();

		cameraHolder = GameObject.FindWithTag("camera_holder") as GameObject;
		_camera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
		
//		_eVis = GameObject.Find("Energy Visualiser").GetComponent<EnergyVisualiser>();

		//Debug interface
		_debug1 = GameObject.FindWithTag("debug_1").GetComponent<Text>();
		_debug2 = GameObject.FindWithTag("debug_2").GetComponent<Text>();
		_debug3 = GameObject.FindWithTag("debug_3").GetComponent<Text>();
		_debug4 = GameObject.FindWithTag("debug_4").GetComponent<Text>();
		_debugY = GameObject.FindWithTag("debug_y").GetComponent<Text>();

		//Set inital game state
		SetState (IDLE);

		GameObject LIST_ITEM = Resources.Load("UI/List Item") as GameObject;
		GameObject thingList = GameObject.FindWithTag("thing_list") as GameObject;

		List<string> things = new List<string>();
		things.Add("Toilet");
		things.Add("Campfire");
		things.Add("Coffee");

		List<GameObject> listItems = new List<GameObject>();
		for(int i = 0; i < things.Count; i++){
			string key = things[i];
			listItems.Add(Instantiate(LIST_ITEM, Vector2.zero, Quaternion.identity) as GameObject);
			listItems[i].GetComponentsInChildren<Text>()[0].text = things[i];
			listItems[i].GetComponent<Button>().onClick.AddListener(() => OnItemPush(key));
		}
		AttachList(thingList, listItems);

//		GameObject nObj = GameObject.FindGameObjectWithTag("node") as GameObject;
//		Node node = nObj.GetComponent<Node>();
//		node.AddNode().GetComponent<Node>().AddNode();
		

//		Debug.Log("here other is node:" + node);
	}

	//
	void Update(){
		UpdatePan(_camera);
		UpdatePinch(_camera);

		//Position marker according to grid
		Vector3 pos = PointOnGround(Input.mousePosition, groundPlane);
		pos.x = Mathf.Floor(pos.x / 5);
		pos.y = _curLevel;
		pos.z = Mathf.Floor(pos.z / 5);

		Vector3 newPos = pos * 5;
		newPos += new Vector3(0f, 2.5f, 0f);

		switch(_state){
		case IDLE:
			_marker.transform.position = newPos;
			break;
		case SPOT:
		case CONFIRM:
			_marker.transform.position = newPos;
			break;
		case START:
			_marker.transform.position = newPos;
			break;
		case AREA:
			if(Input.GetMouseButton(0) && Input.mousePosition.x < Screen.width - 100){
				_marker2.transform.position = newPos;
				DrawSelectionArea(_marker.transform.position, _marker2.transform.position);
			}
			break;
		case LINE:
			if(Input.GetMouseButton(0) && Input.mousePosition.x < Screen.width - 100){
				Vector3 basePos = _marker.transform.position;
				if(Mathf.Abs(basePos.x - newPos.x) >
				   Mathf.Abs(basePos.z - newPos.z)){
					_marker2.transform.position = new Vector3(newPos.x, basePos.y, basePos.z);
				} else {
					_marker2.transform.position = new Vector3(basePos.x, basePos.y, newPos.z);
				}
				DrawSelectionArea(basePos, _marker2.transform.position);
			}
			break;
		case MARK:
			_marker.transform.position = newPos;
			break;
		default:
			break;
		}

		//If mouse click, add or delete block depending on if space vacant
		if(Input.GetMouseButtonDown(0)){
			OnTouchStart(_marker.transform.position);
		}

		if(Input.GetMouseButtonUp(0)){
			OnTouchEnded(_marker.transform.position);
		}

//		if(Input.GetMouseButtonUp(0) && _curNode != null){
//			_buildMgr.UpdateCollision(_curNode);
//			Bounds curNodeBounds = _curNode.GetComponent<Collider>().bounds;
//
//			GameObject[] result = _buildMgr.GetCollidingNode(curNodeBounds);
//			foreach(GameObject go in result){
//				if(go != _curNode){
////					Debug.Log("merging node " + go);
//					_buildMgr.ConnectNodes(go.GetComponent<Node>(), _curNode.GetComponent<Node>());
////					_buildMgr.MergeNodes(go.GetComponent<Node>(), _curNode.GetComponent<Node>());
//				}
//			}
////			if(result.Length > 1){
////
////			}
//			_curNode = null;
//		}

		if(Input.GetMouseButtonDown(1) && _curNode != null){
			_buildMgr.UpdateCollision(_curNode);
			_curNode = _buildMgr.AddNode(_curNode.transform.position, _curNode.GetComponent<Node>());
		}

		if(Input.GetMouseButton(0)){
			OnDrag();
		}

		if((newPos.x != _prevPosition.x || newPos.z != _prevPosition.z) && _curNode != null){
//			Debug.Log("marker moved");
			_curNode.GetComponent<Node>().Refresh();
			_prevPosition = newPos;
		}

		_debug1.text = "x: " + (_marker.transform.position.x / 5);
		_debugY.text = "y: " + (_marker.transform.position.y / 5);
		_debug2.text = "z: " + (_marker.transform.position.z / 5);
//		float vertExtent = Camera.main.GetComponent<Camera>().orthographicSize;
//		_debug2.text = "" + vertExtent * Screen.width / Screen.height;
		_debug3.text = _state;
		Vector3 heatCoord =
			new Vector3 (_marker.transform.position.x / 5, 1, _marker.transform.position.z / 5);
//		_debug4.text = "heat: " + _simMgr.GetHeat(heatCoord);
	}

	//
	private void OnTouchStart(Vector3 touch){
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

		if(firstNode != null) {
			_curNode = firstNode;
			Debug.Log("got node");
		} else if(firstEdge != null) {
			_curEdge = firstEdge;
			Debug.Log("got edge");
		} else {
			_buildMgr.CreateRoom(_marker.transform.position, 3);
		}
//			if(result[0].GetComponents<Node>().Length > 0){
//
//			}
//			if(result[0].GetComponents<Edge>().Length > 0){
//				
//			}
//		} else {
//			_buildMgr.CreateRoom(_marker.transform.position);
////			_curNode = _buildMgr.AddNode(_marker.transform.position);
//		}

		if(Input.mousePosition.x < Screen.width - 100){
			_curMarked = _meshMgr.CollidesWithBlock(_marker);

//			switch(_state){
//			case IDLE:
//				_curMarked = _meshMgr.CollidesWithBlock(_marker);
//				if(_curMarked != null){
//					_outline.transform.position = _curMarked.transform.position;
//					_outline.transform.localScale = _curMarked.transform.localScale * 1.1f;
//					SetState(MARK);
//				}
//				break;
//			case START:
//				SetState(AREA);
//				break;
//			case SPOT:
//				SetState(CONFIRM);
//				_marker2.transform.position = _marker.transform.position;
//				break;
//			case AREA:
//				break;
//			case LINE:
//				break;
//			case MARK:
//				_curMarked = _meshMgr.CollidesWithBlock(_marker);
//				if(_curMarked != null){
//					_outline.transform.position = _curMarked.transform.position;
//					_outline.transform.localScale = _curMarked.transform.localScale * 1.1f;
//				} else{
//					SetState(IDLE);
//				}
//				break;
//			default:
//				break;
//			}
		}
	}

	private void OnTouchEnded(Vector3 lastTouch){
		//Released node
		if(_curNode != null){
			_buildMgr.UpdateCollision(_curNode);
			Bounds curNodeBounds = _curNode.GetComponent<Collider>().bounds;
		
			GameObject[] result = _buildMgr.GetCollidingNode(curNodeBounds);
			foreach(GameObject go in result){
				if(go.GetComponent<Node>() != null && go != _curNode){
					_buildMgr.ConnectNodes(go.GetComponent<Node>(), _curNode.GetComponent<Node>());
				}
			}
			_curNode = null;
		} else if(_curEdge != null){
			_buildMgr.SplitEdge(_curEdge.GetComponent<Edge>(), lastTouch);
			_curEdge = null;
			Debug.Log("Let go of edge");
		}
	}

	private void OnDrag(){
		if(_curNode != null) {
			_curNode.transform.position = _marker.transform.position;
		}
	}

	//
	public void SetState(string newState){
		_state = newState;

		switch (_state) {
		case IDLE:
			okOption.GetComponent<CanvasGroup>().interactable = false;
			cancelOption.GetComponent<CanvasGroup>().interactable = false;
			_marker2.GetComponent<Renderer>().enabled = false;
//			_marker2.transform.position = new Vector3(-100, 0, -100);
			_selectArea.GetComponent<Renderer>().enabled = false;
//			_selectArea.transform.position = new Vector3(-100, 0, -100);
//			_selectArea.transform.localScale = new Vector3(1, 1, 1);
			_outline.GetComponent<Renderer>().enabled = false;
			break;
		case SPOT:
			okOption.GetComponent<CanvasGroup>().interactable = false;
			cancelOption.GetComponent<CanvasGroup>().interactable = true;
			break;
		case CONFIRM:
			okOption.GetComponent<CanvasGroup>().interactable = true;
			cancelOption.GetComponent<CanvasGroup>().interactable = true;
			_marker2.GetComponent<Renderer>().enabled = true;
			break;
		case START:
			okOption.GetComponent<CanvasGroup>().interactable = true;
			cancelOption.GetComponent<CanvasGroup>().interactable = true;
			break;
		case AREA:
			okOption.GetComponent<CanvasGroup>().interactable = true;
			cancelOption.GetComponent<CanvasGroup>().interactable = true;
			_marker2.GetComponent<Renderer>().enabled = true;
			_selectArea.GetComponent<Renderer>().enabled = true;
			break;
		case MARK:
			okOption.GetComponent<CanvasGroup>().interactable = true;
			cancelOption.GetComponent<CanvasGroup>().interactable = true;
			_outline.GetComponent<Renderer>().enabled = true;
			break;
		default:
			break;
		}
	}

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
		_curShape = Shape.RECTANGLE;
		_curBlock = ESManager.Block.Wall;
	}

	//
	public void OnCirclePressed(){
		SetState(START);
		_curShape = Shape.CIRCLE;
		_curBlock = ESManager.Block.Wall;
	}

//	//
//	public void OnCampFirePressed(){
//		_curBlock = ESManager.Block.Campfire;
//	}
//
//	//
//	public void OnCoffeePressed(){
//		_curBlock = ESManager.Block.Coffee;
//	}
//
//	//
//	public void OnToiletPressed(){
//		_curBlock = ESManager.Block.Toilet;
//	}

	public void OnItemPush(string tag){
		switch(tag) {
		case "Toilet":
			_curBlock = ESManager.Block.Toilet;
			break;
		case "Campfire":
			_curBlock = ESManager.Block.Campfire;
			break;
		case "Coffee":
			_curBlock = ESManager.Block.Coffee;
			break;
		}

		SetState(SPOT);
	}

	//
	public void OnOKPressed(){
		List<Vector3> storedCells;
		Vector3 start = _marker.transform.position;
		Vector3 end = _marker2.transform.position;
		Vector3 center = start + (end - start) / 2;

		switch(_state){
		case CONFIRM:
			_meshMgr.AddMesh(end, _curBlock);
			SetState(IDLE);
			break;
		case AREA:
			storedCells = StoreSelection(start / 5, end / 5);

			if(_curShape == Shape.RECTANGLE){
				_meshMgr.CreateRoom(start, end);
			} else if(_curShape == Shape.CIRCLE){
//				_meshMgr.CreateCircleRoom(center, Vector3.Distance(start, end) / 2);
			}
			SetState (IDLE);
			break;
		case MARK:
			storedCells = StoreSelection(
				_marker.transform.position / 5, _marker2.transform.position / 5);
//			_simMgr.SetType(storedCells, ESManager.Block.Empty);
			_meshMgr.DestroyMesh(_curMarked);
			SetState (IDLE);
			break;
		default:
			break;
		}
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
//		_eVis.SetFloor(_curLevel * 5f + 0.1f);
	}

	//
	public void OnDownPressed(){
		_curLevel--;
//		_eVis.SetFloor(_curLevel * 5f + 0.1f);
	}

	//
	public void OnCheckboxChanged(bool value){
//		_eVisToggle = !_eVisToggle;
//		_eVis.SetVisible(_eVisToggle);
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
		
		//		Debug.Log ("start: "+startX+","+startZ);
		//		Debug.Log ("end: "+endX+","+endZ);
		//		Debug.Log ("size: "+width+","+height);
		
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

	//Help method for filling a gui list with list elements
	private void AttachList(GameObject list, List<GameObject> elements){
		Transform listTransform = list.transform;
		ClearChildren(listTransform);

		foreach(GameObject element in elements){
			element.transform.SetParent(listTransform, false);
		}
		
//		for(int i = 0; i < elements.Count; i++){
//			elements[i].transform.SetParent(listTransform, false);
//		}
	}

	//Help method for clearing a game object of children
	private void ClearChildren(Transform transform){
		List<GameObject> children = new List<GameObject>();
		foreach (Transform child in transform){
			children.Add (child.gameObject);
		}
		children.ForEach(child => Destroy(child));
	}

	public GameObject Load(string path){
		GameObject prefab = Resources.Load(path) as GameObject;
		GameObject go = Instantiate(prefab, Vector3.zero, Quaternion.identity) as GameObject;
		return go;
	}

	public void AddCollision(GameObject obj){
		_collisions.Add(obj, obj.GetComponent<Collider>().bounds);

//		Debug.Log(_collisions.Count);
	}

	public void RemoveCollision(GameObject obj){
		_collisions.Remove(obj);
	}
}
