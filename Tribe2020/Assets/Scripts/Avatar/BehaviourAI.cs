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
	private AvatarActivity _nextActivity;

	//private GameObject[] _appliances;
	private static Appliance[] _devices;
	private Room _curRoom;

	private float _startTime, _endTime;
	private bool _isSync = false;
	private bool _isScheduleOver = false;

	//Definition of a schedule item
	[System.Serializable]
	public struct ScheduleItem {
		public string time;
		public AvatarActivity activity;
	}
	public ScheduleItem[] schedule;
	private int _scheduleIndex = 0;

	// Use this for initialization
	void Start() {
		_controller = PilotController.GetInstance();
		_timeMgr = GameTime.GetInstance();

		_stats = GetComponent<AvatarStats>();
		_agent = GetComponent<NavMeshAgent>();
		_charController = GetComponent<ThirdPersonCharacter>();

		//Prepare collection of devices in pilot
		//if(_devices.Length == 0) {
			_devices = UnityEngine.Object.FindObjectsOfType<Appliance>();
		//}

		//Synchronise schedule to get current activity for time
		SyncSchedule();
	}

	// Update is called once per frame
	void Update() {
		double curTime = _timeMgr.GetTotalSeconds();

		if(curTime > _curActivity.endTime) {
			NextActivity();
			_curActivity.Run();
		}

		if(curTime < _curActivity.startTime) {
			PreviousActivity();
			_curActivity.Run();
		}

		//CheckLighting();

		switch(_curActivityState) {
			//Not doing anything, do something feasible in the pilot
			case ActivityState.Idle:
				//	_curActivity.Init(this);
				//	Debug.Log(name + " began " + _curActivity.name + " at " + time.Hour + ":" + time.Minute);
				break;
			// Walking towards an object, check if arrived to proceed
			case ActivityState.OverrideWalking:
			case ActivityState.Walking:
				if(_agent.remainingDistance > _agent.stoppingDistance) {
					_charController.Move(_agent.desiredVelocity, false, false);
				} else if(!_agent.pathPending && _agent.remainingDistance <= _agent.stoppingDistance) {
					_charController.Move(Vector3.zero, false, false);
					_curActivity.OnDestinationReached();

					//if(_curActivityState == ActivityState.Walking) {
					//	_curTargetObj.GetComponent<Appliance>().AddHarvest();
					//	_controller.OnAvatarSessionComplete(_curActivityState.ToString());
					//	_curActivity.NextStep(this);
					//} else if(_curActivityState == ActivityState.OverrideWalking) {
					//	_curActivityState = ActivityState.OverrideIdle;
					//	_controller.OnAvatarReachedPosition(this, _agent.pathEndPosition);
					//}
				}
				break;
			// Waiting for a duration, check if wait is over to proceed
			case ActivityState.Waiting:
				_charController.Move(Vector3.zero, false, false);
				_curActivity.Step(this);
				_delay -= Time.deltaTime;
				//if(_delay < 0) {
				//	_controller.OnAvatarSessionComplete(_curActivity.name);
				//	_curActivity.NextStep(this);
				//}
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
		double curTime = _timeMgr.GetTotalSeconds();

		//Get next schedule index with potential offset in days
		int nxtIndex = (_scheduleIndex + 1) % schedule.Length;
		int endTimeDayOffset = (int)Mathf.Floor((_scheduleIndex + 1) / schedule.Length);

		//Setup next schedule item and the one after that
		ScheduleItem curItem = schedule[_scheduleIndex];
		ScheduleItem nxtItem = schedule[nxtIndex];

		//Determine end time for next item from current time and start time of item after that
		double startTime = _timeMgr.ScheduleToTS(curTime, 0, curItem.time);
		double endTime = _timeMgr.ScheduleToTS(curTime, endTimeDayOffset, nxtItem.time);

		SetActivity(curItem.activity, startTime, endTime);

		//Step forward and then backward until time is right
		while(curTime > _curActivity.endTime) {
			NextActivity();
		}

		while(curTime < _curActivity.startTime) {
			PreviousActivity();
		}

		_curActivity.Run();
	}

	//
	public void SetActivity(AvatarActivity activity, double startTime, double endTime) {
		_curActivity = UnityEngine.Object.Instantiate(activity) as AvatarActivity;
		_curActivity.Init(this, startTime, endTime);
	}

	//
	public void StartActivity(AvatarActivity activity) {
		_curActivity = activity;
		_curActivity.Init(this);
	}

	//
	public void NextActivity() {
		//Iterate schedule index
		_scheduleIndex = (_scheduleIndex + 1) % schedule.Length;

		//Get next schedule index with potential offset in days
		int nxtIndex = (_scheduleIndex + 1) % schedule.Length;
		int endTimeDayOffset = (int)Mathf.Floor((_scheduleIndex + 1) / schedule.Length);

		//Setup next schedule item and the one after that
		ScheduleItem curItem = schedule[_scheduleIndex];
		ScheduleItem nxtItem = schedule[nxtIndex];

		//Determine end time for next item from current time and start time of item after that
		double endTime = _timeMgr.ScheduleToTS(_curActivity.endTime, endTimeDayOffset, nxtItem.time);

		SetActivity(curItem.activity, _curActivity.endTime, endTime);
	}

	//
	public void PreviousActivity() {
		int prevIndex = 0;
		if(schedule.Length > 0) {
			//Iterate schedule index
			_scheduleIndex = (_scheduleIndex + schedule.Length - 1) % schedule.Length;

			//Get next schedule index with potential offset in days
			prevIndex = (_scheduleIndex + schedule.Length - 1) % schedule.Length;
		}
		int startTimeDayOffset = 0;
		if(prevIndex > _scheduleIndex) { startTimeDayOffset = -1; }

		//Setup next schedule item and the one after that
		ScheduleItem curItem = schedule[_scheduleIndex];
		ScheduleItem prevItem = schedule[prevIndex];

		//Determine end time for next item from current time and start time of item after that
		double startTime = _timeMgr.ScheduleToTS(_curActivity.startTime, startTimeDayOffset, prevItem.time);

		SetActivity(prevItem.activity, startTime, _curActivity.startTime);
		_curActivity.Run();
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
	public void WalkTo(string targetTag, bool isOwned) {
		_curTargetObj = FindNearestDevice(targetTag, isOwned);
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
	public void WarpToDestination() {
		_agent.Warp(_agent.destination);
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
	public void CheckLighting() {
		if(_curRoom) {
			//if(_curRoom.lux < 1) {
			//	Debug.Log(transform.parent.name + " thinks it's to dark in the " + _curRoom.name);
			//	Appliance lightSwitch = _curRoom.GetLightSwitch();
			//	if(lightSwitch) {
			//		//Debug.Log(transform.parent.name +  " found a light switch");
			//		TurnOnLight(lightSwitch);
			//	}
			//}
		}
	}
}
