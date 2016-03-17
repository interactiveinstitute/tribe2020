using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BehaviourAI : MonoBehaviour {
	private const string IDLE = "base_idle";

	private GameTime _timeMgr;

	private NavMeshAgent _agent;
	private GameObject _curTarget;
	//private float _thirst;
	//private float _need;
	//private float _temperature;

	private BaseBehaviour _curBehavior;
	private GameObject[] _appliances;

	//Definition of a scheduled behaviour
	[System.Serializable]
	public struct ShceduleItem {
		public string time;
		public BaseBehaviour behaviour;
	}
	public ShceduleItem[] schedule;
	private int _curActivityIndex = 0;

	//public List<BaseBehaviour> behaviours;
	public string curState = IDLE;
	public float delay = 0;

	// Use this for initialization
	void Start() {
		_timeMgr = GameTime.GetInstance();

		_agent = GetComponent<NavMeshAgent>();
		//_thirst = 100;
		//_need = 0;
		//_temperature = 25;

		_appliances = GameObject.FindGameObjectsWithTag("Appliance");
	}

	// Update is called once per frame
	void Update() {
		float curTime = _timeMgr.GetDateTime().Hour * 60 + _timeMgr.GetDateTime().Minute;

		//Skip old schedule items until synced with current time
		while(curTime > ScheduleItemToMinutes(schedule[_curActivityIndex]) && curState == IDLE) {
			//TODO: Quick perform behaviour to catch up in simulation
			Debug.Log(schedule[_curActivityIndex].time + ":" + schedule[_curActivityIndex].behaviour + " was skipped");

			OnBehaviorOver();
			_curActivityIndex++;
			_curBehavior = schedule[_curActivityIndex].behaviour;

			Debug.Log("now going to " + schedule[_curActivityIndex].time + ":" + schedule[_curActivityIndex].behaviour + " while in " + curState);
		}

		//If not started, init behaviour
		if(curState == IDLE) {
			_curBehavior.Init(this);
		}

		//Update current behaviour
		_curBehavior.Step(this);

		//Check if reached target if looking for target
		if(_curTarget &&
			Vector3.Distance(transform.position, _curTarget.transform.position) < 2) {
			_curBehavior.OnHasReached(this, _curTarget.tag);
		}
	}

	//
	public int ScheduleItemToMinutes(ShceduleItem item) {
		string[] timeParse = item.time.Split(':');
		return int.Parse(timeParse[0]) * 60 + int.Parse(timeParse[1]);
	}

	//
	public void GoTo(string tag) {
		Debug.Log("Going for a place to " + tag);
		_curTarget = FindNearestObject(tag);

		if(_curTarget == null) {
			return;
		}

		_agent.SetDestination(_curTarget.transform.position);
	}

	//
	public GameObject FindNearestObject(string tag) {
		GameObject target = null;
		float minDist = float.MaxValue;

		foreach(GameObject app in _appliances) {
			List<string> affordances = app.GetComponent<Appliance>().avatarAffordances;

			if(affordances.Contains(tag)) {
				float dist = Vector3.Distance(transform.position, app.transform.position);
				if(dist < minDist) {
					minDist = dist;
					target = app;
				}
			}
		}

		return target;
	}

	//
	public void OnBehaviorOver() {
		curState = IDLE;
		delay = 0;
	}
}

public struct ScheduleItem {
	public int time;
	public string activity;
}
