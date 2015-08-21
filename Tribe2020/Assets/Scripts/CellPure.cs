using UnityEngine;
using System.Collections;

//This is the model for a cell within the energy simulation
public class CellPure {
	private GridManager.Block _type;
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
	
	// This is not an implementation of MonoBehaviours Update, so it needs
	// to be called manually
	public void Update () {
		if (_isInitialized) {
			float total = 0;
			foreach(CellPure c in _neighbours){
				total += c.Heat;
			}

			foreach(CellPure c in _neighbours){
				if(c.Heat < this.Heat){
					c.Heat = c.Heat + 3;
					this.Heat = this.Heat - 3;
				} else{ 
					c.Heat = c.Heat - 3;
					this.Heat = this.Heat + 3;
				}
			}

			if(_heatEmittance != -1){
				this.Heat = _heatEmittance;
			}
		}
	}

	// Set the type of block which occupied this cell
	public void SetType(GridManager.Block type){
		_type = type;

		if (type == GridManager.Block.Campfire) {
			_heatEmittance = 255f;
		} else if (type == GridManager.Block.Void) {
			_heatEmittance = 0f;
		} else {
			_heatEmittance = -1f;
		}
	}

	public GridManager.Block GetType(){
		return _type;
	}

	public float Heat{
		get { return _heat;}
		set { _heat = value;}
	}
}
