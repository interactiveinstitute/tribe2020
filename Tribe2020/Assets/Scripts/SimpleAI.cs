using UnityEngine;
using System.Collections;

public class SimpleAI : MonoBehaviour {
	private NavMeshAgent _agent;
	private float _waterLevel;

	private Transform _toilet, _coffee;

	// Use this for initialization
	void Start () {
		_agent = GetComponent<NavMeshAgent> ();
	}
	
	// Update is called once per frame
	void Update () {
		if (_waterLevel > 0) {
			_waterLevel -= Time.deltaTime * 100;
		}

		if (_waterLevel < 100) {
			GameObject coffee = GameObject.FindGameObjectsWithTag("coffee")[0] as GameObject;
			_agent.SetDestination (coffee.transform.position);
		}

		Debug.Log(_waterLevel);

//		_agent.SetDestination (new Vector3 (20f, 0f, 20f));
//		if (_agent.isPathStale) {
//			_agent.SetDestination (new Vector3 (0f, 0f, 0f));
//		}
	}
}
