using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//This is the model for a cell within the energy simulation
public class SimulationVolume{
	public enum DIRECTIONS {UP, DOWN, NORTH, SOUTH, EAST, WEST};
	public enum TYPE {VOID, FACE, VOLUME};
	protected SimulationCell[] _faces;

	private SimulationManager _simMgr;
	private SimulationManager.Block _type;
	private SimulationVolume[] _neighbours;
//	private Dictionary<string, SimulationFace> _faces;

	private float _heat;
	private float _heatEmittance;
	private float _transmission;

	//Properties
	private float _energy;
	private float _mass;
	private float _constant;

//	private 

	private float _xTransfer, _yTransfer, zTransfer; //NS, UD, EW

	private bool _isInitialized = false;

	public SimulationVolume(){
	}


	//TODO Orientations
	//We have various orientations of wall as well as if they are a type
	//of cross section (|, L, T, +)

	// Use this for initialization
	public void Start(SimulationCell u, SimulationCell d, SimulationCell n, SimulationCell s
	                  , SimulationCell e, SimulationCell w){
		_faces[(int)DIRECTIONS.UP]	= u;
		_faces[(int)DIRECTIONS.DOWN] = d;
		_faces[(int)DIRECTIONS.NORTH]= n;
		_faces[(int)DIRECTIONS.SOUTH]= s;
		_faces[(int)DIRECTIONS.EAST] = e;
		_faces[(int)DIRECTIONS.WEST] = w;
	}



//	public SimulationVolume(SimulationManager simMgr, SimulationManager.Block type, Vector3 pos){
//		_simMgr = simMgr;
//
//		_type = type;
//		_heat = Random.value * 60;
//		_heat = 20;
//		_heatEmittance = -1f;
//		_transmission = 1f;
//
//		_faces = new Dictionary<string, SimulationFace>();
//		_simMgr.SetFace("up", this, pos + new Vector3 (0, 1, 0));
//
//		_faces.Add("down", 	this, pos + new Vector3(0, -1, 0));
//		_faces.Add("north", _simMgr.GetFace(pos, pos + new Vector3(1, 0, 0)));
//		_faces.Add("south", _simMgr.GetFace(pos, pos + new Vector3(-1, 0, 0)));
//		_faces.Add("west", 	_simMgr.GetFace(pos, pos + new Vector3(0, 0, -1)));
//		_faces.Add("east", 	_simMgr.GetFace(pos, pos + new Vector3(0, 0, 1)));
//	}

	// Use this for initialization
	public void Init(
		SimulationVolume nw, SimulationVolume n, SimulationVolume ne, SimulationVolume e,
		SimulationVolume se, SimulationVolume s, SimulationVolume sw, SimulationVolume w) {
//
////		_neighbours = new SimulationVolume[8]{nw, n, ne, e, se, s, sw, w};
//		_neighbours = new SimulationVolume[4]{n, e, s, w};
//
//		_faces.Add(
//
//		_isInitialized = true;
	}

	// Use this for initialization
	public void InitializeNeigbours(
		SimulationVolume n, SimulationVolume w, SimulationVolume s, SimulationVolume e,
		SimulationVolume up, SimulationVolume down){
		
		//		_neighbours = new SimulationVolume[8]{nw, n, ne, e, se, s, sw, w};
//		_neighbours = new SimulationVolume[6]{n, w, s, e, up, down};

//		_faces.Add ("north", n);
//		_faces.Add ("west", w);
//		_faces.Add ("south", s);
//		_faces.Add ("east", e);
//		_faces.Add ("up", up);
//		_faces.Add ("down", down);
		
		_isInitialized = true;
	}

//	public void SetFace(string direction, SimulationVolume north){
//		_faces.Add(direction, north);
//	}
	
	// This is not an implementation of MonoBehaviours Update, so it needs
	// to be called manually
	public void Update(){
		if (_isInitialized) {
//			float total = 0;
//			foreach(SimulationVolume c in _neighbours){
//				total += c.Heat;
//			}

			foreach(SimulationVolume c in _neighbours){
				float diff = _heat - c.Heat;

				if(diff > 0){ //this volume is hotter
					c.Heat = c.Heat + diff / 5 * c.Transmission;
//					this.Heat = this.Heat - diff / 10 * this.Transmission;
				} else{ //other volume is hotter
					c.Heat = c.Heat + diff / 5 * c.Transmission;
//					this.Heat = this.Heat + diff / 10 * this.Transmission;
				}
			}

			if(_heatEmittance != -1){
				_heat = _heatEmittance;
			}
		}

//		if (!_isInitialized)
//			return null;

//		foreach (SimulationFace face in _faces) {
//			face.Update();
//		}
	}

	// Set the type of block which occupied this cell
	public void SetBlockType(SimulationManager.Block type){
		_type = type;

		if (type == SimulationManager.Block.Campfire) {
			_heatEmittance = 60f;
			_transmission = 0f;
		} else if (type == SimulationManager.Block.Void) {
			_heatEmittance = 0f;
			_transmission = 1f;
		} else if (type == SimulationManager.Block.Empty) {
			_heatEmittance = -1f;
			_transmission = 1f;
		} else if (type == SimulationManager.Block.Wall) {
			_heatEmittance = -1f;
			_transmission = 0.1f;
		} else {
			_heatEmittance = -1f;
			_transmission = 1f;
		}
	}

	public SimulationManager.Block GetBlockType(){
		return _type;
	}

	public float Heat{
		get { return _heat;}
		set { _heat = value;}
	}

	public float Transmission{
		get { return _transmission;}
		set { _transmission = value;}
	}

//	public float Temperature{
//		get { return _temperature;}
//		set { _temperature = value;}
//	}
//
//	public float Isolation{
//		get { return _isolation;}
//		set { _isolation = value;}
//	}
}
