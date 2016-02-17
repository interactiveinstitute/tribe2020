using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CameraManager : MonoBehaviour {
	//Singleton features
	private static CameraManager _instance;
	public static CameraManager GetInstance(){
		return _instance;
	}

	public string cameraState = IDLE;
	public const string IDLE = "camera_idle";
	public const string FULL = "camera_full";
	public const string ROOM = "camera_room";
	
	private int _viewPointIndex = 0;
	private Transform _currentViewPoint;
	private Transform[] _viewpoints;

	private Vector3 _lastPos = Vector3.zero;
	private Vector3 _targetPos = Vector3.zero;

	//Orientation interaction parameters
	public GameObject cameraHolder;
	public Camera camera;
	public float perspectiveZoomSpeed = 0.25f;
	public float orthoZoomSpeed = 0.25f;

	public float speed = 0.1f;
	private float startTime;
	private float journeyLength;

	//Sort use instead of constructor
	void Awake(){
		_instance = this;
	}

	// Use this for initialization
	void Start(){
		//Ref to camera
		cameraHolder = GameObject.FindWithTag("camera_holder") as GameObject;
		camera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();

		GameObject[] tmpViewpoints = GameObject.FindGameObjectsWithTag("ViewPoint");
		_viewpoints = new Transform[tmpViewpoints.Length];

		foreach(GameObject go in tmpViewpoints){
			_viewpoints[go.GetComponent<Viewpoint>().index] = go.transform;
		}

		NextViewpoint();
	}
	
	// Update is called once per frame
	void Update(){
		float distCovered = (Time.time - startTime) * 10;
		float fracJourney = distCovered / journeyLength;

		if(cameraState == IDLE){
			camera.transform.position = Vector3.Lerp(_lastPos, _targetPos, fracJourney);
		}
	}

	//
	public void NextViewpoint(){
		_viewPointIndex = (_viewPointIndex + 1) % _viewpoints.Length;
		_currentViewPoint = _viewpoints[_viewPointIndex];

		_lastPos = camera.transform.position;
		_targetPos = _currentViewPoint.position;

		startTime = Time.time;
		journeyLength = Vector3.Distance(_lastPos, _targetPos);
	}
	
	//
	public void PrevViewpoint(){
		_viewPointIndex = (_viewPointIndex + _viewpoints.Length - 1) % _viewpoints.Length;
		_currentViewPoint = _viewpoints[_viewPointIndex];
		
		_lastPos = camera.transform.position;
		_targetPos = _currentViewPoint.position;
		
		startTime = Time.time;
		journeyLength = Vector3.Distance(_lastPos, _targetPos);
	}
}
