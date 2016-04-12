using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class BehaviourAI : MonoBehaviour {
	public enum ActivityState { Idle, Walking, Waiting, Unscheduled, OverrideIdle, OverrideWalking };
	[SerializeField]
	private ActivityState _curActivityState = ActivityState.Idle;
	[SerializeField]
	private float _delay = 0;

	private PilotController _controller;
	private GameTime _timeMgr;

	private AvatarStats _stats;
	private NavMeshAgent _agent;
	private Vector3 _curTargetPos;
	private GameObject _curTargetObj;

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
		_controller = PilotController.GetInstance();
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

		switch(_curActivityState) {
			// Not doing anything, continously sync in order to poll for an activity
			case ActivityState.Idle:
				SyncSchedule();
				_curActivity.Init(this);
				break;
			// Walking towards an object, check if arrived to proceed
			case ActivityState.Walking:
				_curActivity.Step(this);
				if(Vector3.Distance(transform.position, _curTargetObj.transform.position) < 2) {
					_curTargetObj.GetComponent<Appliance>().AddHarvest();
					_controller.OnAvatarSessionComplete(_curActivityState.ToString());
					_curActivity.NextStep(this);
				}
				break;
			// Waiting for a duration, check if wait is over to proceed
			case ActivityState.Waiting:
				_curActivity.Step(this);
				_delay -= Time.deltaTime;
				if(_delay < 0) {
					_controller.OnAvatarSessionComplete(_curActivity.name);
					_curActivity.NextStep(this);
				}
				break;
			// Override activity for instance from a quest narration controller, callback if arrived
			case ActivityState.OverrideWalking:
				if(Vector3.Distance(transform.position, _curTargetPos) < 2) {
					_curActivityState = ActivityState.OverrideIdle;
					_controller.OnAvatarReachedPosition(this, _curTargetPos);
				}
				break;
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
	public void WalkTo(Vector3 target) {
		_curTargetPos = target;
		_agent.SetDestination(_curTargetPos);

		_curActivityState = ActivityState.OverrideWalking;
	}

	//
	public void StartActivity(AvatarActivity activity) {
		_curActivity = activity;
		_curActivity.Init(this);

	}

	//
	public void EndOverride() {
		_curActivityState = ActivityState.Idle;
	}

	//
	public void WalkTo(string[] args) {
		if(args.Length == 1) {
			_curTargetObj = FindNearestObject(args[0], false);
		} else if(args[1] == "own") {
			_curTargetObj = FindNearestObject(args[0], true);
		}


		if(_curTargetObj == null) {
			return;
		}

		_agent.SetDestination(_curTargetObj.transform.position);

		_curActivityState = ActivityState.Walking;
	}

	//
	public void TeleportTo(string[] args) {
		_curTargetObj = null;

		if(args.Length == 1) {
			_curTargetObj = FindNearestObject(args[0], false);
		} else if(args[1] == "own") {
			_curTargetObj = FindNearestObject(args[0], true);
		}

		if(_curTargetObj == null) {
			return;
		}

		_curTargetObj.GetComponent<Appliance>().AddHarvest();
		transform.position = _curTargetObj.transform.position;
	}

	//
	public void Delay(float seconds) {
		_curActivityState = ActivityState.Waiting;
		_delay = seconds;
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
		_controller.OnAvatarActivityComplete(_curActivity.name);
		_curActivityState = ActivityState.Idle;
		_delay = 0;
	}
}
