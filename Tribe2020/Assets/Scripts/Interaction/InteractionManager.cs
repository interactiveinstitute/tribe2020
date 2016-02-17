using UnityEngine;
using System.Collections;

public class InteractionManager : MonoBehaviour{
	//Singleton features
	private static InteractionManager _instance;
	public static InteractionManager GetInstance(){
		return _instance;
	}

	public GameObject TitleUI;
	public GameObject InspectorUI;

	private CameraManager _camMgr;

	//Interaction parameters
	private const string IDLE = "idle";
	private const string TAP = "tap";
	private string _touchState = IDLE;
	private float _touchTimer = 0;
	private float _doubleTimer = 0;
	private Vector3 _startPos;
	public const float TAP_TIMEOUT = 0.1f;
	public const float D_TAP_TIMEOUT = 0.2f;
	public const float SWIPE_THRESH = 50;

	//Sort use instead of constructor
	void Awake(){
		_instance = this;
	}

	// Use this for initialization
	void Start(){
		_camMgr = CameraManager.GetInstance();
	}
	
	// Update is called once per frame
	void Update(){
		//Mobile interaction
		UpdatePan(_camMgr.camera);
		UpdatePinch(_camMgr.camera);
		
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

		if(_touchState == TAP) {
			_doubleTimer += Time.deltaTime;
			if(_doubleTimer > D_TAP_TIMEOUT){
				OnTap(_startPos);
				_doubleTimer = 0;
				_touchState = IDLE;
			}
		}
	}

	//
	private void OnTouchStart(Vector3 pos){
		_touchTimer = 0;
		if(_touchState == IDLE) {
			_startPos = pos;
		}
	}
	
	//
	private void OnTouch(Vector3 pos){
		_touchTimer += Time.deltaTime;
	}
	
	//
	private void OnTouchEnded(Vector3 pos){
		float dist = Vector3.Distance(_startPos, pos);

		//Touch ended before tap timeout, trigger OnTap
		if(_touchTimer < TAP_TIMEOUT && dist < SWIPE_THRESH){
			_touchTimer = 0;
			if(_touchState == IDLE){
				_touchState = TAP;
			} else if(_touchState == TAP){
				_touchState = IDLE;
				_doubleTimer = 0;
				OnDoubleTap(pos);
			}
//			OnTap(pos);
		} else if(dist >= SWIPE_THRESH) {
			OnSwipe(_startPos, pos);
		}
	}
	
	//
	private void OnTap(Vector3 pos){
		RaycastHit hit;
		Ray ray = Camera.main.ScreenPointToRay(pos);

		Debug.Log("Tapped at " + pos);
		
		if(Physics.Raycast(ray, out hit)){  
			Transform selected = hit.transform;
			Debug.Log("hit: " + selected.tag);

			if(selected.GetComponent<Interactable>()){
				InspectorUI.SetActive(true);
			}
		}
	}

	//
	private void OnDoubleTap(Vector3 pos){
		Debug.Log("Double tapped at " + pos);
	}

	//
	private void OnSwipe(Vector3 start, Vector3 end){
//		float dir = Vector3.Angle(start, end);
		float dir = Mathf.Atan2(end.y - start.y, end.x - start.x);
		dir = (dir * Mathf.Rad2Deg + 360) % 360;
		float dist = Vector3.Distance(start, end);

		float dirMod = (dir + 90) % 360;
		if(dirMod > 45 && dirMod <= 135){
			_camMgr.PrevViewpoint();
		} else if(dir > 45 && dir <= 135){
			Debug.Log("swipe up, " + dir);
		} else if(dir > 135 && dir <= 225){
			_camMgr.NextViewpoint();
		} else if(dir > 225 && dir <= 315){
			Debug.Log("swipe down, " + dir);
		}
	}
	
	//
	public void OnButtonTap(string button){
		switch(button){
		case "close_inspector":
			InspectorUI.SetActive(false);
			break;
		default:
			break;
		}
	}

	//

	//
	public void UpdatePan(Camera camera){
		Vector3 camPos = camera.transform.position;
		Transform camTransform = camera.transform;
		
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
		
		camera.transform.position = camPos;
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
				camera.orthographicSize += deltaMagnitudeDiff * _camMgr.orthoZoomSpeed;
				camera.orthographicSize = Mathf.Max(camera.orthographicSize, 0.1f);
			} else {
				camera.fieldOfView += deltaMagnitudeDiff * _camMgr.perspectiveZoomSpeed;
				camera.fieldOfView = Mathf.Clamp(camera.fieldOfView, 0.1f, 179.9f);
			}
		}
	}
}
