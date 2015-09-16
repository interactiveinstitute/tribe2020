using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class InputManager : MonoBehaviour {
	public Collider groundPlane;
	public GameObject ground;

	private GameObject _marker, _marker2, _selectArea, _outline, _curMarked;
	private SimulationManager _simMgr;
	private MeshManager _meshMgr;

	private Text _debug1, _debug2, _debug3, _debug4, _debugY;

	public GameObject cameraHolder;

	private SimulationManager.Block _curBlock;
	private int _curLevel;

	//Energy Visualiser
	private EnergyVisualiser _eVis;
	private bool _eVisToggle = true;

	public const string IDLE = "state_idle";
	public const string SPOT = "state_spot";
	public const string AREA = "state_area";
	public const string LINE = "state_line";
	public const string MARK = "marked_mesh";

	private string _state;

	// Use this for initialization
	void Start(){
		_marker = GameObject.FindWithTag("ui_pointer") as GameObject;
		_marker2 = GameObject.FindWithTag("ui_pointer2") as GameObject;
		_selectArea = GameObject.FindWithTag("ui_selection") as GameObject;
		_outline = GameObject.FindWithTag("outline") as GameObject;
//		_selectedCells = new List<Vector3> ();

		_curBlock = SimulationManager.Block.Floor;
		_curLevel = 0;

		ground = GameObject.FindWithTag("ent_ground") as GameObject;
		groundPlane = ground.GetComponent<Collider>();

		_simMgr = GameObject.FindWithTag("managers").GetComponent<SimulationManager>();
		_meshMgr = GameObject.FindWithTag("managers").GetComponent<MeshManager>();

		cameraHolder = GameObject.FindWithTag("camera_holder") as GameObject;

//		_eVis = GameObject.Find("Energy Visualiser") as GameObject;
		_eVis = GameObject.Find("Energy Visualiser").GetComponent<EnergyVisualiser>();

		//Debug interface
		_debug1 = GameObject.FindWithTag ("debug_1").GetComponent<Text> ();
		_debug2 = GameObject.FindWithTag ("debug_2").GetComponent<Text> ();
		_debug3 = GameObject.FindWithTag ("debug_3").GetComponent<Text> ();
		_debug4 = GameObject.FindWithTag ("debug_4").GetComponent<Text> ();
		_debugY = GameObject.FindWithTag ("debug_y").GetComponent<Text> ();

		//Set inital game state
		SetState (IDLE);
	}

	void Update(){
//		Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
//		RaycastHit hit;

		Vector3 camPos = cameraHolder.transform.position;
		Transform camTransform = cameraHolder.transform;

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
		newPos += new Vector3(0f, 2.5f, 0f);

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
		case MARK:
			_marker.transform.position = newPos;
			break;
		default:
			break;
		}

		//If mouse click, add or delete block depending on if space vacant
		if(Input.GetMouseButtonDown(0)){
			OnClick((int)pos.x, _curLevel, (int)pos.z);
		}

		_debug1.text = "x: "+(_marker.transform.position.x / 5);
		_debugY.text = "y: "+(_marker.transform.position.y / 5);
		_debug2.text = "z: "+(_marker.transform.position.z / 5);
//		float vertExtent = Camera.main.GetComponent<Camera>().orthographicSize;
//		_debug2.text = "" + vertExtent * Screen.width / Screen.height;
		_debug3.text = _state;
		Vector3 heatCoord =
			new Vector3 (_marker.transform.position.x / 5, 1, _marker.transform.position.z / 5);
		_debug4.text = "heat: " + _simMgr.GetHeat(heatCoord);
	}

	private void OnClick(int x, int y, int z){
		if (Input.mousePosition.x < Screen.width - 100) {
			switch(_state){
			case IDLE:
				_curMarked = _meshMgr.CollidesWithBlock(_marker);
				if(_curMarked != null){
					_outline.transform.position = _curMarked.transform.position;
					_outline.transform.localScale = _curMarked.transform.localScale * 1.1f;
					SetState(MARK);
				} else{
					SetState(AREA);
				}
				break;
			case SPOT:
				break;
			case AREA:
				break;
			case LINE:
				break;
			case MARK:
				_curMarked = _meshMgr.CollidesWithBlock(_marker);
				if(_curMarked != null){
					_outline.transform.position = _curMarked.transform.position;
					_outline.transform.localScale = _curMarked.transform.localScale * 1.1f;
				} else{
					SetState(IDLE);
				}
				break;
			default:
				break;
			}
		}
	}

	private void DrawSelectionArea(Vector3 basePos, Vector3 edgePos){
		Vector3 pos = basePos + (edgePos - basePos) / 2;
		pos.y = _curLevel + 0.2f;
		_selectArea.transform.position = pos;
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
			_outline.GetComponent<Renderer>().enabled = false;
			break;
		case AREA:
			_marker2.GetComponent<Renderer>().enabled = true;
			_selectArea.GetComponent<Renderer>().enabled = true;
			break;
		case MARK:
			_outline.GetComponent<Renderer>().enabled = true;
			break;
		default:
			break;
		}
	}

	//OnPress events for the interface buttons
	public void OnFloorPressed(){
		_curBlock = SimulationManager.Block.Floor;
	}

	public void OnWallPressed(){
		_curBlock = SimulationManager.Block.Wall;
	}

	public void OnCampFirePressed(){
		_curBlock = SimulationManager.Block.Campfire;
	}

	public void OnCoffeePressed(){
		_curBlock = SimulationManager.Block.Coffee;
	}

	public void OnToiletPressed(){
		_curBlock = SimulationManager.Block.Toilet;
	}

	public void OnOKPressed(){
		List<Vector3> storedCells;

		switch (_state) {
		case AREA:
			storedCells = StoreSelection(
				_marker.transform.position / 5, _marker2.transform.position / 5);
			_simMgr.SetType(storedCells, _curBlock);
			_meshMgr.AddMesh(_marker.transform.position, _marker2.transform.position, _curBlock);
			SetState (IDLE);
			break;
		case MARK:
			storedCells = StoreSelection(
				_marker.transform.position / 5, _marker2.transform.position / 5);
			_simMgr.SetType(storedCells, SimulationManager.Block.Empty);
			_meshMgr.DestroyMesh(_curMarked);
			SetState (IDLE);
			break;
		default:
			break;
		}
	}

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

	public void OnUpPressed(){
		_curLevel++;
		_eVis.SetFloor(_curLevel * 5f + 0.1f);
	}

	public void OnDownPressed(){
		_curLevel--;
		_eVis.SetFloor(_curLevel * 5f + 0.1f);
	}

	public void OnCheckboxChanged(bool value){
//		Debug.Log (value);
		_eVisToggle = !_eVisToggle;
		_eVis.SetVisible(_eVisToggle);
	}
}
