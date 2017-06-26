using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractionManager : MonoBehaviour {
	private static InteractionManager _instance;
	public static InteractionManager GetInstance() {
		return _instance;
	}

	private InteractionListener _listener;

	//Interaction props
	[SerializeField]
	private string _touchState = IDLE;
	private float _touchTimer = 0;
	private float _doubleTimer = 0;
	private Vector3 _startPos;
	private bool _isPinching = false;
	private bool _touchReset = false;

	//Interaction consts
	private const string IDLE = "idle";
	private const string TAP = "tap";
	public const float TAP_TIMEOUT = 0.1f;
	public const float D_TAP_TIMEOUT = 0.2f;
	public const float SWIPE_THRESH = 200;

	//
	void Awake() {
		_instance = this;	
	}

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		UpdateTouch();
		UpdatePinch();
	}

	//
	public void SetListener(InteractionListener listener) {
		_listener = listener;
	}

	//Updates basic onStart, onTouch, onEnd, tap, double tap and swipe interaction
	private void UpdateTouch() {
		if(!_touchReset) {
			//Touch start
			if(Input.GetMouseButtonDown(0)) {
				OnTouchStart(Input.mousePosition);
			}

			//Touch ongoing
			if(Input.GetMouseButton(0)) {
				OnTouch(Input.mousePosition);
			}

			//Touch end
			if(Input.GetMouseButtonUp(0)) {
				OnTouchEnded(Input.mousePosition);
			}

			//Delay tap for possibility to double tap
			if(_touchState == TAP) {
				_doubleTimer += Time.unscaledDeltaTime;
				if(_doubleTimer > D_TAP_TIMEOUT) {
					OnTap(_startPos);
					ResetTouch();
				}
			}
		} else {
			_touchReset = false;
		}
	}

	//New touch started
	private void OnTouchStart(Vector3 pos) {
		if(_touchState == IDLE) {
			_touchTimer = 0;
			_startPos = pos;
		}
	}

	//Touch ongoing
	private void OnTouch(Vector3 pos) {
		_touchTimer += Time.unscaledDeltaTime;
	}

	//Touch ended
	private void OnTouchEnded(Vector3 pos) {
		float dist = Vector3.Distance(_startPos, pos);
		if(!_touchReset) {
			if(_touchTimer < TAP_TIMEOUT && dist < SWIPE_THRESH) {
				if(_touchState == IDLE) {
					//First tap, start double tap timer
					_touchState = TAP;
					_touchTimer = 0;
				} else if(_touchState == TAP) {
					//Second tap before double tap timer ran out, trigger double tap
					OnDoubleTap(pos);
					ResetTouch();
				}
			} else if(dist >= SWIPE_THRESH) {
				//Swipe distance greater than threshold, trigger swipe
				OnSwipe(_startPos, pos);
				ResetTouch();
			}
		}
	}

	//Callback for when tap is triggered
	private void OnTap(Vector3 pos) {
		_listener.OnTap(pos);

		_touchState = IDLE;
	}

	//Callback for when double tap triggered
	private void OnDoubleTap(Vector3 pos) {
		_touchState = IDLE;
	}

	//Callback for when swipe triggered
	private void OnSwipe(Vector3 start, Vector3 end) {
		float dir = Mathf.Atan2(end.y - start.y, end.x - start.x);
		dir = (dir * Mathf.Rad2Deg + 360) % 360;
		float dist = Vector3.Distance(start, end);

		float dirMod = (dir + 90) % 360;
		if(dirMod > 45 && dirMod <= 135) {
			_listener.OnSwipe(Vector2.right);
		} else if(dir > 45 && dir <= 135) {
			_listener.OnSwipe(Vector2.up);
		} else if(dir > 135 && dir <= 225) {
			_listener.OnSwipe(Vector2.left);
		} else if(dir > 225 && dir <= 315) {
			_listener.OnSwipe(Vector2.down);
		}

		_touchState = IDLE;
	}

	//
	public void OnPinchIn() {
	}

	//
	public void OnPinchOut() {
	}

	//
	public void OnPinching(float magnitude) {
	}

	//
	public void ResetTouch() {
		_touchTimer = 0;
		_doubleTimer = 0;
		_touchState = IDLE;
		_touchReset = true;
	}

	//
	public void UpdatePinch() {
		if(Input.touchCount == 2) {
			_isPinching = true;

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

			OnPinching(deltaMagnitudeDiff);
		} else if(_isPinching) {
			_isPinching = false;

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
			if(deltaMagnitudeDiff > 0) {
				OnPinchOut();
			} else {
				OnPinchIn();
			}
		}
	}
}
