using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GridManager : MonoBehaviour {
	public const int xMax = 150, yMax = 50, zMax = 150;
	private int _curUpdateLayer = 0;
	public const int offset = 0;
	public GameObject[,,] cells;
	private CellPure[,,] cellPures;
	public GameObject PREFAB_CELL, PREFAB_FLOOR, PREFAB_FIRE;
	public ParticleSystem pm;
	private ParticleSystem.Particle[] _ps;

	public enum Block {Void, Empty, Floor, Campfire};

	// Use this for initialization
	void Start(){
		PREFAB_CELL = GameObject.Find("ent_cell");

		PREFAB_FLOOR = GameObject.Find("ent_block");
		PREFAB_FIRE = GameObject.Find("ent_campfire");

		_ps = new ParticleSystem.Particle[this.pm.particleCount];
		pm.GetParticles(_ps);

		cells = new GameObject[xMax, yMax, zMax];
		cellPures = new CellPure[xMax, yMax, zMax];

//		for(int x = 0; x < 20; x++){
//			for(int y = 0; y < 20; y++){
//				float rX = x + y - x % 2;
//				float rZ = x;
//
//				Vector3 pos = new Vector3(rX, 0, rZ) * 5;
//				
//
//				GameObject uiCell = GameObject.Find ("UICell");
//				GameObject energyUI =
//					Instantiate(uiCell, pos, Quaternion.identity) as GameObject;
//			}
//		}

		for(int x = 0; x < xMax; x++){
			for(int y = 0; y < yMax; y++){
				for(int z = 0; z < zMax; z++){
//					Vector3 pos = new Vector3(x, y, z) * 5;

//					
//
//					GameObject newCell =
//						Instantiate(cell, pos, Quaternion.identity) as GameObject;
//
//					cells[x, y, z] = newCell;
//					cells[x, y, z].GetComponent<Cell>().Init();
//
					if(x > offset && y > offset && z > offset &&
					   x < xMax-1 && y < yMax-1 && z < zMax-1){
						cellPures[x, y, z] = new CellPure(Block.Empty);
//						SetType(new Vector3(x, y, z), Block.Empty);
					} else{
						cellPures[x, y, z] = new CellPure(Block.Void);
//						SetType(new Vector3(x, y, z), Block.Void);
					}
				}
			}
		}

		//Set Neighbours
		for (int x = 0; x < xMax; x++) {
			for (int y = 0; y < yMax; y++) {
				for (int z = 0; z < zMax; z++) {
					if(cellPures[x, y, z].GetType() != Block.Void){
						cellPures[x, y, z].Init(
							cellPures[x - 1, y, z + 1], //NW
							cellPures[x, y, z + 1], //N
							cellPures[x + 1, y, z + 1], //NE
							cellPures[x + 1, y, z], //E
							cellPures[x + 1, y, z - 1], //SE
							cellPures[x, y, z - 1], //S
							cellPures[x - 1, y, z - 1], //SW
							cellPures[x - 1, y, z]  //W
						);
					}
				}
			}
		}
	}

	// Update is called once per frame
	void Update(){
		_curUpdateLayer = (_curUpdateLayer + 1) % yMax;

		for(int x = 0; x < xMax; x++){
			for(int z = 0; z < zMax; z++){
				cellPures[x, _curUpdateLayer, z].Update();
			}
		}

//		Vector3 pos = _ps [0].position;
//		pos.y = 10;
//		_ps[0].position = pos;
	}

	public void SetType(Vector3 cellCoord, Block type){
		int x = (int)cellCoord.x;
		int y = (int)cellCoord.y;
		int z = (int)cellCoord.z;

//		if(x >= offset && y >= offset && z >= offset &&
//		   x < xMax && y < yMax && z < zMax){
//			Cell cell = cells[x, y, z].GetComponent<Cell>();
//			if(cells[x, y, z].GetComponent<Cell>().GetType() != Block.Void){
//				cells[x, y, z].GetComponent<Cell>().SetType(type);
//			}
//		} else{
//		}

		if(x >= offset && y >= offset && z >= offset &&
		   x < xMax && y < yMax && z < zMax){
			CellPure cell = cellPures[x, y, z];
			if(cellPures[x, y, z].GetType() != Block.Void){
				cellPures[x, y, z].SetType(type);
			}
		} else{
		}
	}

	public void SetType(List<Vector3> cellCoords, Block type){
		foreach (Vector3 cellCoord in cellCoords) {
//			Debug.Log ("SetType: " + cellCoord.x + ","+cellCoord.y+","+cellCoord.z);
			SetType (cellCoord, type);
		}
	}

	public Block GetType(int x, int y, int z){
//		print (x + ";" + y + ";" + z + ": " + cells[x, y, z].GetComponent<Cell>().GetType());
		if (IsWithinBounds (x, y, z)) {
			return cellPures [x, y, z].GetType ();
		}

		return Block.Void;
	}

	public Block GetType(Vector3 pos){
		int x = (int)pos.x;
		int y = (int)pos.y;
		int z = (int)pos.z;

		return GetType (x, y, z);

//		if (IsWithinBounds (x, y, z)) {
//			return cellPures [x, y, z].GetType ();
//		}
//
//		return Block.Void;
	}

	public float GetHeat(Vector3 pos){
		int x = (int)pos.x;
		int y = (int)pos.y;
		int z = (int)pos.z;

		if (IsWithinBounds (x, y, z)) {
			return cellPures [x, y, z].Heat;
		}

		return 0f;
	}

	private bool IsWithinBounds(int x, int y, int z){
		return x >= offset && y >= offset && z >= offset &&
			x < xMax && y < yMax && z < zMax;
	}


//	public GameObject GetBlock(int x, int y, int z){
//		return cells[x, y, z];
////		return cells[x, y, z].GetComponent<Cell>().block;
//	}

//
//	public void RemoveBlock(GameObject obj){
//		Vector3 pos = obj.transform.position / 5;
//		
//		cells[(int)pos.x, (int)pos.y, (int)pos.z].GetComponent<Cell>().Reset();
//	}

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

//	public void ResetParticles(){
//		pm.SetParticles(_ps, _ps.Length);
//	}

//	public static GameObject TypeToObject(Block type){
//		switch (type) {
//		case Block.Floor:
//			return GameObject.Find ("ent_block");
//		case Block.Campfire:
//			return GameObject.Find ("ent_campfire");
//		default:
//			return null;
//		}
//	}
}
