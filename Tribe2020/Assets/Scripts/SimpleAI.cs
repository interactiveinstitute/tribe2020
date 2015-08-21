using UnityEngine;
using System.Collections;

public class SimpleAI : MonoBehaviour {
	private NavMeshAgent _agent;

	// Use this for initialization
	void Start () {
		_agent = GetComponent<NavMeshAgent> ();
	}
	
	// Update is called once per frame
	void Update () {
		_agent.SetDestination (new Vector3 (20f, 0f, 20f));
		if (_agent.isPathStale) {
			_agent.SetDestination (new Vector3 (0f, 0f, 0f));
		}
	}
}
