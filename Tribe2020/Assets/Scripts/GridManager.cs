using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GridManager : MonoBehaviour {
	public const int xMax = 150, yMax = 50, zMax = 150;
	private int _curUpdateLayer = 0;
	public const int offset = 0;
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

		cellPures = new CellPure[xMax, yMax, zMax];

		//Instantiate all cells, void if at border and empty otherwise
		for(int x = 0; x < xMax; x++){
			for(int y = 0; y < yMax; y++){
				for(int z = 0; z < zMax; z++){
					if(x > offset && y > offset && z > offset &&
					   x < xMax-1 && y < yMax-1 && z < zMax-1){
						cellPures[x, y, z] = new CellPure(Block.Empty);
					} else{
						cellPures[x, y, z] = new CellPure(Block.Void);
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

		//Update one layer at a time to decrease lag
		for(int x = 0; x < xMax; x++){
			for(int z = 0; z < zMax; z++){
				cellPures[x, _curUpdateLayer, z].Update();
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
			CellPure cell = cellPures[x, y, z];
			if(cellPures[x, y, z].GetType() != Block.Void){
				cellPures[x, y, z].SetType(type);
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
			return cellPures [x, y, z].GetType ();
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
			return cellPures [x, y, z].Heat;
		}

		return 0f;
	}

	private bool IsWithinBounds(int x, int y, int z){
		return x >= offset && y >= offset && z >= offset &&
			x < xMax && y < yMax && z < zMax;
	}
}
