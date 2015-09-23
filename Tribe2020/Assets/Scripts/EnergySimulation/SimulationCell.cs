using UnityEngine;
using System.Collections;

public class SimulationCell{
	public enum FACE {UP = 0, DOWN = 1, NORTH = 2, SOUTH = 3, EAST = 4, WEST = 5};
	protected SimulationCell[] _faces;

	public enum TYPE {VOID, FACE, VOLUME, EMPTY};
	private TYPE _type;
	private bool _isInitialized;

	//Properties
	private float _energy;
	private float _mass;
	private float _constant;

	public SimulationCell(TYPE type){
		_type = type;
		if(_type != TYPE.VOID){
			_faces = new SimulationCell[6];
		}
		_isInitialized = false;
	}

	// Use this for initialization
	public void SetFaces(SimulationCell u, SimulationCell d, SimulationCell n, SimulationCell s
	           , SimulationCell e, SimulationCell w){
		_faces[(int)FACE.UP] 	= u;
		_faces[(int)FACE.DOWN] = d;
		_faces[(int)FACE.NORTH]= n;
		_faces[(int)FACE.SOUTH]= s;
		_faces[(int)FACE.EAST] = e;
		_faces[(int)FACE.WEST] = w;

		_isInitialized = true;
	}

	public void SetFace(int dir, SimulationCell face){
		_faces[dir] = face;

		foreach(SimulationCell f in _faces){
			if(f == null) return;
		}
		
		_isInitialized = true;
	}

	public void SetType(TYPE type){
	}

	public void SetFace(FACE dir, TYPE type){
		SimulationCell face = _faces[(int)dir];
		face.SetType(type);
	}
	
	// Update is called once per frame
	public void Update(){
		if(_type == TYPE.VOID || _type == TYPE.EMPTY || !_isInitialized)
			return;

		if(_type == TYPE.FACE){
			foreach(SimulationCell other in _faces){
				if(this.Energy > other.Energy){
					this.Energy -= 1 * Time.deltaTime;
					other.Energy += 1 * Time.deltaTime;
				} else if(this.Energy < other.Energy){
					this.Energy += 1 * Time.deltaTime;
					other.Energy -= 1 * Time.deltaTime;
				}
			}
		} else if(_type == TYPE.VOLUME){
			foreach(SimulationCell other in _faces){
				if(this.Energy > other.Energy){
					this.Energy -= 1 * Time.deltaTime;
					other.Energy += 1 * Time.deltaTime;
				} else if(this.Energy < other.Energy){
					this.Energy += 1 * Time.deltaTime;
					other.Energy -= 1 * Time.deltaTime;
				}
			}
		}
	}

	public float Energy{
		get { return _energy;}
		set { _energy = value;}
	}
}
