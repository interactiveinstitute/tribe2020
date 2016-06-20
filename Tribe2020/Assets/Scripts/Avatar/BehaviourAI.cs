using UnityEngine;
using System.Collections.Generic;
using System;
using UnityStandardAssets.Characters.ThirdPerson;

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
	private ThirdPersonCharacter _charController;
	private Vector3 _curTargetPos;
	private GameObject _curTargetObj;
	private Transform _destination;

	private AvatarActivity _curActivity;
	//private GameObject[] _appliances;
	private Appliance[] _devices;

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
		_charController = GetComponent<ThirdPersonCharacter>();

		//_appliances = GameObject.FindGameObjectsWithTag("Appliance");
		_devices = UnityEngine.Object.FindObjectsOfType<Appliance>();
	}

	// Update is called once per frame
	void Update() {
		if(!_isSync) {
			SyncSchedule();
		}

		//Debug.Log(name + "Update: " + _curActivityState);
		switch(_curActivityState) {
			// Not doing anything, continously sync in order to poll for an activity
			case ActivityState.Idle:
				SyncSchedule();
				_curActivity.Init(this);
				break;
			// Walking towards an object, check if arrived to proceed
			case ActivityState.OverrideWalking:
			case ActivityState.Walking:
				//Debug.Log("remain:" + _agent.remainingDistance + ", stop:" + _agent.stoppingDistance);
				if(_agent.remainingDistance > _agent.stoppingDistance) {
					_charController.Move(_agent.desiredVelocity, false, false);
				} else if(!_agent.pathPending &&
					_agent.remainingDistance <= _agent.stoppingDistance /*&&
					(!_agent.hasPath || _agent.velocity.sqrMagnitude == 0f)*/) {
					//Debug.Log(name + " doing " + _curActivityState + ", REACHED agent destination");
					_charController.Move(Vector3.zero, false, false);

					if(_curActivityState == ActivityState.Walking) {
						_curTargetObj.GetComponent<Appliance>().AddHarvest();
						_controller.OnAvatarSessionComplete(_curActivityState.ToString());
						_curActivity.NextStep(this);
					} else if(_curActivityState == ActivityState.OverrideWalking) {
						_curActivityState = ActivityState.OverrideIdle;
						_controller.OnAvatarReachedPosition(this, _agent.pathEndPosition);
					}
				}

				//_curActivity.Step(this);
				//if(Vector3.Distance(transform.position, _curTargetObj.transform.position) < 0.5f) {
				//	Debug.Log(name + " also reached target destination");
					
				//}
				break;
			// Waiting for a duration, check if wait is over to proceed
			case ActivityState.Waiting:
				_charController.Move(Vector3.zero, false, false);
				_curActivity.Step(this);
				_delay -= Time.deltaTime;
				if(_delay < 0) {
					_controller.OnAvatarSessionComplete(_curActivity.name);
					_curActivity.NextStep(this);
				}
				break;
			case ActivityState.OverrideIdle:
				_charController.Move(Vector3.zero, false, false);
				break;
				// Override activity for instance from a quest narration controller, callback if arrived
				//case ActivityState.OverrideWalking:
				//	if(Vector3.Distance(transform.position, _curTargetPos) < 2) {
				//		_curActivityState = ActivityState.OverrideIdle;
				//		_controller.OnAvatarReachedPosition(this, _curTargetPos);
				//	}
				//	break;
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
			_curActivity.SimulateExecution(this);

			OnActivityOver();
			_scheduleIndex++;
			_curActivity = schedule[_scheduleIndex].activity;
		}

		//Skip old schedule items until synced with current time
		while(curTime < ScheduleItemToMinutes(schedule[_scheduleIndex]) && _scheduleIndex > 0) {
			OnActivityOver();
			_scheduleIndex--;
			_curActivity = schedule[_scheduleIndex].activity;
		}

		_isSync = true;
	}

	//
	public int ScheduleItemToMinutes(ShceduleItem item) {
		string[] timeParse = item.time.Split(':');
		return int.Parse(timeParse[0]) * 60 + int.Parse(timeParse[1]);
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
	public void WalkTo(Vector3 target) {
		Debug.Log("Commanded to walk towards " + target);
		_curTargetPos = target;
		
		_agent.SetDestination(target);
		_agent.updatePosition = true;
		_curActivityState = ActivityState.OverrideWalking;
	}

	//
	public void WalkTo(string[] args) {
		_curTargetObj = FindNearestObject(args[0], args.Length > 1 && args[1] == "own");
		Debug.Log("Walking towards " + _curTargetObj.GetComponent<Appliance>().interactionPos + " as part of session");
		if(_curTargetObj == null) { return; }

		_agent.SetDestination(_curTargetObj.GetComponent<Appliance>().interactionPos);
		_curActivityState = ActivityState.Walking;
	}

	//
	public void TeleportTo(string[] args) {
		_curTargetObj = FindNearestObject(args[0], args.Length > 1 && args[1] == "own");
		if(_curTargetObj == null) { return; }

		_curTargetObj.GetComponent<Appliance>().AddHarvest();
		_agent.Warp(_curTargetObj.GetComponent<Appliance>().interactionPos);
		//_agent.SetDestination(transform.position);
		//_curActivityState = ActivityState.OverrideWalking;
	}

	//
	public void Delay(float seconds) {
		Debug.Log(name + ".Delay(" + seconds + ")");
		_curActivityState = ActivityState.Waiting;
		_delay = seconds;
	}

	// Looks through available devices and picks the one closest to the
	// avatar through raycasting
	public GameObject FindNearestObject(string tag, bool hasOwner) {
		GameObject target = null;
		float minDist = float.MaxValue;

		//foreach(GameObject appObj in _appliances) {
		//	Appliance app = appObj.GetComponent<Appliance>();

		//	List<string> affordances = app.avatarAffordances;
		//	if(affordances.Contains(tag) && (!hasOwner || app.owners.Contains(_stats.avatarName))) {
		//		float dist = Vector3.Distance(transform.position, app.transform.position);
		//		if(dist < minDist) {
		//			minDist = dist;
		//			target = appObj;
		//		}
		//	}
		//}
		foreach(Appliance device in _devices) {
			//Appliance app = appObj.GetComponent<Appliance>();

			List<string> affordances = device.avatarAffordances;
			if(affordances.Contains(tag) && (!hasOwner || device.owners.Contains(_stats.avatarName))) {
				float dist = Vector3.Distance(transform.position, device.transform.position);
				if(dist < minDist) {
					minDist = dist;
					target = device.gameObject;
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

	//
	void OnTriggerEnter(Collider other) {
		Debug.Log("Avatar.OnTriggerEnter: " + other.name);
	}
}
