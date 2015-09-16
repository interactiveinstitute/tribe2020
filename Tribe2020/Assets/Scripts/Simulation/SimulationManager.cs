using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SimulationManager : MonoBehaviour {
	public const int xMax = 150, yMax = 5, zMax = 150;
	private int _curUpdateLayer = 0;
	public const int offset = 0;
	private SimulationVolume[,,] _simVolumes;
	public GameObject PREFAB_CELL, PREFAB_FLOOR, PREFAB_FIRE;
	private ParticleSystem.Particle[] _ps;

	public enum Block {Void, Empty, Floor, Wall, Campfire, Coffee, Toilet};

	private Dictionary<string, SimulationFace> _worldFaces;

	// Use this for initialization
	void Start(){
		PREFAB_CELL = GameObject.Find("ent_cell");

		PREFAB_FLOOR = GameObject.Find("ent_block");
		PREFAB_FIRE = GameObject.Find("ent_campfire");

		_simVolumes = new SimulationVolume[xMax, yMax, zMax];

		_worldFaces = new Dictionary<string, SimulationFace> ();
		_worldFaces.Add ("up", new SimulationFace ());
		_worldFaces.Add ("down", new SimulationFace ());
		_worldFaces.Add ("north", new SimulationFace ());
		_worldFaces.Add ("south", new SimulationFace ());
		_worldFaces.Add ("east", new SimulationFace ());
		_worldFaces.Add ("west", new SimulationFace ());

		SimulationVolume world = new SimulationVolume();
		SimulationFace up = new SimulationFace();
		SimulationFace down = new SimulationFace();
		SimulationFace north = new SimulationFace();
		SimulationFace south = new SimulationFace();
		SimulationFace west = new SimulationFace();
		SimulationFace east = new SimulationFace();

		//Instantiate all volumes and their faces to each other
//		for(int x = 0; x < xMax; x++){
//			for(int y = 0; y < yMax; y++){
//				for(int z = 0; z < zMax; z++){
//					if(x > offset && y > offset && z > offset &&
//					   x < xMax-1 && y < yMax-1 && z < zMax-1){
//						_simVolumes[x, y, z] = new SimulationVolume();
//					} else{
//						_simVolumes[x, y, z] = new SimulationVolume(Block.Void);
//					}
//				}
//			}
//		}

		//Create and connect faces between all volumes
//		for (int x = 0; x < xMax; x++) {
//			for (int y = 0; y < yMax; y++) {
//				for (int z = 0; z < zMax; z++) {
////					SimulationVolume curVol = _simVolumes[x, y, z];
////					//North
////					if(x == 0){
////						curVol.SetFace("north", world);
////					} else{
////						curVol.SetFace("north", _simVolumes[x - 1, y, z]);
////					}
////					//South
////					if(x == xMax){
////						curVol.SetFace("south", world);
////					} else{
////						curVol.SetFace("south", _simVolumes[x + 1, y, z]);
////					}
////					//Bottom
////					if(y == 0){
////						curVol.SetFace("bottom", world);
////					} else{
////						curVol.SetFace("south", _simVolumes[x, y - 1, z]);
////					}
////					//Top
////					if(y == yMax){
////						curVol.SetFace("top", world);
////					} else{
////						curVol.SetFace("south", _simVolumes[x, y + 1, z]);
////					}
////					//West
////					if(z == 0){
////						curVol.SetFace("west", world);
////					} else{
////						curVol.SetFace("south", _simVolumes[x, y, z - 1]);
////					}
////					//East
////					if(z == zMax){
////						curVol.SetFace("east", world);
////					} else{
////						curVol.SetFace("south", _simVolumes[x, y, z + 1]);
////					}
//
////					if(_simVolumes[x, y, z].GetBlockType() != Block.Void){
////						_simVolumes[x, y, z].Init(
////							_simVolumes[x - 1, y, z + 1], //NW
////							_simVolumes[x, y, z + 1], //N
////							_simVolumes[x + 1, y, z + 1], //NE
////							_simVolumes[x + 1, y, z], //E
////							_simVolumes[x + 1, y, z - 1], //SE
////							_simVolumes[x, y, z - 1], //S
////							_simVolumes[x - 1, y, z - 1], //SW
////							_simVolumes[x - 1, y, z]  //W
////							);
////					}
//				}
//			}
//		}

		//add all top faces
		//add n face
		//

//		//Instantiate all cells, void if at border and empty otherwise
//		for(int x = 0; x < xMax; x++){
//			for(int y = 0; y < yMax; y++){
//				for(int z = 0; z < zMax; z++){
//					if(x > offset && y > offset && z > offset &&
//					   x < xMax-1 && y < yMax-1 && z < zMax-1){
//						_simVolumes[x, y, z] = new SimulationVolume(Block.Empty);
//					} else{
//						_simVolumes[x, y, z] = new SimulationVolume(Block.Void);
//					}
//				}
//			}
//		}
//
//		//Set Neighbours
//		for (int x = 0; x < xMax; x++) {
//			for (int y = 0; y < yMax; y++) {
//				for (int z = 0; z < zMax; z++) {
//					if(_simVolumes[x, y, z].GetBlockType() != Block.Void){
//						_simVolumes[x, y, z].Init(
//							_simVolumes[x - 1, y, z + 1], //NW
//							_simVolumes[x, y, z + 1], //N
//							_simVolumes[x + 1, y, z + 1], //NE
//							_simVolumes[x + 1, y, z], //E
//							_simVolumes[x + 1, y, z - 1], //SE
//							_simVolumes[x, y, z - 1], //S
//							_simVolumes[x - 1, y, z - 1], //SW
//							_simVolumes[x - 1, y, z]  //W
//						);
//					}
//				}
//			}
//		}
	}

	// Update is called once per frame
	void Update(){
		_curUpdateLayer = (_curUpdateLayer + 1) % yMax;

		//Update one layer at a time to decrease lag
		for(int x = 0; x < xMax; x++){
			for(int z = 0; z < zMax; z++){
				_simVolumes[x, _curUpdateLayer, z].Update();
//				_simVolumes[x, 1, z].Update();
			}
		}
	}

//	public SimulationFace GetFace(SimulationVolume v1, Vector3 v2){
//		SimulationVolume other = GetVolume(v2);
//		if(other =! null){
//			SimulationFace newFace = new SimulationFace(v1, other);
//			v1.
//		}
//		//returns the face between volume1 and volume2
//	}

	public void SetFace(string dir, SimulationVolume vol, Vector3 checkPos){
		SimulationVolume other = GetVolume(checkPos);
		if (other = ! null) {
			SimulationFace newFace = new SimulationFace (vol, other);
			vol.SetFace (dir, other);
			other.SetFace (OppositeFace (dir), vol);
		} else {
			vol.SetFace (_worldFaces[dir]);
		}
	}

	public string OppositeFace(string dir){
		switch (dir) {
		case "up":
			return "down";
		case "down":
			return "up";
		case "north":
			return "south";
		case "south":
			return "north";
		case "east":
			return "west";
		case "west":
			return "east";
		default:
			return "";
		}
	}

	public SimulationVolume GetVolume(Vector3 coord){
		//returns the volume which inlcudes the coord
	}

	//Set type of cell given separate coordinates
	public void SetType(Vector3 cellCoord, Block type){
		int x = (int)cellCoord.x;
		int y = (int)cellCoord.y;
		int z = (int)cellCoord.z;

		if(x >= offset && y >= offset && z >= offset &&
		   x < xMax && y < yMax && z < zMax){
//			SimulationVolume cell = _simVolumes[x, y, z];
			if(_simVolumes[x, y, z].GetBlockType() != Block.Void){
				_simVolumes[x, y, z].SetBlockType(type);
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

	//
	public Block GetType(int x, int y, int z){
		if (IsWithinBounds (x, y, z)) {
			return _simVolumes [x, y, z].GetBlockType ();
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
			return _simVolumes [x, y, z].Heat;
		}

		return 0f;
	}

	private bool IsWithinBounds(int x, int y, int z){
		return x >= offset && y >= offset && z >= offset &&
			x < xMax && y < yMax && z < zMax;
	}
}
