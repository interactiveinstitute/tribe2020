using UnityEngine;
using System.Collections;

public class SimulationFace {
	private SimulationVolume _v1, _v2;

	public SimulationFace(){
	}

	public SimulationFace(SimulationVolume v1, SimulationVolume v2){
		_v1 = v1;
		_v2 = v2;
	}

	// Use this for initialization
	public void Start(){
	
	}

	public void Update(){
		if(_v1.Heat > _v2.Heat){
			_v1.Heat -= 1 * Time.deltaTime;
			_v2.Heat += 1 * Time.deltaTime;
		} else if(_v1.Heat < _v2.Heat){
			_v1.Heat += 1 * Time.deltaTime;
			_v2.Heat -= 1 * Time.deltaTime;
		}
	}
}
