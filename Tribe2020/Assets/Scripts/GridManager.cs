﻿using UnityEngine;
using System.Collections;

public class GridManager : MonoBehaviour {
	public const int xMax = 10, yMax = 10, zMax = 10;
	public const int offset = -5;
	public GameObject[,,] cells;
	public GameObject cell, floor, campfire;
	public ParticleSystem pm;
	private ParticleSystem.Particle[] _ps;

	public enum Block {Void, Empty, Floor, Campfire};

	// Use this for initialization
	void Start(){
		cell = GameObject.Find ("ent_cell");
		floor = GameObject .Find ("ent_block");
		campfire = GameObject.Find ("ent_campfire");

		_ps = new ParticleSystem.Particle[this.pm.particleCount];
		pm.GetParticles(_ps);

		cells = new GameObject[xMax, yMax, zMax];
		for(int x = 0; x < xMax; x++){
			for(int y = 0; y < yMax; y++){
				for(int z = 0; z < zMax; z++){

					Vector3 pos = new Vector3(x, y, z) * 5;

					GameObject newCell = Instantiate(cell, pos, Quaternion.identity) as GameObject;

					cells[x, y, z] = newCell;
					cells[x, y, z].GetComponent<Cell>().Init(pos);

					if(x > 0 && y > 0 && z > 0 && x < xMax-1 && y < yMax-1 && z < zMax-1){
						cells[x, y, z].GetComponent<Cell>().SetType(Block.Empty);
					} else{
						cells[x, y, z].GetComponent<Cell>().SetType(Block.Void);
					}
				}
			}
		}
	}
	
	// Update is called once per frame
	void Update(){
//		Vector3 pos = _ps [0].position;
//		pos.y = 10;
//		_ps[0].position = pos;
	}

	public void AddBlock(int x, int y, int z, Block type){
		if(x > offset && x < xMax-offset &&
		   y > offset && y < yMax-offset &&
		   z > offset && z < zMax-offset){
			Vector3 pos = new Vector3(x * 5, y * 5, z * 5);
			GameObject blockObj = TypeToObject(type);
			GameObject newBlock = (GameObject)Instantiate(
				blockObj, pos, Quaternion.identity);
			cells[x, y, z].GetComponent<Cell>().SetBlock(newBlock);
		}
	}

	public void RemoveBlock(GameObject obj){
		Vector3 pos = obj.transform.position / 5;

		cells[(int)pos.x, (int)pos.y, (int)pos.z].GetComponent<Cell>().Reset();
	}

//	public GameObject CollidesWithBlock(int x, int y, int z, GameObject otherObj){
//		Collider col = otherObj.GetComponent<Collider>();
//		Bounds pb = col.bounds;
//		foreach(GameObject b in blocks){
//			Bounds bb = b.GetComponent<Collider>().bounds;
//			if(pb.Intersects(bb)){
//				return b;
//			}
//		}
//	}

	public GameObject GetBlock(int x, int y, int z){
		return cells[x, y, z].GetComponent<Cell>().block;
	}

	public void ResetParticles(){
		pm.SetParticles(_ps, _ps.Length);
	}

	public static GameObject TypeToObject(Block type){
		switch (type) {
		case Block.Floor:
			return GameObject.Find ("ent_block");
		case Block.Campfire:
			return GameObject.Find ("ent_campfire");
		default:
			return null;
		}
	}
}
