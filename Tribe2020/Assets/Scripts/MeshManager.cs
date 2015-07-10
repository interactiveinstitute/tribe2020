﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MeshManager : MonoBehaviour {
	public GameObject PREFAB_CELL, PROTO_FLOOR, PREFAB_FIRE;
	private List<GameObject> _meshes;
	
	// Use this for initialization
	void Start () {
		PROTO_FLOOR = GameObject.Find("ent_block");
		_meshes = new List<GameObject>();
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void AddMesh(Vector3 start, Vector3 end, GridManager.Block type){
		Vector3 pos = start + (end - start) / 2;
		GameObject newMesh = Instantiate(PROTO_FLOOR, pos, Quaternion.identity) as GameObject;
		float scaleX = Mathf.Max (start.x, end.x) - Mathf.Min (start.x, end.x) + 5;
		float scaleZ = Mathf.Max (start.z, end.z) - Mathf.Min (start.z, end.z) + 5;
		newMesh.transform.localScale = new Vector3(scaleX, 5, scaleZ);
		_meshes.Add (newMesh);
	}

	public GameObject CollidesWithBlock(GameObject otherObj){
//		Collider col = otherObj.GetComponent<Collider>().bounds;
		Bounds pb = otherObj.GetComponent<Collider>().bounds;
		foreach(GameObject b in _meshes){
			Bounds bb = b.GetComponent<Collider>().bounds;
			if(pb.Intersects(bb)){
				return b;
			}
		}
		return null;
	}

	public void DestroyMesh(GameObject mesh){
		_meshes.Remove (mesh);
		Destroy (mesh);
		//		Vector3 pos = obj.transform.position / 5;
		//		
		//		cells[(int)pos.x, (int)pos.y, (int)pos.z].GetComponent<Cell>().Reset();
		//	}
	}
}
