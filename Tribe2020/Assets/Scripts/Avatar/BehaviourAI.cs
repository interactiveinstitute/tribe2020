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
	public AvatarActivity _curActivity;
    public AvatarActivity _nextActivity;
    public AvatarActivity _prevActivity;

	//private GameObject[] _appliances;
	private static Appliance[] _devices;
	private Room _curRoom;

	private float _startTime;
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
		if(_curActivityState != ActivityState.OverrideWalking && _curActivityState != ActivityState.OverrideIdle) {
			
            //Ok. Let's find the stamp for the next activity.
            if(curTime > _nextActivity.startTime)
            {
                Debug.Log("Next activity's startTime (" + _nextActivity.startTime+ ") passed. Finish current one and start the next one");
                _curActivity.FinishCurrentActivity();
                NextActivity();
                _curActivity.Run();
            }


            //if(curTime > _curActivity.endTime) {
            //	Debug.Log("Current activity's endTime (" + _curActivity.endTime + ") passed. Finish it and start next one");
            //	_curActivity.FinishCurrentActivity();
            //	NextActivity();
            //	_curActivity.Run();
            //}

            //same as above but backwards.
            if (curTime < _curActivity.startTime) {
				Debug.Log("Current activity's startTime passed. Revert the activity and start the previous one");
				_curActivity.Revert();
				PreviousActivity();
				_curActivity.Run();
			}
		}
        
        //do delta time stuffz
        _curActivity.Step(this);

        //First disable sit flag if we're not in the sitting state
        if(_curActivityState != ActivityState.Sitting) {
            _charController.StandUp();//Turns off a state boolean for the animator.
        }else
        {
            Debug.Log("ActivityState is Sitting");
        }

        switch (_curActivityState) {
			//Not doing anything, do something feasible in the pilot
			case ActivityState.Idle:
				_charController.Move(Vector3.zero, false, false);
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
                    _charController.Move(Vector3.zero, false, false);
                    //Ok. Let's notify the activity that the current session is finished
                    _curActivity.OnDestinationReached();

                    //if(_curActivityState == ActivityState.Walking) {
                    //	_curTargetObj.GetComponent<Appliance>().AddHarvest();
                    //	_controller.OnAvatarSessionComplete(_curActivityState.ToString());
                    //	_curActivity.NextStep(this);
                    //} else if(_curActivityState == ActivityState.OverrideWalking) {
                    //if(_curActivityState == ActivityState.OverrideWalking) {
                    //	_curActivityState = ActivityState.OverrideIdle;
                    //} else {
                    //	_curActivityState = ActivityState.Idle;
                    //}
                    _controller.OnAvatarReachedPosition(this, _agent.pathEndPosition); //triggers narration relate stuff
					//}
				}
				break;
            case ActivityState.Sitting:
                //Be aware. This SitDown method is another than the one in this class.
                //_charController.SitDown(); //Sets a boolean in the animator object
                _charController.Move(Vector3.zero, false, false);
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
    
    //Soooo. What this function is doing now is: Choose which activity should be the current one given the current time. Also sets _prevActivity and _nextActivity
    public void SyncSchedule() {
        double curTime = _timeMgr.GetTotalSeconds();
        DateTime curDateTime = _timeMgr.GetDateTime();
        double curHour = curDateTime.Hour;
        double curMinute = curDateTime.Minute;

        Debug.Log("starting schedule sync");

        //loop through the schedule until we get to an actvity that should happen in the future
        for(; _scheduleIndex < schedule.Length; _scheduleIndex++)
        {
            //Handle schedule posts without time. Such activities should happen as soon as the activity before is finished.
            if(schedule[_scheduleIndex].time == null || schedule[_scheduleIndex].time == "")
            {
                //Ok. so this schedule post has no time specified. Let's loop further.
                continue;
            }
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

                //ok. this activity is the closest upcoming one in the future. Let's set it as the _curActivity and the adjacent activities as prev and next.
                //To achieve that we must first calculate some timstamps and stuff...

                //Setup the scheduleIndices.
                int nxtIndex = (_scheduleIndex + 1) % schedule.Length;
                int prevIndex = (_scheduleIndex + schedule.Length - 1) % schedule.Length;
                //Get the schedule items
                ScheduleItem prevItem = schedule[prevIndex];
                ScheduleItem curItem = schedule[_scheduleIndex];
                ScheduleItem nxtItem = schedule[nxtIndex];

                //Set dayoffsets
                //Will be set to 1 if the current activity is the last in schedule.
                //If that's the case we want to set the startTime for next activity to be on the next day
                int nxtActivityDayOffset = (int)Mathf.Floor((_scheduleIndex + 1) / schedule.Length);
                //If current is at index 0 than previous is the day before.
                int prevActivityDayOffset = 0;
                //Is the prevActivity started the previous day?
                if (_scheduleIndex == 0) { prevActivityDayOffset = -1; }

                //Determine startTime as timeStamp. We set dayOffset to 0 since we should set the activity to this day.
                double startTime = 0;
                if (curItem.time != "")
                {
                    startTime  = _timeMgr.ScheduleToTS(curTime, 0, curItem.time);
                    SetCurrentActivity(curItem.activity, startTime);
                }
                else {
                    SetCurrentActivity(curItem.activity);
                }

                //Determine startTime for nextActivity
                double nxtStartTime = 0;
                if (nxtItem.time != "")
                {
                    nxtStartTime = _timeMgr.ScheduleToTS(curTime, nxtActivityDayOffset, nxtItem.time);
                    SetNextActivity(nxtItem.activity, nxtStartTime);
                }
                else
                {
                    SetNextActivity(nxtItem.activity);
                }

                //Determine startTime for prevActivity
                double prevStartTime = 0;
                if (prevItem.time != "")
                {
                    prevStartTime = _timeMgr.ScheduleToTS(curTime, prevActivityDayOffset, prevItem.time);
                    SetPrevActivity(prevItem.activity, prevStartTime);
                }else
                {
                    SetPrevActivity(prevItem.activity);
                }

                
                

                Debug.Log("startActivity set for " + name + ". It's " + curItem.activity + " with startTimeStamp " + startTime);
                return;
            }
        }
    }

	//
	public void SetCurrentActivity(AvatarActivity activity, double startTime) {
        //Debug.Log("setting current activity: " + activity);
		_curActivity = UnityEngine.Object.Instantiate(activity) as AvatarActivity;
		_curActivity.Init(this, startTime);
	}

    public void SetCurrentActivity(AvatarActivity activity)
    {
        //Debug.Log("setting current activity: " + activity);
        _curActivity = UnityEngine.Object.Instantiate(activity) as AvatarActivity;
        _curActivity.Init(this);
    }

    public void SetNextActivity(AvatarActivity activity, double startTime)
    {
        _nextActivity = UnityEngine.Object.Instantiate(activity) as AvatarActivity;
        _nextActivity.Init(this, startTime);
    }

    public void SetNextActivity(AvatarActivity activity)
    {
        _nextActivity = UnityEngine.Object.Instantiate(activity) as AvatarActivity;
        _nextActivity.Init(this);
    }

    public void SetPrevActivity(AvatarActivity activity, double startTime)
    {
        _prevActivity = UnityEngine.Object.Instantiate(activity) as AvatarActivity;
        _prevActivity.Init(this, startTime);
    }

    public void SetPrevActivity(AvatarActivity activity)
    {
        _prevActivity = UnityEngine.Object.Instantiate(activity) as AvatarActivity;
        _prevActivity.Init(this);
    }

    //
    public void StartActivity(AvatarActivity activity) {
		//Debug.Log(name + ".StartActivity(" + activity + ") with end time " + _timeMgr.time + " + " + (60 * 3));
		_curActivityState = ActivityState.Idle;
		//activity.endTime = _timeMgr.time + 60 * 3;
		_curActivity = activity;
		_curActivity.Init(this);
	}

	//Alright. Let's pick the next activity in the schedule. This function updates the references of _prev, _cur and _next -activity.
	public void NextActivity() {
		//Iterate schedule index
        //Soo. _scheduleIndex is here incremented to point on the activity we jump to.
		_scheduleIndex = (_scheduleIndex + 1) % schedule.Length;

        //Get next schedule index with potential offset in days
        int nxtIndex = (_scheduleIndex + 1) % schedule.Length;
        int nxtActivityDayOffset = (int)Mathf.Floor((_scheduleIndex + 1) / schedule.Length);

		//Setup next schedule item
        ScheduleItem nxtItem = schedule[nxtIndex];

        //Determine startTime for nextActivity
        double nxtStartTime = _timeMgr.ScheduleToTS(_curActivity.startTime, nxtActivityDayOffset, nxtItem.time);

        //We have already an instantiated activity in _nextActivity. So we set _curActivity as _prevActivity and _nextActivity as _curActivity. Then we instantiate a new one for _nextActivity.
        _prevActivity = _curActivity;
        _curActivity = _nextActivity;
        SetNextActivity(nxtItem.activity, nxtStartTime);
    }

	//
	public void PreviousActivity() {
        int prevIndex = 0;
        if (schedule.Length > 0) {

            //Iterate schedule index
            _scheduleIndex = (_scheduleIndex + schedule.Length - 1) % schedule.Length;
            prevIndex = (_scheduleIndex + schedule.Length - 1) % schedule.Length;
        }

		int startTimeDayOffset = 0;
        //Is the new (previous) activity started the previous day?
        if (prevIndex > _scheduleIndex) { startTimeDayOffset = -1; }

		//Setup cur schedule item and the one after that
		ScheduleItem prevItem = schedule[prevIndex];

        //Determine the startTime of the new _prevActivity,
        //using the _prevActivity as day reference
        //This means that if _prevActivity is already on the previous day, this will be the day reference for setting startTime. If prevIndex is on same day as _prevActivity dayOffset will be 0.
        //If prevIndex instead is one day before _scheduleIndex AND _prevActivity already is on previous day (should only be possible with a schedule of length 1 I think)
        //we will get one day back from _prevActivity day reference and additionally one day back from startTimeDayOffset.
		double prevActivityStartTime = _timeMgr.ScheduleToTS(_prevActivity.startTime, startTimeDayOffset, prevItem.time);
        
        //Move all the three activities one step back
        _nextActivity = _curActivity;
        _curActivity = _prevActivity;
        SetPrevActivity(prevItem.activity, prevActivityStartTime);
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
            Debug.LogError("Didn't find a WalkTo target " + target + ". doing activity " + _curActivity.name);
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
    public void SitDown() {
        _agent.enabled = false;
        GetComponent<Rigidbody>().isKinematic = true;
        Vector3 coord;
        Transform sitPosition = _curTargetObj.transform.Find("Sit Position");
        if (sitPosition == null)
        {
            Debug.LogError("Didn't find a gameobject called sitPosition inside " + _curTargetObj.name);
        }
        else
        {
            coord.x = sitPosition.position.x;
            coord.z = sitPosition.position.z;
            coord.y = transform.position.y;
            transform.position = coord;
            Debug.Log("Setting avatar position to sitPosition from appliance object");
        }
        _curActivityState = ActivityState.Sitting;
        _charController.SitDown(); //Sets a boolean in the animator object
    }

	//
	public void Delay(float seconds) {
		//Debug.Log(name + ".Delay(" + seconds + ")");
		_curActivityState = ActivityState.Waiting;
	}

	//
	public void SetRunLevel(AvatarActivity.Target target, string parameter) {
		GameObject device = FindNearestDevice(target, false);
        if(device == null)
        {
            Debug.LogError("Didn't find device for setting runlevel");
            return;
        }

        ElectricMeter electricMeter = device.GetComponent<ElectricMeter>();
        if (electricMeter == null)
        {
            Debug.LogError("Didn't find electric meter for device");
            return;
        }

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

        if(target == null) {
            Debug.Log(name + " could not find the affordance " + affordance.ToString());
        }

		return target;
	}

	//
	public void OnActivityOver() {
		Debug.Log(name + "'s activity " + _curActivity.name + " is over");
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
