using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class SerialObject{
//	private Vector3 _position;
	private float _posX, _posY, _posZ;
//	private Quaternion _rotation;
	private float _rotX, _rotY, _rotZ, _rotW;
//	private Vector3 _scale;
	private float _scaleX, _scaleY, _scaleZ;
	private string _tag;
	
	public SerialObject(GameObject go){
//		_position = go.transform.position;
		_posX = go.transform.position.x;
		_posY = go.transform.position.y;
		_posZ = go.transform.position.z;

//		_rotation = go.transform.rotation;
		_rotX = go.transform.rotation.x;
		_rotY = go.transform.rotation.y;
		_rotZ = go.transform.rotation.z;
		_rotW = go.transform.rotation.w;

//		_scale = go.transform.localScale;
		_scaleX = go.transform.localScale.x;
		_scaleY = go.transform.localScale.y;
		_scaleZ = go.transform.localScale.z;

		_tag = go.tag;
	}

	public GameObject ToGameObject(){
		GameObject go = new GameObject();
		Vector3 pos = new Vector3(_posX, _posY, _posZ);
		Quaternion rot = new Quaternion(_rotX, _rotY, _rotZ, _rotW);
		Vector3 scale = new Vector3(_scaleX, _scaleY, _scaleZ);

		go.transform.position = pos;
		go.transform.rotation = rot;
		go.transform.localScale = scale;

		return go;
	}

	public string GetTag(){
		return _tag;
	}
}
