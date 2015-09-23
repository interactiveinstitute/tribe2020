               using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SimulationManager : MonoBehaviour {
	public const int xMax = 150, yMax = 5, zMax = 150;
	private int _curUpdateLayer = 0;
	public const int offset = 0;
	private SimulationVolume[,,] _simVolumes;
//	public GameObject PREFAB_CELL, PREFAB_FLOOR, PREFAB_FIRE;
	private ParticleSystem.Particle[] _ps;

	public enum Block {Void, Empty, Floor, Wall, Campfire, Coffee, Toilet};

	private Dictionary<string, SimulationFace> _worldFaces;
	private SimulationCell[,,] _cells;

	// Use this for initialization
	void Start(){
//		PREFAB_CELL = GameObject.Find("ent_cell");
//
//		PREFAB_FLOOR = GameObject.Find("ent_block");
//		PREFAB_FIRE = GameObject.Find("ent_campfire");

		_simVolumes = new SimulationVolume[xMax, yMax, zMax];
		_cells = new SimulationCell[xMax, yMax, zMax];

//		_worldFaces = new Dictionary<string, SimulationFace> ();
//		_worldFaces.Add("up", new SimulationFace());
//		_worldFaces.Add("down", new SimulationFace());
//		_worldFaces.Add("north", new SimulationFace());
//		_worldFaces.Add("south", new SimulationFace());
//		_worldFaces.Add("east", new SimulationFace());
//		_worldFaces.Add("west", new SimulationFace());

//		SimulationVolume world = new SimulationVolume();
//		SimulationFace up = new SimulationFace();
//		SimulationFace down = new SimulationFace();
//		SimulationFace north = new SimulationFace();
//		SimulationFace south = new SimulationFace();
//		SimulationFace west = new SimulationFace();
//		SimulationFace east = new SimulationFace();

		//Instantiate all cells with respective type
		for(int x = 0; x < xMax; x++){
			for(int y = 0; y < yMax; y++){
				for(int z = 0; z < zMax; z++){
					if(y % 2 == 0) {
						if(x % 2 == 1 && z % 2 == 1) {
							_cells[x, y, z] = new SimulationCell(SimulationCell.TYPE.FACE);
						} else {
							_cells[x, y, z] = new SimulationCell(SimulationCell.TYPE.VOID);
						}
					} else {
						if(x % 2 == 0) {
							if(z % 2 == 0) {
								_cells[x, y, z] = new SimulationCell(SimulationCell.TYPE.VOID);
							} else {
								_cells[x, y, z] = new SimulationCell(SimulationCell.TYPE.FACE);
							}
						} else {
							if(z % 2 == 0) {
								_cells[x, y, z] = new SimulationCell(SimulationCell.TYPE.FACE);
							} else {
								_cells[x, y, z] = new SimulationCell(SimulationCell.TYPE.EMPTY);
							}
						}
					}
				}
			}
		}

		//Set all connections for relevant cells types
		for(int x = 0; x < xMax; x++){
			for(int y = 0; y < yMax; y++){
				for(int z = 0; z < zMax; z++){
//					SimulationCell cell = _cells[x, y, z];
//					cell.SetFace((int)SimulationCell.FACE.UP, 	_cells[x, y + 1, z]);
//					cell.SetFace((int)SimulationCell.FACE.DOWN, _cells[x, y - 1, z]);
//					cell.SetFace((int)SimulationCell.FACE.NORTH, _cells[x + 1, y, z]);
//					cell.SetFace((int)SimulationCell.FACE.SOUTH, _cells[x - 1, y, z]);
//					cell.SetFace((int)SimulationCell.FACE.EAST, _cells[x, y, z + 1]);
//					cell.SetFace((int)SimulationCell.FACE.WEST, _cells[x, y, z - 1]);
				}
			}
		}

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
			for(int y = 0; y < yMax; y++){
				for(int z = 0; z < zMax; z++){
					_cells[x, y, z].Update();
				}
//				_simVolumes[x, _curUpdateLayer, z].Update();
////				_simVolumes[x, 1, z].Update();
			}
		}
	}

	public Vector3 SpaceToSimCoord(Vector3 pos){
		return pos * 2 + new Vector3(1, 1, 1);
	}

	public SimulationCell GetVolume(Vector3 pos){
		Vector3 simPos = SpaceToSimCoord(pos);
		return _cells[(int)simPos.x, (int)simPos.y, (int)simPos.z];
	}

	public void SetVolume(Vector3 pos, SimulationCell.TYPE type){
		GetVolume(pos).SetType(type);
	}

	public void SetFace(Vector3 pos, SimulationCell.FACE face, SimulationCell.TYPE type){
		GetVolume(pos).SetFace(face, type);
	}

	public float GetEnergy(Vector3 pos){
		Vector3 simPos = SpaceToSimCoord(pos);
		return _cells[(int)simPos.x, (int)simPos.y, (int)simPos.z].Energy;
	}

	public void SetRoom(Vector3[][] simCells, string type){
		for(int x = 0; x < simCells.Length; x++){
			for(int z = 0; z < simCells[0].Length; z++){
				//TODO add volumes with faces to each other
				if(x == 0){
					// Special west
				} else if(z == 0){
					//Special south
				} else if(x == simCells.Length - 1){
					//Special east
				} else if(z == simCells[0].Length - 1){
					//Special North
				}
			}
		}
	}

	//TODO
	//Set type of cell given a 3d coordinate
	public void SetType(List<Vector3> cellCoords, Block type){
		foreach (Vector3 pos in cellCoords){
			SetType(pos, type);
		}
	}

	public void SetType(Vector3 pos, Block type){
		int x = (int)pos.x;
		int y = (int)pos.y;
		int z = (int)pos.z;
		
		if(x >= offset && y >= offset && z >= offset &&
		   x < xMax && y < yMax && z < zMax){

			SetVolume(pos, SimulationCell.TYPE.VOLUME);

			//			SimulationVolume cell = _simVolumes[x, y, z];
			if(_simVolumes[x, y, z].GetBlockType() != Block.Void){
				_simVolumes[x, y, z].SetBlockType(type);
			}
		} else{
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

//	public void SetFace(string dir, SimulationVolume vol, Vector3 checkPos){
//		SimulationVolume other = GetVolume(checkPos);
//		if (other = ! null) {
//			SimulationFace newFace = new SimulationFace (vol, other);
//			vol.SetFace (dir, other);
//			other.SetFace (OppositeFace (dir), vol);
//		} else {
//			vol.SetFace (_worldFaces[dir]);
//		}
//	}

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

//	public SimulationVolume GetVolume(Vector3 coord){
//		//returns the volume which inlcudes the coord
//	}

	//Set type of cell given separate coordinates
//	public void SetType(Vector3 cellCoord, Block type){
//		int x = (int)cellCoord.x;
//		int y = (int)cellCoord.y;
//		int z = (int)cellCoord.z;
//
//		if(x >= offset && y >= offset && z >= offset &&
//		   x < xMax && y < yMax && z < zMax){
////			SimulationVolume cell = _simVolumes[x, y, z];
//			if(_simVolumes[x, y, z].GetBlockType() != Block.Void){
//				_simVolumes[x, y, z].SetBlockType(type);
//			}
//		} else{
//		}
//	}

//	//Set type of cell given a 3d coordinate
//	public void SetType(List<Vector3> cellCoords, Block type){
//		foreach (Vector3 cellCoord in cellCoords) {
//			SetType (cellCoord, type);
//		}
//	}

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
