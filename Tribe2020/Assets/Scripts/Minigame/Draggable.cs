using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Draggable : MonoBehaviour, IPointerDownHandler, IPointerUpHandler {
	public bool restrictX = false;
	public bool restrictY = false;

	[SerializeField]
	private bool _isDragged = false;
	private Rigidbody2D _rb;
	private SpringJoint2D _spring;
	private float _origGravity = 0;

	private List<Vector3> _distSamples;
	private Vector3 _lastPos;

	
	// Use this for initialization
	void Start () {
		_rb = GetComponent<Rigidbody2D>();
		if(_rb) {
			_origGravity = _rb.gravityScale;
		}
		_spring = GetComponent<SpringJoint2D>();
		_distSamples = new List<Vector3>();
		//if(_spring) {
		//	//_spring.enabled = false;
		//	_spring.connectedAnchor = transform.position;
		//}
	}
	
	// Update is called once per frame
	void Update () {
		if(_isDragged) {
			Vector2 newPos = Input.mousePosition;
			if(restrictX) { newPos.x = transform.position.x; }
			if(restrictY) { newPos.y = transform.position.y; }

			transform.position = newPos;

			if(_rb) {
				Vector3 diff = Input.mousePosition - _lastPos;
				//Debug.Log(diff);
				_distSamples.Add(diff);
				if(_distSamples.Count > 5) {
					_distSamples.RemoveAt(0);
				}
				_lastPos = Input.mousePosition;

				//Vector3 meanDist = Vector3.zero;
				//foreach(Vector3 distSample in _distSamples) {
				//	meanDist += distSample;
				//}
				//meanDist /= _distSamples.Count;
				//meanDist.x = -meanDist.x;
				//_rb.AddForce(-meanDist * 100);

				//Vector3 touchPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
				//float dist = Vector3.Distance(Input.mousePosition, transform.position);
				//_rb.AddForce((Input.mousePosition - transform.position) * dist * .2f);
			}
		}
	}

	//
	public void OnPointerClick(PointerEventData eventData) {
	}

	//
	public void OnPointerDown(PointerEventData eventData) {
		_isDragged = true;
		if(_rb) {
			_lastPos = Input.mousePosition;
			_distSamples.Clear();
			_rb.gravityScale = 0;
			_rb.velocity = Vector2.zero;
		}
		//if(_spring) {
		//	//_rb.gravityScale = 0;
		//	//_spring.enabled = true;
		//}
	}

	//
	public void OnPointerUp(PointerEventData eventData) {
		_isDragged = false;
		if(_rb) {
			_rb.gravityScale = _origGravity;

			Vector3 meanDist = Vector3.zero;
			foreach(Vector3 distSample in _distSamples) {
				meanDist += distSample;
			}
			meanDist /= _distSamples.Count;

			//Debug.Log(meanDist);

			_rb.AddForce(meanDist * 1000);
		}
		//if(_spring) {
		//	//_spring.enabled = false;
		//}
	}

	//
	public bool IsDragged() {
		return _isDragged;
	}
}
