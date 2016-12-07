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
	public bool xWrap = false;
	public bool yWrap = false;
	private PilotController _controller;

	public enum CameraState { Idle, Full, Room, Panned };
	public CameraState cameraState = CameraState.Idle;

	//Viewpoint variables
	public Vector2 startCoordinates = Vector2.zero;
	public Vector2 currentCoordinates = Vector2.zero;
	//private Transform _curViewpoint;
	private Viewpoint _curView;
	//private Transform[][] _viewpoints;
	private Viewpoint[][] _views;
	private Viewpoint _overview;
	[SerializeField]
	private bool _inOverview;

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
		SetViewpoint((int)startCoordinates.x, (int)startCoordinates.y, Vector2.zero);
	}

	// Update is called once per frame
	void Update() {
		//if(!_isInit) {
		//	SetViewpoint((int)startCoordinates.x, (int)startCoordinates.y, Vector2.zero);
		//	_isInit = true;
		//}

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
		//Viewpoint vp = _curViewpoint.GetComponent<Viewpoint>();

		foreach(GameObject go in _curView.hideObjects) {
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
			if(vp.overview) {
				_overview = vp;
			} else {
				int curY = vp.yIndex;
				if(maxY <= curY) { maxY = curY + 1; }
			}
		}
		//_viewpoints = new Transform[maxY][];
		_views = new Viewpoint[maxY][];

		//Find max x for each floor
		for(int y = 0; y < maxY; y++) {
			int maxX = 0;

			foreach(Viewpoint vp in viewPoints) {
				if(!vp.overview && vp.yIndex == y) {
					int curX = vp.xIndex;
					if(maxX <= curX) { maxX = curX + 1; }
				}
			}

			//_viewpoints[y] = new Transform[maxX];
			_views[y] = new Viewpoint[maxX];

			int x = 0;
			foreach(Viewpoint vp in viewPoints) {
				if(!vp.overview && vp.yIndex == y) {
					_views[y][x++] = vp;
				}
			}
		}
		
		//Add viewpoints to camera manager
		foreach(Viewpoint vp in viewPoints) {
			if(!vp.overview) {
				int curX = vp.xIndex;
				int curY = vp.yIndex;

				//_viewpoints[curY][curX] = vp.transform;
				_views[curY][curX] = vp;
			}
		}
	}

	//
	public void GoToOverview() {
		_inOverview = true;
		_curView = _overview;

		_lastPos = gameCamera.transform.position;
		_targetPos = _curView.transform.position;

		_lastRot = gameCamera.transform.eulerAngles;
		_targetRot = _curView.transform.eulerAngles;

		startTime = Time.unscaledTime;
		journeyLength = Vector3.Distance(_lastPos, _targetPos);

		_controller.OnNewViewpoint(_curView, _views, _inOverview);
	}

	//
	public void GoToGridView() {
		_inOverview = false;

		SetViewpoint((int)currentCoordinates.x, (int)currentCoordinates.y, Vector2.zero);
	}

	//
	public void SetViewpoint(int x, int y, Vector2 dir) {
		//Out of bounds
		if(y > _views.Length - 1 || y < 0) { return; }

		//Panned in y-axis but is out of x-index, switch to highest x-index
		if(x >= _views[y].Length) {
			x = _views[y].Length - 1;
		}

		Vector2 targetCoordinates = new Vector2(x, y);

		//HideObstacles(_curViewpoint);
		if(_views[y][x].locked) {
			if(dir == Vector2.right) { GotoRightView(targetCoordinates); }
			if(dir == Vector2.left) { GotoLeftView(targetCoordinates); }
			if(dir == Vector2.up) { GotoUpperView(targetCoordinates); }
			if(dir == Vector2.down) { GotoLowerView(targetCoordinates); }
			return;
		} else {
			//Update view state
			currentCoordinates = targetCoordinates;
			_curView = _views[(int)currentCoordinates.y][(int)currentCoordinates.x];

			//Prepare interpolated transition
			_lastPos = gameCamera.transform.position;
			_targetPos = _curView.transform.position;

			_lastRot = gameCamera.transform.eulerAngles;
			_targetRot = _curView.transform.eulerAngles;

			startTime = Time.unscaledTime;
			journeyLength = Vector3.Distance(_lastPos, _targetPos);

			//Controller callback
			_controller.OnNewViewpoint(_curView, _views, _inOverview);
		}
	}

	//
	public Viewpoint GetViewpoint(int x, int y) {
		return _views[y][x];
	}

	//
	public Viewpoint[][] GetViewpoints() {
		return _views;
	}

	//
	public Vector2 GetCurrentCoordinates() {
		return currentCoordinates;
	}

	//
	public Viewpoint GetCurrentViewpoint() {
		return _curView;
	}

	//
	public void GotoRightView() {
		GotoRightView(currentCoordinates);
	}

	//
	public void GotoLeftView() {
		GotoLeftView(currentCoordinates);
	}

	//
	public void GotoUpperView() {
		GotoUpperView(currentCoordinates);
	}

	//
	public void GotoLowerView() {
		GotoLowerView(currentCoordinates);
	}

	//
	public void GotoRightView(Vector2 target) {
		if(_inOverview) { return; }

		int floorRooms = _views[(int)target.y].Length;
		if(xWrap) {
			SetViewpoint((int)(target.x + 1) % floorRooms, (int)target.y, Vector2.right);
		} else if(target.x < floorRooms - 1)  {
			SetViewpoint((int)(target.x + 1), (int)target.y, Vector2.right);
		}
	}

	//
	public void GotoLeftView(Vector2 target) {
		if(_inOverview) { return; }

		int floorRooms = _views[(int)target.y].Length;
		if(xWrap) {
			SetViewpoint((int)(target.x + floorRooms - 1) % floorRooms, (int)target.y, Vector2.left);
		} else if(target.x > 0)  {
            SetViewpoint((int)(target.x - 1), (int)target.y, Vector2.left);
		}
	}

	//
	public void GotoUpperView(Vector2 target) {
		if(_inOverview) { return; }

		int floors = _views.Length;
		if(yWrap) {
			SetViewpoint((int)target.x, (int)(target.y + 1) % floors, Vector2.up);
		} else if(target.y < floors - 1) {
			SetViewpoint((int)target.x, (int)(target.y + 1), Vector2.up);
		}
	}

	//
	public void GotoLowerView(Vector2 target) {
		if(_inOverview) { return; }

		int floors = _views.Length;
		if(yWrap) {
			SetViewpoint((int)target.x, (int)(target.y + floors - 1) % floors, Vector2.down);
		} else if(target.y > 0) {
			SetViewpoint((int)target.x, (int)(target.y - 1), Vector2.down);
		}
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
	public void UnlockView(int x, int y) {
		_views[y][x].locked = false;
	}

	//
	public JSONClass SerializeAsJSON() {
		JSONClass json = new JSONClass();

		JSONArray floors = new JSONArray();
		for(int y = 0; y < _views.Length; y++) {
			JSONArray views = new JSONArray();
			for(int x = 0; x < _views[y].Length; x++) {
				JSONClass viewJSON = new JSONClass();
				viewJSON.Add("locked", _views[y][x].locked.ToString());
				views.Add(viewJSON);
			}
			floors.Add(views);
		}
		json.Add("views", floors);

		return json;
	}

	//
	public void DeserializeFromJSON(JSONClass json) {
		if(json != null) {
			JSONArray views = json["views"].AsArray;

			for(int y = 0; y < _views.Length; y++) {
				for(int x = 0; x < _views[y].Length; x++) {
					_views[y][x].locked = views[y][x]["locked"].AsBool;
				}
			}
		}
	}
}
