using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CircleObject : MonoBehaviour {
	public float _radius = 3;
	private int _segments = 20;
	private float _segLength = 10;
	private List<GameObject> _meshes = new List<GameObject>();

	private float _oldRad = 0;

	// Use this for initialization
	void Start(){


	}
	
	// Update is called once per frame
	void Update(){
		if(_radius != _oldRad) {
			UpdateShape();
			_oldRad = _radius;
		}
	}

	public void UpdateShape(){
		foreach(GameObject go in _meshes) {
			Destroy(go);
		}
		_meshes.Clear();

		_segments = (int) Mathf.Max(10, Mathf.Ceil(_radius));
		_segLength = Mathf.PI * 2f * _radius / _segments + 0.5f;

//		Debug.Log(_segments);
		
		for(int i = 0; i < _segments; i++) {
			_meshes.Add(GameObject.CreatePrimitive(PrimitiveType.Cube));
			float angle = 2f * Mathf.PI / _segments * i;
			float xPos = Mathf.Cos(angle);
			float zPos = Mathf.Sin(angle);
			_meshes[i].transform.localScale = new Vector3(2f, 10f, _segLength);
			_meshes[i].transform.position =
				transform.position + new Vector3(xPos * _radius, 0f, zPos * _radius);
			//			float rotation = Vector3.Angle(transform.position, _meshes[i].transform.position);
			_meshes[i].transform.Rotate(0f, 360f - 360f / _segments * i, 0f);
			_meshes[i].transform.parent = transform;
		}
	}
}