﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SimpleAI : MonoBehaviour {
	private NavMeshAgent _agent;
	private GameObject _curTarget;
	private float _thirst;
	private float _need;
	private float _temperature;

	private List<AgentBehavior> _behaviors = new List<AgentBehavior>();
	private AgentBehavior _curBehavior;

	private Transform _toilet, _coffee;

	private const string COFFEE = "coffee";
	private const string TOILET = "toilet";

	// Use this for initialization
	void Start(){
		_agent = GetComponent<NavMeshAgent> ();
		_thirst = 100;
		_need = 0;
		_temperature = 25;

		_behaviors.Add(new Idle(0.25f));
		_behaviors.Add(new DrinkCoffee(0.25f));
		_behaviors.Add(new UseBathroom(0.25f));
		_behaviors.Add(new WarmUp(0.25f));

		_curBehavior = _behaviors[0];
		_curBehavior.Start();
	}
	
	// Update is called once per frame
	void Update(){
		_curBehavior.Update (this);

		if (_curTarget && Vector3.Distance (transform.position, _curTarget.transform.position) < 10) {
			_curBehavior.OnHasReached(_curTarget.tag);
		}

//		foreach (AgentBehavior b in _behaviors) {
//			b.Update(this);
//		}


//		if(_thirst > 0){
//			_thirst += Time.deltaTime * 100;
//		}
//
//		if(_curTarget &&
//		   Vector3.Distance(this.transform.position, _curTarget.transform.position) < 10){
//			if(_curTarget.tag == COFFEE){
//				_thirst -= 200 * Time.deltaTime;
//				_need +=  100 * Time.deltaTime;
//			} else if(_curTarget.tag == TOILET){
//				_need -= 200 * Time.deltaTime;
//			}
//		}
//
//		if(_thirst > 100){
//			_curTarget = FindNearestObject(COFFEE);
//			_agent.SetDestination(_curTarget.transform.position);
//		}
//
//		if(_need > 100){
//			_curTarget = FindNearestObject(TOILET);
//			_agent.SetDestination(_curTarget.transform.position);
//		}
//
//		Debug.Log(_thirst + ", " + _need);
	}

	public void GoTo(string tag){
		_curTarget = FindNearestObject(tag);
		_agent.SetDestination(_curTarget.transform.position);
	}

	public GameObject FindNearestObject(string tag){
		GameObject[] gos = GameObject.FindGameObjectsWithTag(tag);
		GameObject target = gos[0];
		float minDist = 9999;

		foreach(GameObject go in gos){
			float dist = Vector3.Distance(transform.position, go.transform.position);
			if(dist < minDist){
				minDist = dist;
				target = go;
			}
		}

		return target;
	}

	public void OnBehaviorOver(){
		_curBehavior = _behaviors[Random.Range(0, _behaviors.Count)];
		_curBehavior.Start();

		Debug.Log ("done, now: " + _curBehavior);
	}
}