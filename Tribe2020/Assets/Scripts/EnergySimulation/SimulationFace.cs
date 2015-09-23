using UnityEngine;
using System.Collections;

public class SimulationFace{
	private SimulationVolume _v1, _v2;

	public SimulationFace(){
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
