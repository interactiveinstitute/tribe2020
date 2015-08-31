using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SimulationManager : MonoBehaviour {
	public const int xMax = 150, yMax = 50, zMax = 150;
	private int _curUpdateLayer = 0;
	public const int offset = 0;
	private SimulationCell[,,] _simCells;
	public GameObject PREFAB_CELL, PREFAB_FLOOR, PREFAB_FIRE;
	private ParticleSystem.Particle[] _ps;

	public enum Block {Void, Empty, Floor, Campfire, Coffee, Toilet};

	// Use this for initialization
	void Start(){
		PREFAB_CELL = GameObject.Find("ent_cell");

		PREFAB_FLOOR = GameObject.Find("ent_block");
		PREFAB_FIRE = GameObject.Find("ent_campfire");

		_simCells = new SimulationCell[xMax, yMax, zMax];

		//Instantiate all cells, void if at border and empty otherwise
		for(int x = 0; x < xMax; x++){
			for(int y = 0; y < yMax; y++){
				for(int z = 0; z < zMax; z++){
					if(x > offset && y > offset && z > offset &&
					   x < xMax-1 && y < yMax-1 && z < zMax-1){
						_simCells[x, y, z] = new SimulationCell(Block.Empty);
					} else{
						_simCells[x, y, z] = new SimulationCell(Block.Void);
					}
				}
			}
		}

		//Set Neighbours
		for (int x = 0; x < xMax; x++) {
			for (int y = 0; y < yMax; y++) {
				for (int z = 0; z < zMax; z++) {
					if(_simCells[x, y, z].GetBlockType() != Block.Void){
						_simCells[x, y, z].Init(
							_simCells[x - 1, y, z + 1], //NW
							_simCells[x, y, z + 1], //N
							_simCells[x + 1, y, z + 1], //NE
							_simCells[x + 1, y, z], //E
							_simCells[x + 1, y, z - 1], //SE
							_simCells[x, y, z - 1], //S
							_simCells[x - 1, y, z - 1], //SW
							_simCells[x - 1, y, z]  //W
						);
					}
				}
			}
		}
	}

	// Update is called once per frame
	void Update(){
		_curUpdateLayer = (_curUpdateLayer + 1) % yMax;

		//Update one layer at a time to decrease lag
		for(int x = 0; x < xMax; x++){
			for(int z = 0; z < zMax; z++){
				_simCells[x, _curUpdateLayer, z].Update();
			}
		}
	}

	//Set type of cell given separate coordinates
	public void SetType(Vector3 cellCoord, Block type){
		int x = (int)cellCoord.x;
		int y = (int)cellCoord.y;
		int z = (int)cellCoord.z;

		if(x >= offset && y >= offset && z >= offset &&
		   x < xMax && y < yMax && z < zMax){
//			SimulationCell cell = _simCells[x, y, z];
			if(_simCells[x, y, z].GetBlockType() != Block.Void){
				_simCells[x, y, z].SetBlockType(type);
			}
		} else{
		}
	}

	//Set type of cell given a 3d coordinate
	public void SetType(List<Vector3> cellCoords, Block type){
		foreach (Vector3 cellCoord in cellCoords) {
			SetType (cellCoord, type);
		}
	}


	public Block GetType(int x, int y, int z){
		if (IsWithinBounds (x, y, z)) {
			return _simCells [x, y, z].GetBlockType ();
		}

		return Block.Void;
	}

	public Block GetType(Vector3 pos){
		int x = (int)pos.x;
		int y = (int)pos.y;
		int z = (int)pos.z;

		return GetType (x, y, z);
	}

	public float GetHeat(Vector3 pos){
		int x = (int)pos.x;
		int y = (int)pos.y;
		int z = (int)pos.z;

		if (IsWithinBounds (x, y, z)) {
			return _simCells [x, y, z].Heat;
		}

		return 0f;
	}

	private bool IsWithinBounds(int x, int y, int z){
		return x >= offset && y >= offset && z >= offset &&
			x < xMax && y < yMax && z < zMax;
	}
}
