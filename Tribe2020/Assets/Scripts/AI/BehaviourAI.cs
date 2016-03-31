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

	private NavMeshAgent _agent;
	private GameObject _curTarget;

	private AvatarActivity _curActivity;
	private GameObject[] _appliances;

	private float _startTime, _endTime;

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

		_agent = GetComponent<NavMeshAgent>();

		_appliances = GameObject.FindGameObjectsWithTag("Appliance");

		SyncSchedule();
	}

	// Update is called once per frame
	void Update() {
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

		//Debug.Log("time is: " + time.Hour + ":" + time.Minute);

		//Outside schedule, send avatar home
		if(curTime < _startTime || curTime > _endTime) {
			//Debug.Log("outside schedule");
			return;
		}

		//Skip old schedule items until synced with current time
		while(curTime > ScheduleItemToMinutes(schedule[_scheduleIndex]) && _scheduleIndex < schedule.Length - 1) {
			//Debug.Log("skipped " + schedule[_scheduleIndex].time + " - " + schedule[_scheduleIndex].activity + " forward");
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
	}

	//
	public int ScheduleItemToMinutes(ShceduleItem item) {
		string[] timeParse = item.time.Split(':');
		return int.Parse(timeParse[0]) * 60 + int.Parse(timeParse[1]);
	}

	//
	public void WalkTo(string tag) {
		_curTarget = FindNearestObject(tag);

		if(_curTarget == null) {
			return;
		}

		_agent.SetDestination(_curTarget.transform.position);

		_curState = WALKING;
	}

	//
	public void Delay(float seconds) {
		_curState = WAITING;
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
	public void OnActivityOver() {
		_curState = IDLE;
		delay = 0;
	}
}
