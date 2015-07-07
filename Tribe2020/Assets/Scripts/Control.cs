using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class Control : MonoBehaviour {
	public Collider groundPlane;
	public GameObject ground;

	private GameObject _marker, _marker2, _selectArea;
	public GridManager gridMgr;

	private Text _debug1, _debug2;

	public GameObject cameraHolder;

	private GridManager.Block _curBlock;
	private int _curLevel;

	public const string IDLE = "state_idle";
	public const string SPOT = "state_spot";
	public const string AREA = "state_area";
	public const string LINE = "state_line";

	private string _state;
//	= IDLE;

	// Use this for initialization
	void Start(){
		_marker = GameObject.FindWithTag("ui_pointer") as GameObject;
		_marker2 = GameObject.FindWithTag("ui_pointer2") as GameObject;
		_selectArea = GameObject.FindWithTag("ui_selection") as GameObject;
//		_selectedCells = new List<Vector3> ();

		_curBlock = GridManager.Block.Floor;
		_curLevel = 1;

		ground = (GameObject)GameObject.Find("ent_ground");
		groundPlane = ground.GetComponent<Collider>();

		gridMgr = GameObject.Find("mgr_grid").GetComponent<GridManager>();

		cameraHolder = GameObject.Find("camera_holder");

		_debug1 = GameObject.FindWithTag ("debug_1").GetComponent<Text> ();
		_debug2 = GameObject.FindWithTag ("debug_2").GetComponent<Text> ();

		SetState (IDLE);
	}

	void Update(){
//		Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
//		RaycastHit hit;

		Vector3 camPos = cameraHolder.transform.position;
		Transform camTransform = cameraHolder.transform;
//		float speed = 100;
		float tSpeed = 0.1F;

		Vector3 tmpPos = ground.transform.position;
		tmpPos.y = _curLevel * 5;
		ground.transform.position = tmpPos;

#if UNITY_ANDROID
		if (Input.touchCount == 1 && Input.GetTouch(0).phase == TouchPhase.Moved) {
			// Get movement of the finger since last frame
			Vector2 touchDeltaPosition = Input.GetTouch(0).deltaPosition;
			// Move object across XY plane
			camTransform.Translate(-touchDeltaPosition.x * tSpeed, -touchDeltaPosition.y * tSpeed, 0);
		} else if(Input.touchCount == 1 && Input.GetTouch(0).phase == TouchPhase.Ended){

		}
#endif
#if UNITY_EDITOR || UNITY_WEBPLAYER
//		if(Input.mousePosition.x > Screen.width - 100){
//			camPos.x += 100 * Time.deltaTime;
//		}else if (Input.mousePosition.x < 100){
//			camPos.x -= 100 * Time.deltaTime;
//		}
//		
//		if(Input.mousePosition.y > Screen.height - 10d0){
//			camPos.z += 100 * Time.deltaTime;
//		}else if (Input.mousePosition.y < 100){
//			camPos.z -= 100 * Time.deltaTime;
//		}

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

		//Position marker according to grid
		Vector3 pos = PointOnGround(Input.mousePosition, groundPlane);
		pos.x = Mathf.Floor (pos.x / 5);
		pos.y = _curLevel;
		pos.z = Mathf.Floor (pos.z / 5);

		Vector3 newPos = pos * 5;

		switch(_state){
		case IDLE:
			_marker.transform.position = newPos;
			break;
		case AREA:
			if (Input.GetMouseButton(0) && Input.mousePosition.x < Screen.width - 100) {
				_marker2.transform.position = newPos;
				DrawSelectionArea(_marker.transform.position, _marker2.transform.position);
			}
			break;
		case LINE:
			if (Input.GetMouseButton(0) && Input.mousePosition.x < Screen.width - 100) {
				Vector3 basePos = _marker.transform.position;
				if(Mathf.Abs(basePos.x - newPos.x) >
				   Mathf.Abs(basePos.z - newPos.z)){
					_marker2.transform.position = new Vector3(newPos.x, basePos.y, basePos.z);
				} else{
					_marker2.transform.position = new Vector3(basePos.x, basePos.y, newPos.z);
				}
				DrawSelectionArea(basePos, _marker2.transform.position);
			}
			break;
		default:
			break;
		}

		//If mouse click, add or delete block depending on if space vacant
		if(Input.GetMouseButtonDown(0)){
			OnClick((int)pos.x, _curLevel, (int)pos.z);
		}

		_debug1.text = "x: "+(_marker.transform.position.x / 5);
		_debug2.text = "z: "+(_marker.transform.position.z / 5);
	}

	private void OnClick(int x, int y, int z){
		if (Input.mousePosition.x < Screen.width - 100) {
//			if (gridMgr.GetType (x, y, z) == GridManager.Block.Empty) {
//				gridMgr.SetType (x, y, z, GridManager.Block.Floor);
//			} else {
//				gridMgr.SetType (x, y, z, GridManager.Block.Empty);
//			}

			switch(_state){
			case IDLE:
				SetState(AREA);
//				_state = AREA;
//				_marker2.transform.position = _marker.transform.position;
				break;
			case SPOT:
				break;
			case AREA:
				break;
			case LINE:
				break;
			default:
				break;
			}
//			Debug.Log ("OnClick: " + _state);
		}


	}

//	private void SetMarker(float x, float y, float z){
//		_marker.transform.position = new Vector3 (x, y, z);
//		_state = "marker_set";
//	}

	private void DrawSelectionArea(Vector3 basePos, Vector3 edgePos){
		_selectArea.transform.position = basePos + (edgePos - basePos) / 2;
		Vector3 tmpScale = _selectArea.transform.localScale;
		tmpScale.x = (edgePos.x - basePos.x) / 10 + 0.5f * Mathf.Sign(edgePos.x - basePos.x);
		tmpScale.z = (edgePos.z - basePos.z) / 10 + 0.5f * Mathf.Sign(edgePos.z - basePos.z);
		_selectArea.transform.localScale = tmpScale;
	}

	private List<Vector3> StoreSelection(Vector3 startPos, Vector3 endPos){
		int startX = (int)(Mathf.Min(startPos.x, endPos.x));
		int startZ = (int)(Mathf.Min(startPos.z, endPos.z));
		int endX = (int)(Mathf.Max(startPos.x, endPos.x));
		int endZ = (int)(Mathf.Max(startPos.z, endPos.z));
		int width = endX - startX + 1;
		int height = endZ - startZ + 1;
		List<Vector3> storedCells = new List<Vector3> ();

		Debug.Log ("start: "+startX+","+startZ);
		Debug.Log ("end: "+endX+","+endZ);
		Debug.Log ("size: "+width+","+height);

		for (int x = 0; x < width; x++) {
			for(int z = 0; z < height; z++){
				storedCells.Add(new Vector3(startX + x, 1, startZ + z));
			}
		}
		return storedCells;
	}

	private Vector3 PointOnGround(Vector2 screenCoord, Collider plane){
		Ray ray = Camera.main.ScreenPointToRay(screenCoord);
		RaycastHit hit;

		if(plane.Raycast(ray, out hit, 10000.0f)){
			return ray.GetPoint(hit.distance);
		}
		return new Vector3();
	}

	public void SetState(string newState){
		_state = newState;

		switch (_state) {
		case IDLE:
			_marker2.GetComponent<Renderer>().enabled = false;
			_marker2.transform.position = new Vector3(-100, 0, -100);
			_selectArea.GetComponent<Renderer>().enabled = false;
			_selectArea.transform.position = new Vector3(-100, 0, -100);
			_selectArea.transform.localScale = new Vector3(1, 1, 1);
			break;
		case AREA:
			_marker2.GetComponent<Renderer>().enabled = true;
			_selectArea.GetComponent<Renderer>().enabled = true;
			break;
		default:
			break;
		}
	}
	
	public void OnFloorPressed(){
		_curBlock = GridManager.Block.Floor;
	}

	public void OnWallPressed(){
		_curBlock = GridManager.Block.Floor;
	}

	public void OnCampFirePressed(){
		_curBlock = GridManager.Block.Campfire;
	}

	public void OnOKPressed(){
		switch (_state) {
		case AREA:
			List<Vector3> storedCells = StoreSelection(_marker.transform.position / 5, _marker2.transform.position / 5);
			gridMgr.SetType(storedCells, _curBlock);
			SetState (IDLE);
//			_state = IDLE;
			break;
		default:
			break;
		}
	}

	public void OnCancelPressed(){
		switch (_state) {
		case AREA:
			SetState(IDLE);
//			_state = IDLE;
			break;
		default:
			break;
		}
	}

	public void OnUpPressed(){
		_curLevel++;
	}

	public void OnDownPressed(){
		_curLevel--;
	}
}
