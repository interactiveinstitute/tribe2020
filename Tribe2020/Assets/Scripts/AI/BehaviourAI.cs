using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

//TODO: sync time if GameTime is stepped
public class BehaviourAI : MonoBehaviour {
	private const string IDLE = "avatar_idle";
	private const string WALKING = "avatar_walking";
	private const string WAITING = "avatar_waiting";
	private const string OUTSIDE = "avatar_outside_schedule";
	[SerializeField]
	private string _curState = IDLE;

	public float delay = 0;

	private GameTime _timeMgr;

	private AvatarStats _stats;
	private NavMeshAgent _agent;
	private GameObject _curTarget;

	private AvatarActivity _curActivity;
	private GameObject[] _appliances;

	private float _startTime, _endTime;
	private bool _isSync = false;

	//Definition of a schedule item
	[System.Serializable]
	public struct ShceduleItem {
		public string time;
		public AvatarActivity activity;
	}
	public ShceduleItem[] schedule;
	private int _scheduleIndex = 0;

	// Use this for initialization
	void Start() {
		_timeMgr = GameTime.GetInstance();

		_stats = GetComponent<AvatarStats>();
		_agent = GetComponent<NavMeshAgent>();

		_appliances = GameObject.FindGameObjectsWithTag("Appliance");
	}

	// Update is called once per frame
	void Update() {
		if(!_isSync) {
			SyncSchedule();
		}
		//if(curState == OUTSIDE) {
		//	SyncSchedule();
		//}

		//If not started, init behaviour
		if(_curState == IDLE) {
			SyncSchedule();
			_curActivity.Init(this);
		} else {
			//Update current behaviour
			_curActivity.Step(this);
		}

		//Check if reached target if looking for target
		if(_curState == WALKING && Vector3.Distance(transform.position, _curTarget.transform.position) < 2) {
			_curTarget.GetComponent<Appliance>().AddHarvest();

			_curActivity.NextStep(this);
		}

		//Step delay and check if waiting is over
		if(_curState == WAITING) {
			delay -= Time.deltaTime;

			if(delay < 0) {
				_curActivity.NextStep(this);
			}
		}

		//SyncSchedule();
	}

	//
	public void SyncSchedule() {
		DateTime time = _timeMgr.GetDateTime();
		float curTime = time.Hour * 60 + time.Minute;
		_startTime = ScheduleItemToMinutes(schedule[0]);
		_endTime = ScheduleItemToMinutes(schedule[schedule.Length - 1]);

		_curActivity = schedule[_scheduleIndex].activity;

		//Debug.Log("time is: " + time.Hour + ":" + time.Minute);

		//Outside schedule, send avatar home
		if(curTime < _startTime || curTime > _endTime) {
			//Debug.Log("outside schedule");
			return;
		}

		//Skip old schedule items until synced with current time
		while(curTime > ScheduleItemToMinutes(schedule[_scheduleIndex]) && _scheduleIndex < schedule.Length - 1) {
			//Debug.Log("skipped " + schedule[_scheduleIndex].time + " - " + schedule[_scheduleIndex].activity + " forward");
			_curActivity.SimulateExecution(this);

			OnActivityOver();
			_scheduleIndex++;
			_curActivity = schedule[_scheduleIndex].activity;
		}

		//Skip old schedule items until synced with current time
		while(curTime < ScheduleItemToMinutes(schedule[_scheduleIndex]) && _scheduleIndex > 0) {
			//Debug.Log("skipped " + schedule[_scheduleIndex].time + " - " + schedule[_scheduleIndex].activity + " backwards");
			OnActivityOver();
			_scheduleIndex--;
			_curActivity = schedule[_scheduleIndex].activity;
		}

		//Debug.Log("now going to " + schedule[_scheduleIndex].activity);
		_isSync = true;
	}

	//
	public int ScheduleItemToMinutes(ShceduleItem item) {
		string[] timeParse = item.time.Split(':');
		return int.Parse(timeParse[0]) * 60 + int.Parse(timeParse[1]);
	}

	//
	public void WalkTo(string[] args) {
		if(args.Length == 1) {
			_curTarget = FindNearestObject(args[0], false);
		} else if(args[1] == "own") {
			_curTarget = FindNearestObject(args[0], true);
		}


			if(_curTarget == null) {
			return;
		}

		_agent.SetDestination(_curTarget.transform.position);

		_curState = WALKING;
	}

	//
	public void TeleportTo(string[] args) {
		_curTarget = null;

		if(args.Length == 1) {
			_curTarget = FindNearestObject(args[0], false);
		} else if(args[1] == "own") {
			_curTarget = FindNearestObject(args[0], true);
		}

		if(_curTarget == null) {
			return;
		}

		_curTarget.GetComponent<Appliance>().AddHarvest();
		transform.position = _curTarget.transform.position;
	}

	//
	public void Delay(float seconds) {
		_curState = WAITING;
		delay = seconds;
	}

	//
	public GameObject FindNearestObject(string tag, bool hasOwner) {
		GameObject target = null;
		float minDist = float.MaxValue;

		foreach(GameObject appObj in _appliances) {
			Appliance app = appObj.GetComponent<Appliance>();

			List<string> affordances = app.avatarAffordances;
			if(affordances.Contains(tag) && (!hasOwner || app.owners.Contains(_stats.avatarName))) {
				float dist = Vector3.Distance(transform.position, app.transform.position);
				if(dist < minDist) {
					minDist = dist;
					target = appObj;

					//Debug.Log(_stats.avatarName + " FOUND affordance " + tag + " in " + app.title);
				}
			}
		}
		return target;
	}

	//
	public void OnActivityOver() {
		_curState = IDLE;
		delay = 0;
	}
}
