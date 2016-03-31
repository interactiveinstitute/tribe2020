using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class CameraManager : MonoBehaviour {
	//Singleton features
	private static CameraManager _instance;
	public static CameraManager GetInstance(){
		return _instance;
	}

	private ViewManager _uiMgr;

	public string cameraState = IDLE;
	public const string IDLE = "camera_idle";
	public const string FULL = "camera_full";
	public const string ROOM = "camera_room";
	public const string PANNED = "camera_panned";

	//public Transform pilotTransform;
	
	//Viewpoint variables
	private Vector2 _curView = Vector2.zero;
	private Transform _curViewpoint;
	private Transform[][] _viewpoints;

	//Camera movement variables
	private Vector3 _lastPos = Vector3.zero;
	private Vector3 _targetPos = Vector3.zero;
	private Vector3 _lastRot, _targetRot;

	//Orientation interaction variables
	//public GameObject cameraHolder;
	public Camera gameCamera;
	private float _perspectiveZoomSpeed = 0.25f;
	private float _orthoZoomSpeed = 0.25f;
	private int _curFloor =  0;

	private float speed = 0.1f;
	private float startTime;
	private float journeyLength;

	private float _panSpeed = 0.01f;

	//Sort use instead of constructor
	void Awake(){
		_instance = this;
	}

	// Use this for initialization
	void Start(){
		_uiMgr = ViewManager.GetInstance();

		//Ref to camera
		//cameraHolder = GameObject.FindWithTag("camera_holder") as GameObject;
		//gameCamera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
		_lastPos = _targetPos = gameCamera.transform.position;
		_lastRot = _targetRot = gameCamera.transform.eulerAngles;

		//Populate collection of viewpoints
		PopulateViewpoints(GameObject.FindGameObjectsWithTag("ViewPoint"));

		SetViewpoint(0, 0);
		//UpdateVisibility();
	}
	
	// Update is called once per frame
	void Update(){
		if(journeyLength > 0) {
			float distCovered = (Time.time - startTime) * 10;
			float fracJourney = distCovered / journeyLength;

			if(cameraState == IDLE) {
				gameCamera.transform.position = Vector3.Lerp(_lastPos, _targetPos, fracJourney);
				gameCamera.transform.eulerAngles = Vector3.Lerp(_lastRot, _targetRot, fracJourney);
			}
		}
	}

	//
	public void UpdateVisibility(){
		Viewpoint vp = _curViewpoint.GetComponent<Viewpoint>();

        foreach(GameObject go in vp.hideObjects){
            go.GetComponent<MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly;
        }

		//if(vp.showFloor){
		//	foreach(Transform floor in pilotTransform){
		//		if(floor.name == "Floor " + vp.floor){
		//			ShowFloor(floor);
		//		} else{
		//			HideFloor(floor);
		//		}
		//	}
		//} else if(vp.showPilot){
		//	foreach(Transform floor in pilotTransform){
		//		ShowFloor(floor);
		//	}
		//}

		//if(vp.hideNorth){
		//	HideWall("North Wall");
		//} else if(vp.hideEast){
		//	HideWall("East Wall");
		//} else if(vp.hideSouth){
		//	HideWall("South Wall");
		//} else if(vp.hideWest){
		//	HideWall("West Wall");
		//}
	}

    public void HideObstacles(Transform viewpointTrans){
        Viewpoint vp = viewpointTrans.GetComponent<Viewpoint>();

        foreach(GameObject go in vp.hideObjects){
            go.GetComponent<MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly;
        }
    }

    public void UnhideObstacles(Transform viewpointTrans){
        Viewpoint vp = viewpointTrans.GetComponent<Viewpoint>();

        foreach(GameObject go in vp.hideObjects){
            go.GetComponent<MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
        }
    }

	//
	//public void HideWall(string wall){
	//	foreach(Transform floor in pilotTransform){
	//		foreach(Transform room in floor){
	//			if(room.name == "Exterior"){
	//				foreach(Transform group in room){
	//					if(group.name == wall){
	//						foreach(Transform mesh in group){
	//							mesh.GetComponent<MeshRenderer>().shadowCastingMode =
	//								UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly;
	//							if(mesh.GetComponent<MeshCollider>()){
	//								mesh.GetComponent<MeshCollider>().enabled = false;
	//							}
	//						}
	//					}
	//				}
	//			}
	//		}
	//	}
	//}

	//
	public void HideFloor(Transform floor){
		foreach(Transform room in floor){
			foreach(Transform group in room){
				foreach(Transform mesh in group){
					if(mesh.GetComponent<MeshRenderer>() != null){
						mesh.GetComponent<MeshRenderer>().shadowCastingMode =
						UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly;
					}
				}
			}
		}
	}

	//
	public void ShowFloor(Transform floor){
		foreach(Transform room in floor){
			foreach(Transform group in room){
				foreach(Transform mesh in group){
					if(mesh.GetComponent<MeshRenderer>() != null){
						mesh.GetComponent<MeshRenderer>().shadowCastingMode =
						UnityEngine.Rendering.ShadowCastingMode.On;
					}
				}
			}
		}
	}

	//
	private void PopulateViewpoints(GameObject[] viewObjects){
		int maxY = 0;

		foreach(GameObject vo in viewObjects){
			int curY = vo.GetComponent<Viewpoint>().yIndex;
			if(maxY <= curY){ maxY = curY + 1; }
		}
		_viewpoints = new Transform[maxY][];

		for(int y = 0; y < maxY; y++){
			int maxX = 0;

			foreach(GameObject vo in viewObjects){
				if(vo.GetComponent<Viewpoint>().yIndex == y){
					int curX = vo.GetComponent<Viewpoint>().xIndex;
					if(maxX <= curX){ maxX = curX + 1; }
				}
			}

			_viewpoints[y] = new Transform[maxX];
		}

		foreach(GameObject vo in viewObjects){
			int curX = vo.GetComponent<Viewpoint>().xIndex;
			int curY = vo.GetComponent<Viewpoint>().yIndex;
			
			_viewpoints[curY][curX] = vo.transform;
		}

		//Debug.Log("Populated " + _viewpoints.Length + " floors with " + viewObjects.Length + " views");
	}

	//
	public void SetViewpoint(int x, int y){
		if(x >= _viewpoints[y].Length){
			x = 0;
		}

		_curView = new Vector2(x, y);

		if(_curViewpoint != null) {
			UnhideObstacles(_curViewpoint);
		}
		_curViewpoint = _viewpoints[(int)_curView.y][(int)_curView.x];
		HideObstacles(_curViewpoint);

		_lastPos = gameCamera.transform.position;
		_targetPos = _curViewpoint.position;

		_lastRot = gameCamera.transform.eulerAngles;
		_targetRot = _curViewpoint.eulerAngles;

		startTime = Time.time;
		journeyLength = Vector3.Distance(_lastPos, _targetPos);

		_uiMgr.title.GetComponent<Text>().text = _curViewpoint.GetComponent<Viewpoint>().title;

		//UpdateVisibility();

		_uiMgr.UpdateViewpointGuide(_viewpoints[y].Length, x);
	}

	//
	public void GotoRightView(){
		int floorRooms = _viewpoints[(int)_curView.y].Length;
		SetViewpoint((int)(_curView.x + 1) % floorRooms, (int)_curView.y);
	}

	//
	public void GotoLeftView(){
		int floorRooms = _viewpoints[(int)_curView.y].Length;
		SetViewpoint((int)(_curView.x + floorRooms - 1) % floorRooms, (int)_curView.y);
	}

	//
	public void GotoUpperView(){
		int floors = _viewpoints.Length;
		SetViewpoint((int)_curView.x, (int)(_curView.y + 1) % floors);
	}

	//
	public void GotoLowerView(){ 
		int floors = _viewpoints.Length;
		SetViewpoint((int)_curView.x, (int)(_curView.y + floors - 1) % floors);
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
}
