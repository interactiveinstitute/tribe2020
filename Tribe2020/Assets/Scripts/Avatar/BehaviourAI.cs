using UnityEngine;
using System.Collections.Generic;
using System;
using UnityStandardAssets.Characters.ThirdPerson;

public class BehaviourAI : MonoBehaviour {
	public enum ActivityState { Idle, Walking, Waiting, Unscheduled, OverrideIdle, OverrideWalking, TurningOnLight };
	[SerializeField]
	private ActivityState _curActivityState = ActivityState.Idle;
	[SerializeField]
	private float _delay = 0;

	private PilotController _controller;
	private GameTime _timeMgr;

	private AvatarStats _stats;
	private NavMeshAgent _agent;
	private ThirdPersonCharacter _charController;

	//private Vector3 _curTargetPos;
	private GameObject _curTargetObj;
	private AvatarActivity _curActivity;

	//private GameObject[] _appliances;
	private Appliance[] _devices;
	private Room _curRoom;

	private float _startTime, _endTime;
	private bool _isSync = false;
	private bool _isScheduleOver = false;

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
		DateTime time = _timeMgr.GetDateTime();
		float curTime = time.Hour * 60 + time.Minute;
		_startTime = ScheduleItemToMinutes(schedule[0]);
		_endTime = ScheduleItemToMinutes(schedule[schedule.Length - 1]);

		if(curTime < _startTime) {
			if(_isScheduleOver) {
				_scheduleIndex = 0;
				_isSync = false;
			}
			return;
		} else if(curTime > _endTime) {
			_isScheduleOver = true;
			return;
		}

		if(!_isSync) {
			SyncSchedule();
		}

		CheckLighting();

		//Debug.Log(name + "Update: " + _curActivityState);
		switch(_curActivityState) {
			// Not doing anything, continously sync in order to poll for an activity
			case ActivityState.Idle:
				SyncSchedule();
				_curActivity.Init(this);
				Debug.Log(name + " began " + _curActivity.name + " at " + time.Hour + ":" + time.Minute);
				break;
			// Walking towards an object, check if arrived to proceed
			case ActivityState.OverrideWalking:
			case ActivityState.Walking:
				if(_agent.remainingDistance > _agent.stoppingDistance) {
					_charController.Move(_agent.desiredVelocity, false, false);
				} else if(!_agent.pathPending && _agent.remainingDistance <= _agent.stoppingDistance) {
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
			case ActivityState.TurningOnLight:
				if(_agent.remainingDistance > _agent.stoppingDistance) {
					_charController.Move(_agent.desiredVelocity, false, false);
				} else if(!_agent.pathPending && _agent.remainingDistance <= _agent.stoppingDistance) {
					_charController.Move(Vector3.zero, false, false);
					_curTargetObj.GetComponent<ElectricMeter>().On();
					_curTargetObj.GetComponentInParent<Room>().UpdateLighting();
					_curActivity.ResumeSession(this);
				}
				break;
		}
	}

	//
	public void SyncSchedule() {
		DateTime time = _timeMgr.GetDateTime();
		float curTime = time.Hour * 60 + time.Minute;

		_curActivity = UnityEngine.Object.Instantiate(schedule[_scheduleIndex].activity) as AvatarActivity;
		//_curActivity = schedule[_scheduleIndex].activity;

		//Quest questInstance = Object.Instantiate(quests[questIndex]) as Quest;

		//Debug.Log("time is: " + time.Hour + ":" + time.Minute);

		//Outside schedule, send avatar home


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
		_curActivity.ResumeSession(this);
		//_curActivityState = ActivityState.Idle;
	}

	//
	public void WalkTo(Vector3 target) {
		//_curTargetPos = target;
		
		_agent.SetDestination(target);
		_agent.updatePosition = true;
		_curActivityState = ActivityState.OverrideWalking;
	}

	//
	public void WalkTo(string[] args) {
		_curTargetObj = FindNearestDevice(args[0], args.Length > 1 && args[1] == "own");
		if(_curTargetObj == null) { return; }

		_agent.SetDestination(_curTargetObj.GetComponent<Appliance>().interactionPos);
		_curActivityState = ActivityState.Walking;
	}

	//
	public void TurnOnLight(Appliance lightSwitch) {
		_agent.SetDestination(lightSwitch.interactionPos);
		_curTargetObj = lightSwitch.gameObject;
		_curActivityState = ActivityState.TurningOnLight;
	}

	//
	public void TeleportTo(string[] args) {
		_curTargetObj = FindNearestDevice(args[0], args.Length > 1 && args[1] == "own");
		if(_curTargetObj == null) { return; }

		_curTargetObj.GetComponent<Appliance>().AddHarvest();
		_agent.Warp(_curTargetObj.GetComponent<Appliance>().interactionPos);
		//_agent.SetDestination(transform.position);
		//_curActivityState = ActivityState.OverrideWalking;
	}

	//
	public void Delay(float seconds) {
		//Debug.Log(name + ".Delay(" + seconds + ")");
		_curActivityState = ActivityState.Waiting;
		_delay = seconds;
	}

	// Searches devices for device with nearest Euclidean distance which fullfill affordance and ownership
	public GameObject FindNearestDevice(string affordance, bool isOwned) {
		GameObject target = null;
		float minDist = float.MaxValue;

		foreach(Appliance device in _devices) {
			List<string> affordances = device.avatarAffordances;
			if(affordances.Contains(affordance) && (!isOwned || device.owners.Contains(_stats.avatarName))) {
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
		if(other.GetComponent<Room>()) {
			Room room = other.GetComponent<Room>();
			_curRoom = room;
			CheckLighting();
		}

		//Debug.Log("Avatar.OnTriggerEnter: " + other.name);
	}

	//
	public void OnNextDay() {
		_scheduleIndex = 0;
		_isSync = false;
	}

	//
	public void CheckLighting() {
		if(_curRoom) {
			if(_curRoom.lux < 1) {
				Debug.Log(transform.parent.name + " thinks it's to dark in the " + _curRoom.name);
				Appliance lightSwitch = _curRoom.GetLightSwitch();
				if(lightSwitch) {
					//Debug.Log(transform.parent.name +  " found a light switch");
					TurnOnLight(lightSwitch);
				}
			}
		}
	}
}
