using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//TODO: sync time if GameTime is stepped
public class BehaviourAI : MonoBehaviour {
	private const string IDLE = "avatar_idle";
	private const string WALKING = "avatar_walking";
	private const string WAITING = "avatar_waiting";

	private GameTime _timeMgr;

	private NavMeshAgent _agent;
	private GameObject _curTarget;

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

		_appliances = GameObject.FindGameObjectsWithTag("Appliance");

		SyncSchedule();
	}

	// Update is called once per frame
	void Update() {
		//If not started, init behaviour
		if(curState == IDLE) {
			_curBehavior.Init(this);
		}

		//Update current behaviour
		_curBehavior.Step(this);

		//Check if reached target if looking for target
		if(curState == "walking" && Vector3.Distance(transform.position, _curTarget.transform.position) < 2) {
			_curBehavior.NextStep(this);
		}

		//Step delay and check if waiting is over
		if(curState == "waiting") {
			delay -= Time.deltaTime;

			if(delay < 0) {
				_curBehavior.NextStep(this);
			}
		}
	}

	//
	public void SyncSchedule() {
		float curTime = _timeMgr.GetDateTime().Hour * 60 + _timeMgr.GetDateTime().Minute;
		//Debug.Log("time " + curTime);
		_curBehavior = schedule[0].behaviour;

		//Skip old schedule items until synced with current time
		while(curTime > ScheduleItemToMinutes(schedule[_curActivityIndex]) && _curActivityIndex < schedule.Length - 1) {
			//Debug.Log("skipped " + ScheduleItemToMinutes(schedule[_curActivityIndex]) + " forward");
			OnBehaviorOver();
			_curActivityIndex++;
			_curBehavior = schedule[_curActivityIndex].behaviour;
		}

		//Skip old schedule items until synced with current time
		while(curTime < ScheduleItemToMinutes(schedule[_curActivityIndex]) && _curActivityIndex > 0) {
			//Debug.Log("skipped " + ScheduleItemToMinutes(schedule[_curActivityIndex]) + " backwards");
			OnBehaviorOver();
			_curActivityIndex--;
			_curBehavior = schedule[_curActivityIndex].behaviour;
		}
	}

	//
	public int ScheduleItemToMinutes(ShceduleItem item) {
		string[] timeParse = item.time.Split(':');
		return int.Parse(timeParse[0]) * 60 + int.Parse(timeParse[1]);
	}

	//
	public void GoTo(string tag) {
		_curTarget = FindNearestObject(tag);

		if(_curTarget == null) {
			return;
		}

		_agent.SetDestination(_curTarget.transform.position);
	}

	//
	public void WalkTo(string tag) {

		curState = WALKING;
		GoTo(tag);
	}

	//
	public void Delay(float seconds) {
		curState = WAITING;
		delay = seconds;
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
