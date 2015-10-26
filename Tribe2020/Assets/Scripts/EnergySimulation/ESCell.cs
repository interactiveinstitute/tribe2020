using UnityEngine;
using System.Collections;

public class ESCell{
	public enum FACE {UP = 0, DOWN = 1, NORTH = 2, SOUTH = 3, EAST = 4, WEST = 5};
	protected ESCell[] _faces;

	public enum TYPE {VOID, FACE, VOLUME, EMPTY};
	private TYPE _type;
	private bool _isInitialized;

	//Properties
	private float _energy;
	private float _mass;
	private float _constant;

	private float _thermalEnergy;
	private float _uValue;
	private float _temperature;

	public ESCell(TYPE type){
		_type = type;
		if(_type != TYPE.VOID){
			_faces = new ESCell[6];
		}
		_isInitialized = false;
	}

	// Set Volume
	public void SetVolume(TYPE type){
		_type = type;
	}

	// Use this for initialization
	public void SetFaces(ESCell u, ESCell d, ESCell n, ESCell s, ESCell e, ESCell w){
		_faces[(int)FACE.UP] 	= u;
		_faces[(int)FACE.DOWN] = d;
		_faces[(int)FACE.NORTH]= n;
		_faces[(int)FACE.SOUTH]= s;
		_faces[(int)FACE.EAST] = e;
		_faces[(int)FACE.WEST] = w;

		_isInitialized = true;
	}

	public void SetFace(FACE dir, ESCell face){
		_faces[(int)dir] = face;

		foreach(ESCell f in _faces){
			if(f == null) return;
		}
		
		_isInitialized = true;
	}

	public ESCell GetFace(FACE dir){
		return _faces[(int)dir];
	}

	public void SetCellType(TYPE type){
	}

	public TYPE GetCellType(){
		return _type;
	}

	public void SetFace(FACE dir, TYPE type){
		ESCell face = _faces[(int)dir];
		face.SetCellType(type);
	}
	
	// Update is called once per frame
	public void Update(){
		if(_type == TYPE.VOID || _type == TYPE.EMPTY || !_isInitialized)
			return;

		if(_type == TYPE.FACE){
			foreach(ESCell other in _faces){
				if(this.Energy > other.Energy){
					this.Energy -= 1 * Time.deltaTime;
					other.Energy += 1 * Time.deltaTime;
				} else if(this.Energy < other.Energy){
					this.Energy += 1 * Time.deltaTime;
					other.Energy -= 1 * Time.deltaTime;
				}
			}
		} else if(_type == TYPE.VOLUME){
			foreach(ESCell other in _faces){
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
