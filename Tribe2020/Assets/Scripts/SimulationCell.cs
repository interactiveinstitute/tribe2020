using UnityEngine;
using System.Collections;

//This is the model for a cell within the energy simulation
public class SimulationCell {
	private SimulationManager.Block _type;
	private SimulationCell[] _neighbours;

	private float _heat;
	private float _heatEmittance;

	private bool _isInitialized = false;

	public SimulationCell(SimulationManager.Block type){
		_type = type;
		_heat = Random.value * 60;
		_heatEmittance = -1f;
	}

	// Use this for initialization
	public void Init (
		SimulationCell nw, SimulationCell n, SimulationCell ne, SimulationCell e,
		SimulationCell se, SimulationCell s, SimulationCell sw, SimulationCell w) {

		_neighbours = new SimulationCell[8]{nw, n, ne, e, se, s, sw, w};

		_isInitialized = true;
	}
	
	// This is not an implementation of MonoBehaviours Update, so it needs
	// to be called manually
	public void Update () {
		if (_isInitialized) {
			float total = 0;
			foreach(SimulationCell c in _neighbours){
				total += c.Heat;
			}

			foreach(SimulationCell c in _neighbours){
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

	// Set the type of block which occupied this cell
	public void SetBlockType(SimulationManager.Block type){
		_type = type;

		if (type == SimulationManager.Block.Campfire) {
			_heatEmittance = 60f;
		} else if (type == SimulationManager.Block.Void) {
			_heatEmittance = 0f;
		} else {
			_heatEmittance = -1f;
		}
	}

	public SimulationManager.Block GetBlockType(){
		return _type;
	}

	public float Heat{
		get { return _heat;}
		set { _heat = value;}
	}
}
