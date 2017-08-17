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

	[Header("External References")]
	public Camera gameCamera;
	public Animator animator;
	public Transform overlayCanvas;
	public ParticleSystem fireworks;

	private CameraInterface _interface;

	[Header("Camera State")]
	public CameraState cameraState = CameraState.PlayerControl;
	public enum CameraState { Idle, Full, Room, Panned, Animating, PlayerControl };

	//Viewpoint variables
	public Vector2 startCoordinates = Vector2.zero;
	public Vector2 currentCoordinates = Vector2.zero;
	private Viewpoint _curView;
	private Viewpoint[][] _views;
	private Viewpoint _overview;
	[SerializeField]
	private bool _inOverview;

	//Camera movement variables
	private float _defaultFOV;
	private Vector3 _lastPos = Vector3.zero;
	private Vector3 _targetPos = Vector3.zero;
	private Quaternion _lastRot, _targetRot;
	private float _lastFOV;
	private bool _isLooking = false;
	[SerializeField]
	private Quaternion _lookAtRotation;
	[SerializeField, ShowOnly]
	private float _lookaAtFOV;
	[SerializeField]
	[Range(0, 10)]
	private float zoomInLevel;

	[Range(0, 1)]
	public float fracJourney = 0;

	//Orientation interaction variables
	private float _perspectiveZoomSpeed = 0.25f;
	private float _orthoZoomSpeed = 0.25f;
	private int _curFloor = 0;

	private float speed = 0.1f;
	private float startTime;
	[ShowOnly]
	public float journeyLength;
	private float _panSpeed = 0.01f;

	private bool _firstLoop = true;
	#endregion

	//Sort use instead of constructor
	void Awake() {
		_instance = this;
	}

	// Use this for initialization
	void Start() {
		_lastFOV = _defaultFOV = gameCamera.fieldOfView;
		_lastPos = _targetPos = gameCamera.transform.position;
		_lastRot = _targetRot = gameCamera.transform.rotation;

		//Populate collection of viewpoints
		PopulateViewpoints();
	}

	// Update is called once per frame
	void Update() {
		//Initialization after start
		if(_firstLoop) {
			SetViewpoint((int)startCoordinates.x, (int)startCoordinates.y, Vector2.zero);
			OnFloorChange(startCoordinates);
			_firstLoop = false;
		}

		//If on it's way somewhere, update motion
		if(journeyLength > 0) {
			float distCovered = (Time.unscaledTime - startTime) * 10;
			fracJourney = distCovered / 3;

			if(!_isLooking) {
				gameCamera.transform.position = Vector3.Lerp(_lastPos, _targetPos, fracJourney);
				gameCamera.transform.rotation = Quaternion.Lerp(_lastRot, _targetRot, fracJourney);
				gameCamera.fieldOfView = Mathf.Lerp(_lastFOV, _defaultFOV, fracJourney);

				if(fracJourney > 0.99f) {
					gameCamera.transform.position = _targetPos;
					gameCamera.transform.rotation = _targetRot;
					journeyLength = 0;
					OnCameraArrived();
				}
			} else {
				gameCamera.transform.rotation = Quaternion.Lerp(_lastRot, _lookAtRotation, fracJourney);
				gameCamera.fieldOfView = Mathf.Lerp(_lastFOV, _lookaAtFOV, fracJourney);
			}
		}
	}

	//
	public void SetInterface(CameraInterface i) {
		_interface = i;
	}

	//
	public void UpdateVisibility() {
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

		//Find max y
		int maxY = 0;
		foreach(Viewpoint vp in viewPoints) {
			if(vp.overview) {
				_overview = vp;
			} else {
				int curY = (int)vp.coordinates.y;
				if(maxY <= curY) { maxY = curY + 1; }
			}
		}
		_views = new Viewpoint[maxY][];

		//Find max x for each floor
		for(int y = 0; y < maxY; y++) {
			int maxX = 0;

			foreach(Viewpoint vp in viewPoints) {
				if(!vp.overview && vp.coordinates.y == y) {
					int curX = (int)vp.coordinates.x;
					if(maxX <= curX) { maxX = curX + 1; }
				}
			}
			_views[y] = new Viewpoint[maxX];

			int x = 0;
			foreach(Viewpoint vp in viewPoints) {
				if(!vp.overview && vp.coordinates.y == y) {
					_views[y][x++] = vp;
				}
			}
		}

		//Add viewpoints to camera manager
		foreach(Viewpoint vp in viewPoints) {
			if(!vp.overview) {
				int curX = (int)vp.coordinates.x;
				int curY = (int)vp.coordinates.y;
				_views[curY][curX] = vp;
			}
		}
	}

	//
	private void SaveCurrentAsLastCameraState() {
		_lastPos = gameCamera.transform.position;
		_lastRot = gameCamera.transform.rotation;
		_lastFOV = gameCamera.fieldOfView;
	}

	//
	public void GoToOverview() {
		_inOverview = true;
		_curView = _overview;

		SaveCurrentAsLastCameraState();

		_targetPos = _curView.transform.position;
		_targetRot = _curView.transform.rotation;

		startTime = Time.unscaledTime;
		journeyLength = Vector3.Distance(_lastPos, _targetPos);

		_interface.OnNewViewpoint(_curView, _views, _inOverview);
	}

	//
	public void GoToGridView() {
		_inOverview = false;

		SetViewpoint((int)currentCoordinates.x, (int)currentCoordinates.y, Vector2.zero);
	}

	public void SetLookAtTarget(Appliance appliance) {
		SaveCurrentAsLastCameraState();
		_isLooking = true;
		// distance to zoomed in object
		//float distance = Vector3.Distance(gameCamera.transform.position, appliance.transform.position
		journeyLength = Vector3.Distance(gameCamera.transform.position, appliance.transform.position);

		////FOV is the vertical agnle of the viewport, so let's use the height (and distance) for calculating how much we should zoom in
		//_lookaAtFOV = Mathf.Rad2Deg * 2 * Mathf.Atan2(applianceHeight, distance);

		_lookaAtFOV = calculateLookAtFOV(appliance);

		startTime = Time.unscaledTime;

		//Find center of appliance in order to set camera rotation
		float applianceHeight = 1.5f; //Set a default approximate height for things without collider
		if(appliance.GetComponent<BoxCollider>()) {
			applianceHeight = appliance.GetComponent<BoxCollider>().bounds.size.y;
		}
		Vector3 appliancePosition = appliance.transform.position;
		appliancePosition.y += applianceHeight / 2;
		Vector3 relativePos = appliancePosition - gameCamera.transform.position;
		_lookAtRotation = Quaternion.LookRotation(relativePos);
		//Position thee appliance on the left side of the screen
		_lookAtRotation *= Quaternion.Euler(0, FOVToHFOV(_lookaAtFOV) / 4, 0);
	}

	private float calculateLookAtFOV(Appliance appliance) {
		// distance to appliance from camera
		float distance = Vector3.Distance(gameCamera.transform.position, appliance.transform.position);
		DebugManager.Log("Distance to appliance: " + distance, this, this);

		// Use collider if we have one
		BoxCollider collider = appliance.GetComponent<BoxCollider>();
		if(collider) {
			Vector3 cen = collider.bounds.center;
			Vector3 ext = collider.bounds.extents;
			Vector2[] extentPoints = new Vector2[8]
			{
				gameCamera.WorldToScreenPoint(new Vector3(cen.x-ext.x, cen.y-ext.y, cen.z-ext.z)),
				gameCamera.WorldToScreenPoint(new Vector3(cen.x+ext.x, cen.y-ext.y, cen.z-ext.z)),
				gameCamera.WorldToScreenPoint(new Vector3(cen.x-ext.x, cen.y-ext.y, cen.z+ext.z)),
				gameCamera.WorldToScreenPoint(new Vector3(cen.x+ext.x, cen.y-ext.y, cen.z+ext.z)),
				gameCamera.WorldToScreenPoint(new Vector3(cen.x-ext.x, cen.y+ext.y, cen.z-ext.z)),
				gameCamera.WorldToScreenPoint(new Vector3(cen.x+ext.x, cen.y+ext.y, cen.z-ext.z)),
				gameCamera.WorldToScreenPoint(new Vector3(cen.x-ext.x, cen.y+ext.y, cen.z+ext.z)),
				gameCamera.WorldToScreenPoint(new Vector3(cen.x+ext.x, cen.y+ext.y, cen.z+ext.z))
			};
			Vector2 min = extentPoints[0];
			Vector2 max = extentPoints[0];
			foreach(Vector2 v in extentPoints) {
				min = Vector2.Min(min, v);
				max = Vector2.Max(max, v);
			}

			//Debug draw the found box
			//{
			//    Color color = Color.red;
			//    Rect box = new Rect(min, max - min);
			//    Texture2D texture = new Texture2D(1, 1);
			//    texture.SetPixel(0, 0, color);
			//    texture.Apply();
			//    GUI.skin.box.normal.background = texture;
			//    GUI.Box(box, GUIContent.none);
			//}

			//Find if wider of taller
			float heightOnScreen = max.y - min.y;
			float widthOnScreen = max.x - min.x;

			//This is some code i've used to debug if the calculated world points actually is representing the outer limits of the collider. Strangely, the points are drawn on weird distances when zoomed out
			// and correctly when zoomed in. 
			//Vector3 minInWorld = gameCamera.ScreenToWorldPoint(new Vector3(min.x, min.y, distance));
			//Vector3 maxInWorld = gameCamera.ScreenToWorldPoint(new Vector3(max.x, max.y, distance));
			//DrawLine(maxInWorld, minInWorld, Color.red, 30.0f);

			//Should we use width or height?
			if(heightOnScreen > widthOnScreen) {
				//Set field of view from height
				Vector3 heightStartPoint = gameCamera.ScreenToWorldPoint(new Vector3(min.x, min.y, distance));
				Vector3 heightEndPoint = gameCamera.ScreenToWorldPoint(new Vector3(min.x, max.y, distance));
				Vector3 heightVector = heightEndPoint - heightStartPoint;
				//DrawLine(heightStartPoint, heightEndPoint, Color.red, 20.0f);
				return 1.2f * Mathf.Rad2Deg * 2 * Mathf.Atan2(heightVector.magnitude / 2, distance);
			} else {
				//Set field of view from width
				Vector3 widthStartPoint = gameCamera.ScreenToWorldPoint(new Vector3(min.x, min.y, distance));
				Vector3 widthEndPoint = gameCamera.ScreenToWorldPoint(new Vector3(max.x, min.y, distance));
				Vector3 widthVector = widthEndPoint - widthStartPoint;
				//DrawLine(widthStartPoint, widthEndPoint, Color.red, 20.0f);
				float hFOV = Mathf.Rad2Deg * 2 * Mathf.Atan2(widthVector.magnitude / 2, distance);
				//Be aware that we should set it double, since we can only use the left side of screen. I.E. Double the fov after calculation so we can us half of that to show appliance zoomed in.
				hFOV *= 2;

				return 1.2f * HFOVToFOV(hFOV);
			}
		} else {
			// We had no AABB. Let's try with a default height and the distance to the appliance
			//FOV is the vertical agnle of the viewport, so let's use some height (and distance) for calculating how much we should zoom in
			float defaultHeight = 1.5f;
			return Mathf.Rad2Deg * 2 * Mathf.Atan2(defaultHeight, distance);
		}
	}

	void DrawLine(Vector3 start, Vector3 end, Color color, float duration = 0.2f) {
		GameObject myLine = new GameObject();
		myLine.transform.position = start;
		myLine.AddComponent<LineRenderer>();
		LineRenderer lr = myLine.GetComponent<LineRenderer>();
		lr.material = new Material(Shader.Find("Particles/Alpha Blended Premultiply"));
		lr.SetColors(color, color);
		lr.SetWidth(0.1f, 0.1f);
		lr.SetPosition(0, start);
		lr.SetPosition(1, end);
		GameObject.Destroy(myLine, duration);
	}

	private float FOVToHFOV(float FOV) {
		// calculate the horizontal FOV from FOV using aspect ratio
		float radAngle = FOV * Mathf.Deg2Rad;
		float radHFOV = 2 * Mathf.Atan(Mathf.Tan(radAngle / 2) * gameCamera.aspect);
		return Mathf.Rad2Deg * radHFOV;
	}

	private float HFOVToFOV(float HFOV) {
		// calculate the horizontal FOV from FOV using aspect ratio
		float radAngle = Mathf.Deg2Rad * HFOV;
		float radFOV = 2 * Mathf.Atan(Mathf.Tan(radAngle / 2) / gameCamera.aspect);
		return Mathf.Rad2Deg * radFOV;
	}

	public void ClearLookAtTarget() {
		//Save the current zoom as the _lookAtFOV
		_lookaAtFOV = gameCamera.fieldOfView;
		_isLooking = false;
		startTime = Time.unscaledTime;
		SaveCurrentAsLastCameraState();
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
			if(dir == Vector2.up) {
				GotoUnlockedViewPointOnFloor((int)targetCoordinates.y, dir);
				GotoUpperView(targetCoordinates);
			}
			if(dir == Vector2.down) {
				GotoUnlockedViewPointOnFloor((int)targetCoordinates.y, dir);
				//GotoLowerView(targetCoordinates);
			}
			return;
		} else {
			//Is new viewpoint on another floor?
			if(TryFloorChange(targetCoordinates)) {
				OnFloorChange(targetCoordinates);
			}

			//Update view state
			currentCoordinates = targetCoordinates;
			_curView = _views[(int)currentCoordinates.y][(int)currentCoordinates.x];

			//Save from state
			SaveCurrentAsLastCameraState();

			//Set to state
			_targetPos = _curView.transform.position;
			_targetRot = _curView.transform.rotation;

			startTime = Time.unscaledTime;
			journeyLength = Vector3.Distance(_lastPos, _targetPos);

			//Controller callback
			_interface.OnNewViewpoint(_curView, _views, _inOverview);
		}
	}

	//
	public void SetViewpoint(string viewpoint) {
		for(int y = 0; y < _views.Length; y++) {
			for(int x = 0; x < _views[y].Length; x++) {
				if(_views[y][x].title == viewpoint) {
					SetViewpoint(x, y, Vector2.zero);
				}
			}
		}
	}

	//
	bool TryFloorChange(Vector2 targetCoordinates) {
		return targetCoordinates.y != currentCoordinates.y;
	}

	//
	void OnFloorChange(Vector2 targetCoordinates) {
		if(_curView.relatedZones.Count > 0) {
			//Turn off lights on current floor
			Floor currentFloor = _curView.relatedZones[0].GetFloor();
			currentFloor.ToggleLightActive(false);

			//Turn on lights on new floor
			Floor newFloor = _views[(int)targetCoordinates.y][(int)targetCoordinates.x].relatedZones[0].GetFloor();
			newFloor.ToggleLightActive(true);
		}
	}

	//Callback for when lerping between views is over and camera is still
	public void OnCameraArrived() {
		_interface.OnCameraArrived(_curView);
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
	Viewpoint GetFirstUnlockedViewpointOnFloor(int y) {
		foreach(Viewpoint viewpoint in _views[y]) {
			if(!viewpoint.locked) {
				return viewpoint;
			}
		}
		return null;
	}

	//
	void GotoUnlockedViewPointOnFloor(int y, Vector2 dir) {
		foreach(Viewpoint viewpoint in _views[y]) {
			if(!viewpoint.locked) {
				SetViewpoint((int)viewpoint.coordinates.x, (int)viewpoint.coordinates.y, dir);
				return;
			}
		}
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
		} else if(target.x < floorRooms - 1) {
			SetViewpoint((int)(target.x + 1), (int)target.y, Vector2.right);
		}
	}

	//
	public void GotoLeftView(Vector2 target) {
		if(_inOverview) { return; }

		int floorRooms = _views[(int)target.y].Length;
		if(xWrap) {
			SetViewpoint((int)(target.x + floorRooms - 1) % floorRooms, (int)target.y, Vector2.left);
		} else if(target.x > 0) {
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
	public void UpdatePan(Vector2 deltaPan) {
		gameCamera.transform.Translate(-deltaPan.x * _panSpeed, -deltaPan.y * _panSpeed, 0);

	}

	//
	public void UpdatePinchZoom(float deltaMagnitude) {
		// Zoom differently depending on ortho or perspective
		if(gameCamera.orthographic) {
			gameCamera.orthographicSize += deltaMagnitude * _orthoZoomSpeed;
			gameCamera.orthographicSize = Mathf.Max(gameCamera.orthographicSize, 0.1f);
		} else {
			gameCamera.fieldOfView += deltaMagnitude * _perspectiveZoomSpeed;
			gameCamera.fieldOfView = Mathf.Clamp(gameCamera.fieldOfView, 0.1f, 179.9f);
		}
	}

	//
	public void UnlockViews(int y) {
		foreach(Viewpoint vp in _views[y]) {
			vp.locked = false;
		}
	}

	//
	public void UnlockView(int x, int y) {
		_views[y][x].locked = false;
	}

	//
	public void PlayAnimation(string animation) {
		animator.enabled = true;
		animator.Play(animation);

		cameraState = CameraState.Animating;
	}

	//
	public void PlayFireworks() {
		fireworks.Play();
	}

	//
	public void StopAnimation() {
		animator.StopPlayback();
		overlayCanvas.GetComponentInChildren<Image>().color = new Color(0, 0, 0, 0);
		animator.enabled = false;

		cameraState = CameraState.PlayerControl;
	}

	//
	public void OnAnimationEvent(string animationEvent) {
		_interface.OnAnimationEvent(animationEvent);
	}

	//Serialize state as json for a save file
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

	//Deserialize json and apply states to aspects handled by the manager
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
