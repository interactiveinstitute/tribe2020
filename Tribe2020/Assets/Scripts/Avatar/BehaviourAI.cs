using UnityEngine;
using System.Collections.Generic;
using System;
using UnityStandardAssets.Characters.ThirdPerson;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(ThirdPersonCharacter))]
public class BehaviourAI : MonoBehaviour {
	public enum ActivityState { Idle, Walking, Sitting, Waiting, Unscheduled, OverrideIdle, OverrideWalking, TurningOnLight };
	[SerializeField]
	private ActivityState _curActivityState = ActivityState.Idle;

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

        //added by Gunnar.
        _agent.updatePosition = true;
        _agent.updateRotation = false;

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

        // These two if statements are meant to handle jumps in time. If curTime suddenly is increased a lot
        // the script will try to simulate the schedule until the curTime is reached.
		if(curTime > _curActivity.endTime) {
            Debug.Log("Current activity's endTime passed. Finish it and start next one");
			_curActivity.FinishCurrentActivity();
			NextActivity();
			_curActivity.Run();
		}

        //same as above but backwards.
		if(curTime < _curActivity.startTime)
        {
            Debug.Log("Current activity's startTime passed. Revert the activity and start the previous one");
            _curActivity.Revert();
			PreviousActivity();
			_curActivity.Run();
		}
        
        _curActivity.Step(this);

        //First disable sit flag if we're not in that state
        if(_curActivityState != ActivityState.Sitting)
        {
            _charController.StandUp();
        }

        switch (_curActivityState) {
			//Not doing anything, do something feasible in the pilot
			case ActivityState.Idle:
                //	_curActivity.Init(this);
                //	Debug.Log(name + " began " + _curActivity.name + " at " + time.Hour + ":" + time.Minute);
                break;
			// Walking towards an object, check if arrived to proceed
			case ActivityState.OverrideWalking:
                //Debug.Log("override walking!");
			case ActivityState.Walking:
				if(_agent.remainingDistance > _agent.stoppingDistance) {
					_charController.Move(_agent.desiredVelocity, false, false);
				} else if(!_agent.pathPending) {
                    //Hurray. We got there.
                    //Debug.Log("we got here!");
                    _charController.Move(Vector3.zero, false, false);
                    //_curActivity.OnDestinationReached();

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
            case ActivityState.Sitting:
                _charController.SitDown();
                break;
			// Waiting
			case ActivityState.Waiting:
                _charController.Move(Vector3.zero, false, false);
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

    //Erik claims that this part might be related to have the avatar behave correctly when making jumps in time.
    //I'm not convinced though... It seems to run only once when initiating an avatar, 
    //so the only chance it might relate to jumps in time is if _scheduleIndex is somehow
    //set from save on startup (now it's always 0 when entering the function, no?).
    //And my understanding is that the schedule is not part of save?
    //I'm now changing it to always find the closest upcoming activity in the schedule and choose it as _curActivity...
    public void SyncSchedule() {
        double curTime = _timeMgr.GetTotalSeconds();
        DateTime curDateTime = _timeMgr.GetDateTime();
        double curHour = curDateTime.Hour;
        double curMinute = curDateTime.Minute;

        //loop through the schedule until we get to an actvity that should happen in the future
        for(; _scheduleIndex < schedule.Length; _scheduleIndex++)
        {

            string[] timeParse = schedule[_scheduleIndex].time.Split(':');
            int schedHour = int.Parse(timeParse[0]);
            int schedMinute = int.Parse(timeParse[1]);

            //Is this activity in the future. OR. Is it the last in the schedule?
            if(curHour <= schedHour || _scheduleIndex+1 == schedule.Length)
            {
                //if same hour. also check minutes. Skip if same hour but minutes already passed.
                //Don't skip if last activity in schedule
                if (curHour == schedHour && curMinute > schedMinute && _scheduleIndex + 1 != schedule.Length)
                {
                    continue; // if same hour and minutes already past, don't pick this activity.
                }

                //ok. this activity is in the future. Let's set it as the _curActivity.
                //To achieve that we must first calculate some timstamps and stuff...

                ////Get next schedule index with potential offset in days
                int nxtIndex = (_scheduleIndex + 1) % schedule.Length;

                //Setup the current schedule item and the one after that.
                //We need the second one to set endtime of the first
                ScheduleItem curItem = schedule[_scheduleIndex];
                ScheduleItem nxtItem = schedule[nxtIndex];

                //Will be set to 1 if the current activity is the last in schedule.
                //If that's the case we want to set the endTime for current activity to be on the next day
                int endTimeDayOffset = (int)Mathf.Floor((_scheduleIndex + 1) / schedule.Length);

                //Determine startTime as timeStamp
                double startTime = _timeMgr.ScheduleToTS(curTime, 0, curItem.time);
                //Determine end time from current time and start time of item after current
                double endTime = _timeMgr.ScheduleToTS(curTime, endTimeDayOffset, nxtItem.time);

                    
                SetActivity(schedule[_scheduleIndex].activity, startTime, endTime);

                Debug.Log("startActivity set. It's " + schedule[_scheduleIndex].activity + " with timeStamps " + startTime + ", " + endTime);
                return;
            }
        }




        //---------------------OLD ERIK VERSION
        //      // get da time!
        //double curTime = _timeMgr.GetTotalSeconds();

        ////Get next schedule index with potential offset in days
        //int nxtIndex = (_scheduleIndex + 1) % schedule.Length;

        //Will be set to 1 if the next activity is the last in schedule.
        //If that's the case we want to set the endTime to be on the next day
        //int endTimeDayOffset = (int)Mathf.Floor((_scheduleIndex + 1) / schedule.Length);

        ////Setup next schedule item and the one after that
        //ScheduleItem curItem = schedule[_scheduleIndex];
        //ScheduleItem nxtItem = schedule[nxtIndex];

        ////Determine end time for next item from current time and start time of item after that
        //double startTime = _timeMgr.ScheduleToTS(curTime, 0, curItem.time);
        //double endTime = _timeMgr.ScheduleToTS(curTime, endTimeDayOffset, nxtItem.time);

        //SetActivity(curItem.activity, startTime, endTime);

        //      //Step forward and then backward until time is right
        //      while (curTime > _curActivity.endTime) {
        //          NextActivity();
        //      }

        //      while (curTime < _curActivity.startTime) {
        //          PreviousActivity();
        //      }

        //      _curActivity.Run();
    }

	//
	public void SetActivity(AvatarActivity activity, double startTime, double endTime) {
        Debug.Log("setting current activity: " + activity);
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
        //int prevIndex = 0;
        int nextIndex = 0;
		if(schedule.Length > 0) {
            //Sooo. Since we're stepping backwards, the current index will become nextIndex
            nextIndex = _scheduleIndex;

            //Iterate schedule index
            _scheduleIndex = (_scheduleIndex + schedule.Length - 1) % schedule.Length;

            ////Get previous schedule index
            //prevIndex = (_scheduleIndex + schedule.Length - 1) % schedule.Length;
		}

		int startTimeDayOffset = 0;
        //Is the previous activity started the previous day?
        if (_scheduleIndex > nextIndex) { startTimeDayOffset = -1; }

		//Setup previous schedule item and the one after that
		ScheduleItem prevItem = schedule[_scheduleIndex];
		ScheduleItem curItem = schedule[nextIndex];

        //Determine the startTime of the activity we're stepping back to,
        //using the activity before that to get startTime
		double startTime = _timeMgr.ScheduleToTS(_curActivity.startTime, startTimeDayOffset, prevItem.time);

		SetActivity(prevItem.activity, startTime, _curActivity.startTime);
		//_curActivity.Run();
	}

	//
	public void EndOverride() {
		_curActivity.ResumeSession(this);
		//_curActivityState = ActivityState.Idle;
	}

	//
	public void Interact() {
	}

	//
	public void Stop() {
        _curActivityState = ActivityState.Idle;
        _charController.Move(Vector3.zero, false, false);
	}

    public void Wait()
    {
        _curActivityState = ActivityState.Idle;
        _charController.Move(Vector3.zero, false, false);
    }

	//This is an override walk. Should be clearer that's the case.
	public void WalkTo(Vector3 target) {
		//_curTargetPos = target;
		
		_agent.SetDestination(target);
		_agent.updatePosition = true;
		_curActivityState = ActivityState.OverrideWalking;
	}

	//
	//public void WalkTo(string[] args) {
	//	_curTargetObj = FindNearestDevice(args[0], args.Length > 1 && args[1] == "own");
	//	if(_curTargetObj == null) { return; }

	//	_agent.SetDestination(_curTargetObj.GetComponent<Appliance>().interactionPos);
	//	_curActivityState = ActivityState.Walking;
	//}

	//Makes the avatar walks towards an object by setting the navmeshagent destination.
	public void WalkTo(AvatarActivity.Target target, bool isOwned) {
		_curTargetObj = FindNearestDevice(target, isOwned);
		if(_curTargetObj == null) {
            Debug.LogError("Didn't find a target " + target);
            return;
        }

		_agent.SetDestination(_curTargetObj.GetComponent<Appliance>().interactionPos);
		_curActivityState = ActivityState.Walking;
	}

	//
	public void TeleportTo(AvatarActivity.Target target, bool isOwned) {
		_curTargetObj = FindNearestDevice(target, isOwned);
		if(_curTargetObj == null) { return; }

		_curTargetObj.GetComponent<Appliance>().AddHarvest();
		_agent.Warp(_curTargetObj.GetComponent<Appliance>().interactionPos);
	}

	//
	//public void TeleportTo(string[] args) {
	//	_curTargetObj = FindNearestDevice(args[0], args.Length > 1 && args[1] == "own");
	//	if(_curTargetObj == null) { return; }

	//	_curTargetObj.GetComponent<Appliance>().AddHarvest();
	//	_agent.Warp(_curTargetObj.GetComponent<Appliance>().interactionPos);
	//}

	//
	public void WarpToDestination() {
		_agent.Warp(_agent.destination);
	}

    //
    public void SitDown()
    {
        _curActivityState = ActivityState.Sitting;
    }

	//
	public void Delay(float seconds) {
		//Debug.Log(name + ".Delay(" + seconds + ")");
		_curActivityState = ActivityState.Waiting;
	}

	//
	public void SetRunLevel(AvatarActivity.Target target, string parameter) {
		GameObject device = FindNearestDevice(target, false);
		device.GetComponent<ElectricMeter>().On();
			//.SetRunlevel(int.Parse(parameter));
	}

	// Searches devices for device with nearest Euclidean distance which fullfill affordance and ownership
	public GameObject FindNearestDevice(AvatarActivity.Target affordance, bool isOwned) {
		GameObject target = null;
		float minDist = float.MaxValue;

		foreach(Appliance device in _devices) {
			List<AvatarActivity.Target> affordances = device.avatarAffordances;
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
			if(_curRoom.lux < 1) {
				//Debug.Log(name + " thinks it's to dark in the " + _curRoom.name);
				Appliance lightSwitch = _curRoom.GetLightSwitch();
				if(lightSwitch) {
					//Debug.Log(transform.parent.name +  " found a light switch");
					TurnOnLight(lightSwitch);
				}
			} else {
				//Debug.Log(name + " is ok with light in " + _curRoom.name);
			}
		}
	}

	//
	public void CheckLighting(GameObject device) {
		Room deviceRoom = device.GetComponentInParent<Room>();
		if(deviceRoom) {
			if(deviceRoom.lux < 1) {
				Appliance lightSwitch = deviceRoom.GetLightSwitch();
				if(lightSwitch) {
					QuickTurnLightOn(lightSwitch);
				}
			}
		}
	}

	//
	public void TurnOnLight(Appliance lightSwitch) {
		AvatarActivity.Session walkToLightSwitch = new AvatarActivity.Session();
		walkToLightSwitch.title = "Walking to light switch";
		walkToLightSwitch.type = AvatarActivity.SessionType.WalkTo;
		walkToLightSwitch.target = AvatarActivity.Target.LampSwitch;
		walkToLightSwitch.currentRoom = true;

		AvatarActivity.Session turnOnLight = new AvatarActivity.Session();
		turnOnLight.title = "Turning on light";
		turnOnLight.type = AvatarActivity.SessionType.SetRunlevel;
		turnOnLight.target = AvatarActivity.Target.LampSwitch;
		turnOnLight.parameter = "1";

		_curActivity.InsertSession(turnOnLight);
		_curActivity.InsertSession(walkToLightSwitch);
	}

	//
	public void QuickTurnLightOn(Appliance lightSwitch) {
		AvatarActivity.Session walkToLightSwitch = new AvatarActivity.Session();
		walkToLightSwitch.type = AvatarActivity.SessionType.WalkTo;
		walkToLightSwitch.target = AvatarActivity.Target.LampSwitch;
		walkToLightSwitch.currentRoom = true;

		AvatarActivity.Session turnOnLight = new AvatarActivity.Session();
		turnOnLight.title = "Turn on light";
		turnOnLight.type = AvatarActivity.SessionType.SetRunlevel;
		turnOnLight.target = AvatarActivity.Target.LampSwitch;
		turnOnLight.parameter = "0";

		_curActivity.InsertSession(turnOnLight);
		_curActivity.InsertSession(walkToLightSwitch);
	}
}
