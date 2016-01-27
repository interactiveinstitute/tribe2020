using UnityEngine;
using System.Collections;

public class InteractionManager : MonoBehaviour{
	//Singleton features
	private static InteractionManager _instance;
	public static InteractionManager GetInstance(){
		return _instance;
	}

	//Orientation interaction parameters
	private GameObject cameraHolder;
	private Camera _camera;
	public float perspectiveZoomSpeed = 0.25f;
	public float orthoZoomSpeed = 0.25f;

	//Interaction parameters
	private float _touchTimer = 0;
	private Vector3 _startPos;
	public const float TAP_TIMEOUT = 0.25f;
	public const float SWIPE_THRESH = 50;

	//Sort use instead of constructor
	void Awake(){
		_instance = this;
	}

	// Use this for initialization
	void Start(){
		//Ref to camera
		cameraHolder = GameObject.FindWithTag("camera_holder") as GameObject;
		_camera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
	}
	
	// Update is called once per frame
	void Update(){
		//Mobile interaction
		UpdatePan(_camera);
		UpdatePinch(_camera);
		
		//Touch start
		if(Input.GetMouseButtonDown(0)){
			OnTouchStart(Input.mousePosition);
		}
		
		//Touch ongoing
		if(Input.GetMouseButton(0)){
			OnTouch(Input.mousePosition);
		}
		
		//Touch end
		if(Input.GetMouseButtonUp(0)){
			OnTouchEnded(Input.mousePosition);
		}
	}

	//
	private void OnTouchStart(Vector3 pos){
		_touchTimer = 0;
		_startPos = pos;
	}
	
	//
	private void OnTouch(Vector3 pos){
		_touchTimer += Time.deltaTime;
	}
	
	//
	private void OnTouchEnded(Vector3 pos){
		float dist = Vector3.Distance(_startPos, pos);

		//Touch ended before tap timeout, trigger OnTap
		if(_touchTimer < TAP_TIMEOUT && dist < SWIPE_THRESH) {
			OnTap(pos);
		} else if(dist >= SWIPE_THRESH) {
			OnSwipe(_startPos, pos);
		}
	}
	
	//
	private void OnTap(Vector3 pos){
		RaycastHit hit;
		Ray ray = Camera.main.ScreenPointToRay(pos);
		
		if(Physics.Raycast(ray, out hit)){  
			Transform selected = hit.transform;
			Debug.Log("hit: " + selected.tag);
		}
	}

	//
	private void OnSwipe(Vector3 start, Vector3 end){
//		float dir = Vector3.Angle(start, end);
		float dir = Mathf.Atan2(end.y - start.y, end.x - start.x);
		dir = (dir * Mathf.Rad2Deg + 360) % 360;
		float dist = Vector3.Distance(start, end);

		float dirMod = (dir + 90) % 360;
		if(dirMod > 45 && dirMod <= 135){
			Debug.Log("swipe right, " + dir);
		} else if(dir > 45 && dir <= 135){
			Debug.Log("swipe up, " + dir);
		} else if(dir > 135 && dir <= 225){
			Debug.Log("swipe left, " + dir);
		} else if(dir > 225 && dir <= 315){
			Debug.Log("swipe down, " + dir);
		}
	}
	
	//
	private void OnButtonTap(string button){
	}
	
	//
	private void OnDoubleTap(){
	}

	//
	public void UpdatePan(Camera camera){
		Vector3 camPos = _camera.transform.position;
		Transform camTransform = _camera.transform;
		
		float tSpeed = 0.1F;
		
		#if UNITY_ANDROID
		if ((Input.touchCount == 1 && Input.GetTouch(0).phase == TouchPhase.Moved)
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
			camPos.z += 50 * Time.deltaTime;
		} else if(Input.GetKey ("s")){
			camPos.z -= 50 * Time.deltaTime;
		} else if(Input.GetKey ("a")){
			camPos.x -= 50 * Time.deltaTime;
		} else if(Input.GetKey ("d")){
			camPos.x += 50 * Time.deltaTime;
		}
		
		_camera.transform.position = camPos;
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
}
