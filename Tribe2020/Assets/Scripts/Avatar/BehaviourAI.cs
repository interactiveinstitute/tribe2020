﻿using UnityEngine;
using System.Collections.Generic;
using UnityStandardAssets.Characters.ThirdPerson;
using SimpleJSON; // For encoding and decoding avatar states
using System;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(ThirdPersonCharacter))]
[System.Serializable]
public class BehaviourAI : MonoBehaviour
{
    [SerializeField]
    private PilotController _controller;
    private GameTime _timeMgr;

    [SerializeField]
    private AvatarStats _stats;
    private NavMeshAgent _agent;
    private ThirdPersonCharacter _charController;
    [SerializeField]
    private Vector3 _savedStandingPosition;

    //private Vector3 _curTargetPos;
    public AvatarActivity _curActivity;
    public AvatarActivity _nextActivity;
    public AvatarActivity _prevActivity;

    private Stack<AvatarActivity> _tempActivities = new Stack<AvatarActivity>();
    //private bool _isTemporarilyUnscheduled = false;

    [SerializeField]
    private bool _isControlled = false;
    [SerializeField]
    private bool _isSimulating = false;

    //private GameObject[] _appliances;
    private static Appliance[] _devices;
    private Room _curRoom;

    //private bool _isSync = false;
    //private bool _isScheduleOver = false;

    //Definition of a schedule item
    [System.Serializable]
    public struct ScheduleItem
    {
        public string time;
        public AvatarActivity activity;
    }

    [SerializeField]
    public ScheduleItem[] schedule;
    [SerializeField]
    private int _scheduleIndex = 0;

    [SerializeField]
    private GameObject prefab;

    // Use this for initialization
    void Start()
    {
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
        //Hmm. I think we actually should jump one back before we start. Since we've set _curActivity to the next upcoming one...
        if (_curActivity != null)
        {
            _curActivity.Start();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (_tempActivities.Count > 0)
        {
            //Handle schedule overriding activities
            UpdateActivity(_tempActivities.Peek());
            return;
        }

        if (_isControlled)
        {
            //Oh snap! This avatar is controlled directly. Let's not interfere with that.
            UpdateCharController();
            return;
        }
        
        // Start activities on scheduled times.
        // These two if statements are meant to handle jumps in time. If curTime suddenly is increased a lot
        // the script will try to simulate the schedule until the curTime is reached.

        //Ok. Let's find the stamp for the next activity. Should we switch to it?
        if (_nextActivity != null && _nextActivity.hasStartTime && _nextActivity.startTimePassed())
        {
            DebugManager.Log("Next activity's startTime (" + _nextActivity.startTime + ") passed. Finish current one and start the next one", this);
            _curActivity.FinishCurrentActivity();
            NextActivity();
            _curActivity.Start();
        }

        //same as above but backwards.
        //We are most likely not handling backwards time very well. But that's out of scope for now.
        if (_curActivity != null && _curActivity.hasStartTime && !_curActivity.startTimePassed())
        {
            DebugManager.Log("Current time " + _timeMgr.GetTotalSeconds() + " is before current activity's startTime " + _curActivity.startTime + ". Revert the activity and start the previous one", this);
            _curActivity.Revert();
            PreviousActivity();
            _curActivity.Start();
        }

        UpdateActivity();

    }

    //If no activitty reference given, Update _curActivity.
    private void UpdateActivity()
    {
        UpdateActivity(_curActivity);
    }

    private void UpdateActivity(AvatarActivity activity)
    {

        if(activity == null)
        {
            return;
        }

        //do delta time stuffz
        activity.Step(this);

        //do activity stuff!
        switch (GetRunningActivity().GetCurrentAvatarState())
        {
            //Not doing anything, do something feasible in the pilot
            case AvatarActivity.AvatarState.Idle:
                _charController.Move(Vector3.zero, false, false);
                //	activity.Init(this);
                //	Debug.Log(name + " began " + activity.name + " at " + time.Hour + ":" + time.Minute);
                break;
            // Walking towards an object, check if arrived to proceed
            //case AvatarState.OverrideWalking:
            //Debug.Log("override walking!");
            case AvatarActivity.AvatarState.Walking:
                if (_agent.remainingDistance > _agent.stoppingDistance)
                {
                    _charController.Move(_agent.desiredVelocity, false, false);
                }
                else if (!_agent.pathPending)
                {
                    _charController.Move(Vector3.zero, false, false);
                    //Ok. Let's notify the activity that the current session is finished
                    activity.OnDestinationReached();
                    
                    //	_curActivityState = ActivityState.Idle;
                    _controller.OnAvatarReachedPosition(this, _agent.pathEndPosition); //triggers narration relate stuff
                                                                                       //}
                }
                break;
            case AvatarActivity.AvatarState.Sitting:
                //Be aware. This SitDown method is another than the one in this class.
                //_charController.SitDown(); //Sets a boolean in the animator object
                _charController.Move(Vector3.zero, false, false);
                break;
            //case AvatarState.TurningOnLight:
            //    if (_agent.remainingDistance > _agent.stoppingDistance)
            //    {
            //        _charController.Move(_agent.desiredVelocity, false, false);
            //    }
            //    else if (!_agent.pathPending && _agent.remainingDistance <= _agent.stoppingDistance)
            //    {
            //        _charController.Move(Vector3.zero, false, false);
            //        _curTargetObj.GetComponent<ElectricMeter>().On();
            //        _curTargetObj.GetComponentInParent<Room>().UpdateLighting();
            //        //activity.ResumeSession(this);
            //    }
            //    break;
        }
    }

    private AvatarActivity GetRunningActivity()
    {
        if (_tempActivities.Count > 0)
        {
            return _tempActivities.Peek();
        }else
        {
            return _curActivity;
        }
    }

    private void UpdateCharController()
    {
        //This function shouldn't do any activity (or AvatarState) related stuff . It should only be concerned with updating the avatar's movements and notify whoever is directly controlling it.
        if (_agent.remainingDistance > _agent.stoppingDistance)
        {
            _charController.Move(_agent.desiredVelocity, false, false);
        }
        else if (!_agent.pathPending)
        {
            _charController.Move(Vector3.zero, false, false);
            
            _controller.OnAvatarReachedPosition(this, _agent.pathEndPosition); //triggers narration relate stuff
        }
    }

    //Erik claims that this part might be related to have the avatar behave correctly when making jumps in time.
    //I'm not convinced though... It seems to run only once when initiating an avatar, 
    //so the only chance it might relate to jumps in time is if _scheduleIndex is somehow
    //set from save on startup (now it's always 0 when entering the function, no?).
    //And my understanding is that the schedule is not part of save?
    //I'm now changing it to always find the closest upcoming activity in the schedule and choose it as _curActivity...

    //Soooo. What this function is doing now is: Choose which activity should be the current one given the current time. Also sets _prevActivity and _nextActivity
    public void SyncSchedule()
    {
        double curTime = _timeMgr.GetTotalSeconds();
        DateTime curDateTime = _timeMgr.GetDateTime();
        double curHour = curDateTime.Hour;
        double curMinute = curDateTime.Minute;

        DebugManager.Log("starting schedule sync", this);

        //TODO: Handle when last item in schedule doesnn't have start time!!

        //loop through the schedule until we pass an actvity that should already have happended
        for (; _scheduleIndex < schedule.Length; _scheduleIndex++)
        {
            //Handle schedule posts without time. Such activities should happen as soon as the activity before is finished. But since they have no startTime, we skip them when setting up the schedule.
            if (schedule[_scheduleIndex].time == null || schedule[_scheduleIndex].time == "")
            {
                //Ok. so this schedule post has no time specified. Let's loop further.
                continue;
            }
            string[] timeParse = schedule[_scheduleIndex].time.Split(':');
            int schedHour = int.Parse(timeParse[0]);
            int schedMinute = int.Parse(timeParse[1]);

            //Is this activity in the future? If so, dont pick it.
            if (curHour <= schedHour && _scheduleIndex+1 != schedule.Length)
            {
                //if same hour. also check minutes. are the minutes past.
                if (curHour == schedHour)
                {
                    if (curMinute < schedMinute && _scheduleIndex+1 != schedule.Length)
                    {
                        continue; // if same hour but minutes not yet past, don't pick this activity.
                    }
                }
                else
                {
                    continue;
                }
            }

            //Ok. Now we should have picked an index.
            //We set the dayoffsets depending on what index we picked
            int prevActivityDayOffset = 0;
            int curActivityDayOffset = 0;
            int nxtActivityDayOffset = 0;
            if (schedule.Length == 1)//In the super unsusual case when schedule only has one item
            {
                //Ok. this might seem weird. But since the length of schedule is 1, curitem is actually from yesterday. Since we picked the closest item earlier than curtime.
                prevActivityDayOffset = -2;
                curActivityDayOffset = -1;
                nxtActivityDayOffset = 0;
            }else if (_scheduleIndex == 0)//First item in schedule picked. prev is yesterday.
            {
                prevActivityDayOffset = -1;
                curActivityDayOffset = 0;
                nxtActivityDayOffset = 0;
                
            }else if(_scheduleIndex+1 == schedule.Length)//Ok. Last item in schedule picked. nxt izz tomarrah.
            {
                prevActivityDayOffset = -1;
                curActivityDayOffset = -1;
                nxtActivityDayOffset = 0;
            }
            //ok. We now have the current index picked. Let's set it as the _curActivity and the adjacent activities as prev and next.
            //To achieve that we must first calculate some timstamps and stuff...

            ////Hm. I change this now to instead set the next upcoming activity as _nextActivity (instead of _curActivity)!!! /Gunnar
            ////Decrement _scheduleIndex by 1 before we do all the stuffzz
            //_scheduleIndex = (_scheduleIndex + schedule.Length - 1) % schedule.Length;

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
            //int nxtActivityDayOffset = (int)Mathf.Floor((_scheduleIndex + 1) / schedule.Length);//OLD version

            //// set day offset for current activity. If it's the last index that means the curactivity is actually from yesterday. And nxt activity is today. This is because we compare start times.
            //int curActivityDayOffset = 0;
            //if (_scheduleIndex + 1 == schedule.Length)
            //{
            //    curActivityDayOffset = -1;
            //}
            //int nxtActivityDayOffset = 0;

            ////If current is at index 0 or curactivity has dayoffset than previous is the day before.
            //int prevActivityDayOffset = 0;
            ////Is the prevActivity started the previous day?
            //if (_scheduleIndex == 0) { prevActivityDayOffset = -1; }

            //Determine startTime for prevActivity
            double prevStartTime = 0;
            if (prevItem.time != "")
            {
                prevStartTime = _timeMgr.ScheduleToTS(curTime, prevActivityDayOffset, prevItem.time);
                SetPrevActivity(prevItem.activity, prevStartTime);
                DebugManager.Log("_prevActivity set: " + _prevActivity.name + " with startTime: " + _prevActivity.startTime, this.gameObject, this);
            }
            else
            {
                SetPrevActivity(prevItem.activity);
            }

            //Determine startTime as timeStamp. We set dayOffset to 0 since we should set the activity to this day.
            double startTime = 0;
            if (curItem.time != "")
            {
                startTime = _timeMgr.ScheduleToTS(curTime, curActivityDayOffset, curItem.time);
                SetCurrentActivity(curItem.activity, startTime);
                DebugManager.Log("_curActivity set: " + _curActivity.name + " with startTime: " + _curActivity.startTime, this.gameObject, this);
            }
            else
            {
                SetCurrentActivity(curItem.activity);
            }

            //Determine startTime for nextActivity
            double nxtStartTime = 0;
            if (nxtItem.time != "")
            {
                nxtStartTime = _timeMgr.ScheduleToTS(curTime, nxtActivityDayOffset, nxtItem.time);
                SetNextActivity(nxtItem.activity, nxtStartTime);
                DebugManager.Log("_nextActivity set: " + _nextActivity.name + " with startTime: " + _nextActivity.startTime, this.gameObject, this);
            }
            else
            {
                SetNextActivity(nxtItem.activity);
            }

            //DebugManager.Log("startActivity set for " + name + ". It's " + curItem.activity + " with startTimeStamp " + startTime, this);
            DebugManager.Log("syncSchedule finished. timestamp is: " + _timeMgr.GetTotalSeconds(), this.gameObject, this);
            return;
        }
    }

    //
    public void SetCurrentActivity(AvatarActivity activity, double startTime)
    {
        DebugManager.Log("setting current activity: " + activity, this);
        _curActivity = UnityEngine.Object.Instantiate(activity) as AvatarActivity;
        _curActivity.Init(this, startTime);
    }

    public void SetCurrentActivity(AvatarActivity activity)
    {
        DebugManager.Log("setting current activity: " + activity, this);
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
    //public void StartActivity(AvatarActivity activity)
    //{
    //    //Debug.Log(name + ".StartActivity(" + activity + ") with end time " + _timeMgr.time + " + " + (60 * 3));
        
    //    //activity.endTime = _timeMgr.time + 60 * 3;
    //    _curActivity = activity;
    //    _curActivity.Init(this);
    //    _curActivity.Start();
    //}

    public void StartTemporaryActivity(AvatarActivity activity)
    {
        DebugManager.Log(name + ". StartTemporaryActivity(" + activity + ")", this);
        _tempActivities.Push(activity);
        _tempActivities.Peek().Init(this);
        _tempActivities.Peek().Start();
    }

    //Alright. Let's pick the next activity in the schedule. This function updates the references of _prev, _cur and _next -activity.
    public void NextActivity()
    {
        //We will use the current time as reference timestamp for picking correct day when converting schedule timestring to epoch timestamp.
        double curTime = _timeMgr.GetTotalSeconds();

        DebugManager.Log("Next activity called. " + _nextActivity.name, this);
        //Iterate schedule index
        //Soo. _scheduleIndex is here incremented to point on the activity we jump to.
        _scheduleIndex = (_scheduleIndex + 1) % schedule.Length;

        //Get next schedule index with potential offset in days
        int nxtIndex = (_scheduleIndex + 1) % schedule.Length;
        int nxtActivityDayOffset = (int)Mathf.Floor((_scheduleIndex + 1) / schedule.Length);

        //Setup next schedule item
        ScheduleItem nxtItem = schedule[nxtIndex];

        //We have already an instantiated activity in _nextActivity. So we set _curActivity as _prevActivity and _nextActivity as _curActivity. Then we instantiate a new one for _nextActivity.
        _prevActivity = _curActivity;
        _curActivity = _nextActivity;


        if (nxtItem.time != "")
        {
            //Determine startTime for nextActivity
            double nxtStartTime = _timeMgr.ScheduleToTS(curTime, nxtActivityDayOffset, nxtItem.time);
            SetNextActivity(nxtItem.activity, nxtStartTime);
        }
        else
        {
            SetNextActivity(nxtItem.activity);
        }
    }

    //
    public void PreviousActivity()
    {
        //We will use the current time as reference timestamp for picking correct day when converting schedule timestring to epoch timestamp.
        double curTime = _timeMgr.GetTotalSeconds();

        int prevIndex = 0;
        if (schedule.Length > 0)
        {

            //Iterate schedule index
            _scheduleIndex = (_scheduleIndex + schedule.Length - 1) % schedule.Length;
            prevIndex = (_scheduleIndex + schedule.Length - 1) % schedule.Length;
        }

        int startTimeDayOffset = 0;
        //Is the new (previous) activity started the previous day?
        if (prevIndex > _scheduleIndex) { startTimeDayOffset = -1; }

        //Setup cur schedule item and the one after that
        ScheduleItem prevItem = schedule[prevIndex];

        //Move all the three activities one step back
        _nextActivity = _curActivity;
        _curActivity = _prevActivity;

        if (prevItem.time != "")
        {
            //Determine the startTime of the new _prevActivity,
            //using the _prevActivity as day reference
            //This means that if _prevActivity is already on the previous day, this will be the day reference for setting startTime. If prevIndex is on same day as _prevActivity dayOffset will be 0.
            //If prevIndex instead is one day before _scheduleIndex AND _prevActivity already is on previous day (should only be possible with a schedule of length 1 I think)
            //we will get one day back from _prevActivity day reference and additionally one day back from startTimeDayOffset.
            double prevActivityStartTime = _timeMgr.ScheduleToTS(curTime, startTimeDayOffset, prevItem.time);
            SetPrevActivity(prevItem.activity, prevActivityStartTime);
        }
        else
        {
            SetPrevActivity(prevItem.activity);
        }
    }

    ////
    //public void EndOverride()
    //{
    //    _curActivity.ResumeSession(this);
    //    //_curActivityState = ActivityState.Idle;
    //}

    ////
    //public void Interact()
    //{
    //}

    void SetAgentDestination(AvatarActivity activity)
    {
        SetAgentDestination(activity.GetCurrentTargetObject().GetComponent<Appliance>());
    }

    void SetAgentDestination(Appliance appliance)
    {
        SetAgentDestination(appliance.interactionPos);
    }

    void SetAgentDestination(Vector3 position)
    {
        _agent.SetDestination(position);
    }

    //
    public void Stop()
    {
        GetRunningActivity().SetCurrentAvatarState(AvatarActivity.AvatarState.Idle);
        _charController.Move(Vector3.zero, false, false);
    }

    public void Wait()
    {
        GetRunningActivity().SetCurrentAvatarState(AvatarActivity.AvatarState.Idle);
        _charController.Move(Vector3.zero, false, false);
    }

    //This is an override walk. Should be clearer that's the case.
    public void WalkTo(Vector3 target)
    {
        if (!_isControlled)
        {
            Debug.LogError("Hey! Your are trying to control an avatar without first calling TakeControlOfAvatar(). Call TakeControlOfAvatar(). Do Stuff. Then call ReleaseControlOfAvatar()", this);
        }
        //_curTargetPos = target;

        SetAgentDestination(target);
        _agent.updatePosition = true;
        //_curAvatarState = AvatarState.OverrideWalking;
    }

    public void TakeControlOfAvatar()
    {
        _isControlled = true;
    }

    public void ReleaseControlOfAvatar()
    {
        _isControlled = false;
    }

    //
    //public void WalkTo(string[] args) {
    //	_curTargetObj = FindNearestDevice(args[0], args.Length > 1 && args[1] == "own");
    //	if(_curTargetObj == null) { return; }

    //	_agent.SetDestination(_curTargetObj.GetComponent<Appliance>().interactionPos);
    //	_curActivityState = ActivityState.Walking;
    //}

    //Makes the avatar walks towards an object by setting the navmeshagent destination.
    public void WalkTo(Affordance affordance, bool isOwned)
    {
        //Appliance targetAppliance = FindNearestAppliance(target, isOwned);
        Appliance targetAppliance = GetApplianceForAffordance(affordance, isOwned);
        WalkTo(targetAppliance, isOwned);
    }

    //Let's use the reference to the appliance
    public void WalkTo(Appliance appliance, bool isOwned)
    {
        if (appliance == null)
        {
            DebugManager.LogError("Didn't find a WalkTo target. doing activity " + _curActivity.name + ". Skipping to next session", this);
            GetRunningActivity().NextSession();
            return;
        }

        DebugManager.Log("walking to appliance ", appliance, this);

        GetRunningActivity().SetCurrentTargetObject(appliance.gameObject);

        SetAgentDestination(appliance);
        GetRunningActivity().SetCurrentAvatarState(AvatarActivity.AvatarState.Walking);

        //If target is outside current room
        if (_curRoom != null && !_curRoom.IsPointInRoom(appliance.interactionPos))
        {
            DebugManager.Log("Walking to appliance in another room", appliance.gameObject, this);
            OnWalkToOtherRoom();
        }
    }

    //
    public void WarpTo(Affordance affordance, bool isOwned)
    {
        //Appliance appliance = FindNearestAppliance(target, isOwned);
        Appliance appliance = GetApplianceForAffordance(affordance, isOwned);
        WarpTo(appliance, isOwned);
    }

    public void WarpTo(Appliance appliance, bool isOwned)
    {
        if (appliance == null) {
            DebugManager.LogError("Didn't get an appliance to warp to!", this, this);
            return;
        }

        GetRunningActivity().SetCurrentTargetObject(appliance.gameObject);

        //appliance.AddHarvest(); // Why would we do this here!! Gunnar
        _agent.Warp(appliance.interactionPos);
    }

    //
    //public void TeleportTo(string[] args) {
    //	_curTargetObj = FindNearestDevice(args[0], args.Length > 1 && args[1] == "own");
    //	if(_curTargetObj == null) { return; }

    //	_curTargetObj.GetComponent<Appliance>().AddHarvest();
    //	_agent.Warp(_curTargetObj.GetComponent<Appliance>().interactionPos);
    //}

    //
    public void WarpToDestination()
    {
        _agent.Warp(_agent.destination);
    }

    public void SitAt(Affordance affordance, bool isOwned)
    {
        if(affordance == null)
        {
            DebugManager.LogError("Hey. You gave me a null affordance when trying to sit. What's up with that?! I'll skip to next session", this, this);
            GetRunningActivity().NextSession();
        }
        //Appliance appliance = FindNearestAppliance(target, isOwned).GetComponent<Appliance>();
        Appliance appliance = GetApplianceForAffordance(affordance, isOwned);
        SitAt(appliance);
    }

    public void SitAt(Appliance appliance)
    {
        if(appliance == null)
        {
            DebugManager.LogError("Didn't get a SitAt target appliance. doing activity " + _curActivity.name + ". Skipping to next session", this);
            GetRunningActivity().NextSession();
            return;
        }
        //Disable stuff before custom positioning
        _agent.enabled = false;
        GetComponent<Rigidbody>().isKinematic = true;

        //save the currentPosition for when standing up again.
        _savedStandingPosition = transform.position;

        //Get the child  game object with the name Sit Position
        Transform sitPosition = appliance.gameObject.transform.Find("Sit Position");
        if (sitPosition == null)
        {
            DebugManager.LogError("Didn't find a gameobject called Sit Position inside " + appliance.name, appliance.gameObject, this);
        }
        else
        {
            //coord.x = sitPosition.position.x;
            //coord.z = sitPosition.position.z;
            //coord.y = transform.position.y;
            transform.position = sitPosition.position;//coord;
            transform.rotation = sitPosition.rotation;
            Debug.Log("Setting avatar position to sitPosition from appliance object");
        }
        GetRunningActivity().SetCurrentAvatarState(AvatarActivity.AvatarState.Sitting);
        _charController.SitDown(); //Sets a boolean in the animator object
    }

    //
    public void SitAtCurrentTarget()
    {
        ////Disable stuff before custom positioning
        //_agent.enabled = false;
        //GetComponent<Rigidbody>().isKinematic = true;

        ////save the currentPosition for when standing up again.
        //_savedStandingPosition = transform.position;

        GameObject targetObject = GetRunningActivity().GetCurrentTargetObject();

        SitAt(targetObject.GetComponent<Appliance>());

        ////Let's search for a gameObject called Sit Position
        //if (targetObject == null)
        //{
        //    DebugManager.LogError("_curTargetObject not set!", this);
        //}

        //Transform sitPosition = targetObject.transform.Find("Sit Position");
        //if (sitPosition == null)
        //{
        //    DebugManager.LogError("Didn't find a gameobject called Sit Position inside " + targetObject.name, this);
        //}
        //else
        //{
        //    //coord.x = sitPosition.position.x;
        //    //coord.z = sitPosition.position.z;
        //    //coord.y = transform.position.y;
        //    transform.position = sitPosition.position;//coord;
        //    transform.rotation = sitPosition.rotation;
        //    Debug.Log("Setting avatar position to sitPosition from appliance object");
        //}
        //GetRunningActivity().SetCurrentAvatarState(AvatarState.Sitting);
        //_charController.SitDown(); //Sets a boolean in the animator object
    }

    public void standUp()
    {
        DebugManager.Log("Standing up.", this, this);
        if (_savedStandingPosition != null)
        {
            transform.position = _savedStandingPosition;
        }
        _agent.enabled = true;
        GetComponent<Rigidbody>().isKinematic = false;
        _charController.StandUp();

        //We don't set the _curAvatarState here, since standing up is performed before doing other stuffz

    }

    //
    public void SetRunLevel(Affordance affordance, int level, bool userOwnage = false)
    {
        //Appliance targetAppliance = FindNearestAppliance(target, false);
        Appliance targetAppliance = GetApplianceForAffordance(affordance, userOwnage);

        if (targetAppliance == null)
        {
            return;
        }

        SetRunLevel(targetAppliance, level);

		//TODO: temp solution
		targetAppliance.OnUsage(affordance);
	}

    public void SetRunLevel(Appliance appliance, int level)
    {
        if(appliance == null)
        {
            DebugManager.LogError("Didn't get an Appliance object for SetRunLevel! Skipping to next session", this);
            GetRunningActivity().NextSession();
			return;
        }

        GetRunningActivity().SetCurrentTargetObject(appliance.gameObject);

        DebugManager.Log("Setting runlevel for " + appliance, appliance, this);

        GameObject targetObject = appliance.gameObject;
        if (targetObject == null)
        {
            DebugManager.LogError("Didn't find device for setting runlevel", this);
            return;
        }

        ElectricDevice device = targetObject.GetComponent<ElectricDevice>();
        if (device == null)
        {
            DebugManager.LogError("Couldn't set runlevel. The provided gameobject doesn't have an electric device component", this);
            return;
        }

        device.SetRunlevel(level);
    }

    public void TurnOn(Affordance affordance, bool userOwnage = false)
    {
        //Appliance targetAppliance = FindNearestAppliance(target, false);
        Appliance targetAppliance = GetApplianceForAffordance(affordance, userOwnage);
        TurnOn(targetAppliance);
		//TODO: temp solution
		targetAppliance.OnUsage(affordance);
	}

    public void TurnOn(Appliance appliance)
    {
        DebugManager.Log("Turning on " + appliance, appliance, this);

        GetRunningActivity().SetCurrentTargetObject(appliance.gameObject);

        GameObject targetObject = appliance.gameObject;
        if (targetObject == null)
        {
            DebugManager.LogError("Didn't find device to turn on", this);
            return;
        }

        ElectricMeter meter = targetObject.GetComponent<ElectricMeter>();
        if (meter == null)
        {
            DebugManager.LogError("Didn't find electric meter to turn on", this);
            return;
        }
        meter.On();

    }

    public void TurnOff(Affordance affordance, bool userOwnage = false)
    {
        //Appliance targetAppliance = FindNearestAppliance(target, false);
        Appliance targetAppliance = GetApplianceForAffordance(affordance, userOwnage);
        TurnOff(targetAppliance);
		//TODO: temp solution
		targetAppliance.OnUsage(affordance);
	}

    public void TurnOff(Appliance appliance)
    {
        DebugManager.Log("Turning off " + appliance, appliance, this);

        GetRunningActivity().SetCurrentTargetObject(appliance.gameObject);

        GameObject targetObject = appliance.gameObject;
        if (targetObject == null)
        {
            DebugManager.LogError("Didn't find device to turn off", this);
            return;
        }

        ElectricMeter meter = targetObject.GetComponent<ElectricMeter>();
        if (meter == null)
        {
            DebugManager.LogError("Didn't find electric meter to turn off", this);
            return;
        }
        meter.Off();

    }

    //// Searches devices for device with nearest Euclidean distance which fullfill affordance and ownership
    //public Appliance FindNearestAppliance(AvatarActivity.Target affordance, bool isOwned)
    //{
    //    Appliance targetAppliance = null;
    //    float minDist = float.MaxValue;

    //    foreach (Appliance device in _devices)
    //    {
    //        List<AvatarActivity.Target> affordances = device.avatarAffordances_old;
    //        //if (affordances.Contains(affordance) && (!isOwned || device.owners.Contains(_stats.avatarName)))
    //        if (affordances.Contains(affordance) && (!isOwned || device.owners.Contains(this)))//Have changed owners to be actual references rather than names. Gunnar.
    //        {
    //            float dist = Vector3.Distance(transform.position, device.transform.position);
    //            if (dist < minDist)
    //            {
    //                minDist = dist;
    //                targetAppliance = device;
    //            }
    //        }
    //    }

    //    if (targetAppliance == null)
    //    {
    //        DebugManager.Log(name + " could not find the appliance with affordance: " + affordance.ToString(), this);
    //    }

    //    return targetAppliance;
    //}

    // Searches devices for device with nearest Euclidean distance which fullfill affordance and ownership
    public Appliance GetApplianceForAffordance(Affordance affordance, bool userOwnage)
    {
        Appliance targetAppliance = null;
        float minDist = float.MaxValue;

        foreach (Appliance app in _devices)
        {
            List<Affordance> affordances = app.avatarAffordances;
            if (affordances.Contains(affordance) && (!userOwnage || app.owners.Contains(this)))
            {
                float dist = Vector3.Distance(transform.position, app.transform.position);
                if (dist < minDist)
                {
                    minDist = dist;
                    targetAppliance = app;
                }
            }
        }

        if (targetAppliance == null)
        {
            if(userOwnage)
                DebugManager.LogError(name + " could not find an OWNED appliance with affordance: " + affordance.ToString(), this);
            else
                DebugManager.LogError(name + " could not find the appliance with affordance: " + affordance.ToString(), this);
        }

        DebugManager.Log("Min dist appliance: ", targetAppliance, this);

        return targetAppliance;
    }

    //
    public void OnActivityOver()
    {

        string activityName = GetRunningActivity().name;

        DebugManager.Log(name + "'s activity " + activityName + " is over", this);
        if(GetRunningActivity().GetCurrentAvatarState() == AvatarActivity.AvatarState.Sitting)
        {
            DebugManager.Log("avatar was sitting. Standing up.", this);
            standUp();
        }

        GetRunningActivity().SetCurrentAvatarState(AvatarActivity.AvatarState.Idle);

        if (_tempActivities.Count > 0)
        {
            //Notify the gamecontroller that we finished this activity!
            _controller.OnAvatarActivityComplete(_tempActivities.Peek().title);

            //Ok. so this overriding activity was finished. Remove it from the tempactivity stack.
            AvatarActivity finishedAvtivity = _tempActivities.Pop();

            DebugManager.Log("Teemporary activity finished!", finishedAvtivity, this);
            DebugManager.Log("Current target object now is: ", GetRunningActivity().GetCurrentTargetObject(), this);

            //_agent.SetDestination(GetRunningActivity().GetCurrentTargetObject().GetComponent<Appliance>().interactionPos);
            SetAgentDestination(GetRunningActivity());

            //We don't want to do moar stuffz in here. Bail out!
            return;
        }

        //Notify the gamecontroller that we finished this activity!
        _controller.OnAvatarActivityComplete(_curActivity.title);

        //Ok. If the next activity doesn't have a startTime we can go ahead and launch it immediately
        if (!_nextActivity.hasStartTime)
        {
            DebugManager.Log("Next activity have no startTime specified so let's start it immediately", this);
            NextActivity();
            _curActivity.Start();
        }
    }

    //This will get called when avatar (that have rigidbody and collider) collides with a collider. <--- wow, so many colliding words in one sentence!!!
    //Note that OnTriggerExit of old zone could be called AFTER OnTriggerEnter for new zone, hence all checks are done when entering a new room/zone and not when exiting /Martin
    void OnTriggerEnter(Collider other)
    {
        //Did the avatar collide with a roooom?
        if (other.GetComponent<Room>())
        {
            //Rooms/zones can have multiple collider boxes which each trigger a collision, hence this check
            if (other.GetComponent<Room>() != _curRoom)
            {
                if (_curRoom != null) //Is null when game starts
                {
                    //Exiting current room
                    DebugManager.Log(name + " exited current room " + _curRoom, other.gameObject, this);
                    _curRoom.OnAvatarExit(this); //Decrease person count in room
                }

                //Entering new room
                DebugManager.Log(name + " entered new room " + other.name, other.gameObject, this);
                _curRoom = other.GetComponent<Room>();
                _curRoom.OnAvatarEnter(this); //Increase person count in room
                CheckLighting(AvatarActivity.SessionType.TurnOn);
            }
        }
    }

    void OnWalkToOtherRoom()
    {
        _stats.TestEnergyEfficiency(AvatarStats.Efficiencies.Lighting, CheckLighting, AvatarActivity.SessionType.TurnOff);
    }

    public void CheckLighting(AvatarActivity.SessionType wantedAction)
    {
        if (_curRoom)
        {
            if (wantedAction == AvatarActivity.SessionType.TurnOn && _curRoom.IsLit())
            {
                //Light is ok: turned on
                return;
            }
            else if(wantedAction == AvatarActivity.SessionType.TurnOff && (!_curRoom.IsLit() || _curRoom.GetPersonCount() > 1))
            {
                //Light is ok: turned off
                return;
            }

            Appliance lightSwitch = _curRoom.GetLightSwitch();
            if (lightSwitch == null)
            {
                DebugManager.LogError(name + " couldn't find a lightswitch in this room!", _curRoom, this);
                return;
            }

            InitApplianceTemporaryActivity(_curRoom.GetLightSwitch(), wantedAction, "", true);

        }
    }

    

    //
    //public void CheckLighting(GameObject device)
    //{
    //    Room deviceRoom = device.GetComponentInParent<Room>();
    //    if (deviceRoom)
    //    {
    //        if (deviceRoom.lux < 1)
    //        {
    //            Appliance lightSwitch = deviceRoom.GetLightSwitch();
    //            if (lightSwitch)
    //            {
    //                QuickTurnLightOn(lightSwitch);
    //            }
    //        }
    //    }
    //}

    //

    public void InitApplianceTemporaryActivity(Appliance appliance, AvatarActivity.Session session, bool walkTo)
    {
        AvatarActivity activity = UnityEngine.ScriptableObject.CreateInstance<AvatarActivity>();
        activity.Init(this);//Just to make sure _curSession is 0 before we start injecting sessions into the activity

        activity.InsertSession(session);

        if (walkTo)
        {
            AvatarActivity.Session walkToSession = new AvatarActivity.Session();
            walkToSession.title = "Walking to appliance";
            walkToSession.type = AvatarActivity.SessionType.WalkTo;
            walkToSession.appliance = appliance;
            walkToSession.currentRoom = true;
            activity.InsertSession(walkToSession);
        }

        //Alright. We've built a super nice activity for turning on the light. Ledz ztart itt!
        StartTemporaryActivity(activity);
    }

    public void InitApplianceTemporaryActivity(Appliance appliance, AvatarActivity.SessionType sessionType, string parameter, bool walkTo)
    {
        DebugManager.Log("Yo! Gonna " + sessionType + " that appliance: ", appliance.gameObject, this);

        AvatarActivity activity = UnityEngine.ScriptableObject.CreateInstance<AvatarActivity>();
        activity.Init(this);//Just to make sure _curSession is 0 before we start injecting sessions into the activity

        //Let's build relevant sessions and inject them into the activity we just created.
        AvatarActivity.Session interactSession = new AvatarActivity.Session();
        interactSession.title = "Interact with appliance";
        interactSession.type = sessionType;
        interactSession.parameter = parameter;
        interactSession.appliance = appliance;
        activity.InsertSession(interactSession);

        if (walkTo)
        {
            AvatarActivity.Session walkToSession = new AvatarActivity.Session();
            walkToSession.title = "Walking to appliance";
            walkToSession.type = AvatarActivity.SessionType.WalkTo;
            walkToSession.appliance = appliance;
            walkToSession.currentRoom = true;
            activity.InsertSession(walkToSession);
        }

        //Alright. We've built a super nice activity for turning on the light. Ledz ztart itt!
        StartTemporaryActivity(activity);
    }

    /*public void UseLightSwitch(Appliance lightSwitch, bool turnOn)
    {
        DebugManager.Log("Yo! Gonna flip that lamp switch to " + turnOn, lightSwitch.gameObject, this);
        //Create an activity for turning on the laaajt!
        AvatarActivity roomLightActivity = UnityEngine.ScriptableObject.CreateInstance<AvatarActivity>();
        roomLightActivity.Init(this);//Just to make sure _curSession is 0 before we start injecting sessions into the activity

        //Let's build relevant sessions and inject them into the activity we just created.
        AvatarActivity.Session walkToLightSwitch = new AvatarActivity.Session();
        walkToLightSwitch.title = "Walking to light switch";
        walkToLightSwitch.type = AvatarActivity.SessionType.WalkTo;
        walkToLightSwitch.target = AvatarActivity.Target.LampSwitch; //Is this needed if we instead set appliance below? I didn't remove this, if it would mess something else up. /Martin
        walkToLightSwitch.appliance = lightSwitch;
        walkToLightSwitch.currentRoom = true;

        AvatarActivity.Session switchLight = new AvatarActivity.Session();
        switchLight.title = turnOn ? "Turning on light" : "Turning off light";
        switchLight.type = turnOn ? AvatarActivity.SessionType.TurnOn : AvatarActivity.SessionType.TurnOff;
        switchLight.appliance = lightSwitch;

        //Insert at beginning (register in inverted performance order)
        roomLightActivity.InsertSession(switchLight);
        roomLightActivity.InsertSession(walkToLightSwitch);

        //Alright. We've built a super nice activity for turning on the light. Ledz ztart itt!
        StartTemporaryActivity(roomLightActivity);
    }*/

    ////
    //public void QuickTurnLightOn(Appliance lightSwitch)
    //{
    //    AvatarActivity.Session walkToLightSwitch = new AvatarActivity.Session();
    //    walkToLightSwitch.type = AvatarActivity.SessionType.WalkTo;
    //    walkToLightSwitch.target = AvatarActivity.Target.LampSwitch;
    //    walkToLightSwitch.currentRoom = true;

    //    AvatarActivity.Session turnOnLight = new AvatarActivity.Session();
    //    turnOnLight.title = "Turn on light";
    //    turnOnLight.type = AvatarActivity.SessionType.SetRunlevel;
    //    turnOnLight.target = AvatarActivity.Target.LampSwitch;
    //    turnOnLight.parameter = "0";

    //    _curActivity.InsertSession(turnOnLight);
    //    _curActivity.InsertSession(walkToLightSwitch);
    //}

    // Save function
    public JSONClass Encode()
    {
        JSONClass json = new JSONClass();
        json.Add("name", name);
        Vector3 position = transform.position;
        json.Add("transform", JsonUtility.ToJson(position));
        json.Add("savedStandingPosition", JsonUtility.ToJson(_savedStandingPosition));
        //json.Add("scheduleIndex", _scheduleIndex.ToString());
        json.Add("_curActivity", _curActivity.Encode());
        json.Add("_nextActivity", _nextActivity.Encode());
        json.Add("_prevActivity", _prevActivity.Encode());
        json.Add("tempActivities", EncodeActivityStack());
        //Should be mooore here!

        json.Add("object", JsonUtility.ToJson(this));

        return json;
    }

    // Load function
    public void Decode(JSONClass json)
    {
        //_scheduleIndex = json["scheduleIndex"].AsInt;
        //JsonUtility.FromJsonOverwrite(json["object"], this);
        transform.position = JsonUtility.FromJson<Vector3>(json["transform"]);
        _savedStandingPosition = JsonUtility.FromJson<Vector3>(json["savedStandingPosition"]);

        _curActivity.Decode(json["_curActivity"]);
        _nextActivity.Decode(json["_nextActivity"]);
        _prevActivity.Decode(json["_prevActivity"]);

        //Should be more here!
    }

    private JSONArray EncodeActivityStack()
    {
        SimpleJSON.JSONArray arr = new JSONArray();
        while (_tempActivities.Count > 0)
        {
            AvatarActivity act = _tempActivities.Pop();
            //TODO: Check whether this serialization round-trips successfully.
            arr.Add(act.Encode());
        }
        return arr;
    }

    private void DecodeActivityStack(JSONArray arr) {
        for(int i = 0; i < arr.Count; i++)
        {
            string act = arr[i];
            _tempActivities.Push(JsonUtility.FromJson<AvatarActivity>(act));
        }
    }

    //This was actually pretty hard to achieve since a lot of the important fields are private... Sooo. fuck it! / GuNnAr
    //public void TestSerializationRoundTrip()
    //{
    //    //Ok. First create a new avatar from the prefab for comparison
    //    GameObject objectInstance = Instantiate(prefab);
    //    BehaviourAI compareAI = objectInstance.GetComponent<BehaviourAI>();

    //    JSONClass encodedAI = Encode();
    //    compareAI.Decode(encodedAI);

    //    //Now we should compare the two behavious instances and see if they're equal. If they are, that implies the serialization is all good.
    //    if(compareAI._curRoom == _curRoom)
    //    {
    //        DebugManager.Log("Is good", this);
    //    }


    //}
}
