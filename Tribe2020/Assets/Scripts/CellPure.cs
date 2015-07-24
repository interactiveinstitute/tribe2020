using UnityEngine;
using System.Collections;

public class CellPure {
	private GridManager.Block _type;
//	private CellPure _nw, _n, _ne, _e, _se, _s, _sw, _w;
	private CellPure[] _neighbours;
	private float _heat;
	private float _heatEmittance;
	private bool _isInitialized = false;

	public CellPure(GridManager.Block type){
		_type = type;
		_heat = Random.value * 255;
		_heatEmittance = -1f;
	}

	// Use this for initialization
	public void Init (
		CellPure nw, CellPure n, CellPure ne, CellPure e,
		CellPure se, CellPure s, CellPure sw, CellPure w) {

		_neighbours = new CellPure[8]{nw, n, ne, e, se, s, sw, w};

		_isInitialized = true;
	}
	
	// Update is called once per frame
	public void Update () {
		if (_isInitialized) {
			float total = 0;
			foreach(CellPure c in _neighbours){
				total += c.Heat;
			}

			foreach(CellPure c in _neighbours){
				if(c.Heat < this.Heat){
					c.Heat = c.Heat + 1;
					this.Heat = this.Heat - 1;
				} else{ 
					c.Heat = c.Heat - 1;
					this.Heat = this.Heat + 1;
				}
			}

			if(_heatEmittance != -1){
				this.Heat = _heatEmittance;
			}
		}
	}

	public void SetType(GridManager.Block type){
		_type = type;
	}

	public GridManager.Block GetType(){
		return _type;
	}

	public float Heat{
		get { return _heat;}
		set { _heat = value;}
	}
}
