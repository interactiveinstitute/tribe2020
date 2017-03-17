using UnityEngine;
using System.Collections;

public class SimulationObject : MonoBehaviour {

	//
	virtual public bool UpdateSim(double time) {
		return false;
	}
}
