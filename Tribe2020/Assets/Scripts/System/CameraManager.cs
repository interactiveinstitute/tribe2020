using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using SimpleJSON;

public class CameraManager : MonoBehaviour {
	//Singleton features
	private static CameraManager _instance;
	public static CameraManager GetInstance() {
		return _instance;
	}

	#region Fields
	public bool debug = false;
	private PilotController _controller;

	public enum CameraState { Idle, Full, Room, Panned };
	public CameraState cameraState = CameraState.Idle;

	//Viewpoint variables
	public Vector2 startView = Vector2.zero;
	private Vector2 _curView = Vector2.zero;
	private Transform _curViewpoint;
	private Transform[][] _viewpoints;
	private Viewpoint[][] _views;

	//Camera movement variables
	private Vector3 _lastPos = Vector3.zero;
	private Vector3 _targetPos = Vector3.zero;
	private Vector3 _lastRot, _targetRot;

	//Orientation interaction variables
	public Camera gameCamera;
	private float _perspectiveZoomSpeed = 0.25f;
	private float _orthoZoomSpeed = 0.25f;
	private int _curFloor = 0;

	private float speed = 0.1f;
	private float startTime;
	private float journeyLength;

	private float _panSpeed = 0.01f;

	private bool _isInit = false;
	#endregion

	//Sort use instead of constructor
	void Awake() {
		_instance = this;
	}

	// Use this for initialization
	void Start() {
		_controller = PilotController.GetInstance();

		_lastPos = _targetPos = gameCamera.transform.position;
		_lastRot = _targetRot = gameCamera.transform.eulerAngles;

		//Populate collection of viewpoints
		PopulateViewpoints();
	}

	// Update is called once per frame
	void Update() {
		if(!_isInit) {
			SetViewpoint((int)startView.x, (int)startView.y, Vector2.zero);
			_isInit = true;
		}

		if(journeyLength > 0) {
			float distCovered = (Time.unscaledTime - startTime) * 10;
			float fracJourney = distCovered / journeyLength;

			if(cameraState == CameraManager.CameraState.Idle) {
				gameCamera.transform.position = Vector3.Lerp(_lastPos, _targetPos, fracJourney);
				gameCamera.transform.eulerAngles = Vector3.Lerp(_lastRot, _targetRot, fracJourney);
			}
		}
	}	

	//
	public void UpdateVisibility() {
		Viewpoint vp = _curViewpoint.GetComponent<Viewpoint>();

		foreach(GameObject go in vp.hideObjects) {
			go.GetComponent<MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly;
		}
	}

	//
	public void HideObstacles(Transform viewpointTrans) {
		Viewpoint vp = viewpointTrans.GetComponent<Viewpoint>();

		foreach(GameObject go in vp.hideObjects) {
			go.GetComponent<MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly;
		}
	}

	//
	public void UnhideObstacles(Transform viewpointTrans) {
		Viewpoint vp = viewpointTrans.GetComponent<Viewpoint>();

		foreach(GameObject go in vp.hideObjects) {
			go.GetComponent<MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
		}
	}

	//
	public void HideFloor(Transform floor) {
		foreach(Transform room in floor) {
			foreach(Transform group in room) {
				foreach(Transform mesh in group) {
					if(mesh.GetComponent<MeshRenderer>() != null) {
						mesh.GetComponent<MeshRenderer>().shadowCastingMode =
						UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly;
					}
				}
			}
		}
	}

	//
	public void ShowFloor(Transform floor) {
		foreach(Transform room in floor) {
			foreach(Transform group in room) {
				foreach(Transform mesh in group) {
					if(mesh.GetComponent<MeshRenderer>() != null) {
						mesh.GetComponent<MeshRenderer>().shadowCastingMode =
						UnityEngine.Rendering.ShadowCastingMode.On;
					}
				}
			}
		}
	}

	//
	private void PopulateViewpoints() {
		List<Viewpoint> viewPoints = new List<Viewpoint>(Object.FindObjectsOfType<Viewpoint>());

		int maxY = 0;

		//Find max y
		foreach(Viewpoint vp in viewPoints) {
			int curY = vp.yIndex;
			if(maxY <= curY) { maxY = curY + 1; }
		}
		_viewpoints = new Transform[maxY][];
		_views = new Viewpoint[maxY][];

		//Find max x for each floor
		for(int y = 0; y < maxY; y++) {
			int maxX = 0;

			foreach(Viewpoint vp in viewPoints) {
				if(vp.yIndex == y) {
					int curX = vp.xIndex;
					if(maxX <= curX) { maxX = curX + 1; }
				}
			}

			_viewpoints[y] = new Transform[maxX];
			_views[y] = new Viewpoint[maxX];

			int x = 0;
			foreach(Viewpoint vp in viewPoints) {
				if(vp.yIndex == y) {
					_views[y][x++] = vp;
				}
			}
		}
		
		//Add viewpoints to camera manager
		foreach(Viewpoint vp in viewPoints) {
			int curX = vp.xIndex;
			int curY = vp.yIndex;

			_viewpoints[curY][curX] = vp.transform;
		}
	}

	//
	public void SetViewpoint(int x, int y, Vector2 dir) {
		//Panned in y-axis but is out of x-index, switch to highest x-index
		if(x >= _viewpoints[y].Length) {
			x = _viewpoints[y].Length - 1;
		}

		_curView = new Vector2(x, y);

		_curViewpoint = _viewpoints[(int)_curView.y][(int)_curView.x];
		//HideObstacles(_curViewpoint);
		if(_viewpoints[y][x].GetComponent<Viewpoint>().locked) {
			if(dir == Vector2.right) { GotoRightView(); }
			if(dir == Vector2.left) { GotoLeftView(); }
			if(dir == Vector2.up) { GotoUpperView(); }
			if(dir == Vector2.down) { GotoLowerView(); }
			return;
		}

		_lastPos = gameCamera.transform.position;
		_targetPos = _curViewpoint.position;

		_lastRot = gameCamera.transform.eulerAngles;
		_targetRot = _curViewpoint.eulerAngles;

		startTime = Time.unscaledTime;
		journeyLength = Vector3.Distance(_lastPos, _targetPos);

		int[] above = new int[_viewpoints[(y + 1) % _viewpoints.Length].Length];

		//_controller.OnNewViewpoint(_curViewpoint.GetComponent<Viewpoint>().title, _viewpoints[y].Length, above.Length, x);
		_controller.OnNewViewpoint(_curViewpoint.GetComponent<Viewpoint>().title, _views, _curView);
	}

	//
	public Viewpoint GetViewPoint() {
		return _curViewpoint.GetComponent<Viewpoint>();
	}

	//
	public void GotoRightView(){
		int floorRooms = _viewpoints[(int)_curView.y].Length;
		SetViewpoint((int)(_curView.x + 1) % floorRooms, (int)_curView.y, Vector2.right);
	}

	//
	public void GotoLeftView(){
		int floorRooms = _viewpoints[(int)_curView.y].Length;
		SetViewpoint((int)(_curView.x + floorRooms - 1) % floorRooms, (int)_curView.y, Vector2.left);
	}

	//
	public void GotoUpperView(){
		int floors = _viewpoints.Length;
		SetViewpoint((int)_curView.x, (int)(_curView.y + 1) % floors, Vector2.up);
	}

	//
	public void GotoLowerView(){ 
		int floors = _viewpoints.Length;
		SetViewpoint((int)_curView.x, (int)(_curView.y + floors - 1) % floors, Vector2.down);
	}

	//
	public void UpdatePan(Vector2 deltaPan){
		gameCamera.transform.Translate(-deltaPan.x * _panSpeed, -deltaPan.y * _panSpeed, 0);

	}

	//
	public void UpdatePinchZoom(float deltaMagnitude){
		// Zoom differently depending on ortho or perspective
		if (gameCamera.orthographic){
			gameCamera.orthographicSize += deltaMagnitude * _orthoZoomSpeed;
			gameCamera.orthographicSize = Mathf.Max(gameCamera.orthographicSize, 0.1f);
		} else {
			gameCamera.fieldOfView += deltaMagnitude * _perspectiveZoomSpeed;
			gameCamera.fieldOfView = Mathf.Clamp(gameCamera.fieldOfView, 0.1f, 179.9f);
		}
	}

	//
	public JSONClass SerializeAsJSON() {
		JSONClass json = new JSONClass();

		return json;
	}

	//
	public void DeserializeFromJSON(JSONClass json) {
	}
}
