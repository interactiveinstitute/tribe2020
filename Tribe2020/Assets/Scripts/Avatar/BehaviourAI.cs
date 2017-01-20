using UnityEngine;
using System.Collections.Generic;
using UnityStandardAssets.Characters.ThirdPerson;
using SimpleJSON; // For encoding and decoding avatar states
using System;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(ThirdPersonCharacter))]
[System.Serializable]
public class BehaviourAI : SimulationObject
{
    [SerializeField]
    private PilotController _controller;
    private GameTime _timeMgr;

    [SerializeField]
    private AvatarStats _stats;
    private NavMeshAgent _agent;
    private ThirdPersonCharacter _charController;
    [SerializeField]
    private Vector3 _savedIdlePosition;

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
    public int _nrOfActiveColliders = 0;

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
    private List<GameObject> thingsInHands;
    [SerializeField]
    public bool showCoffeeCup = false;


    //[SerializeField]
    //private GameObject prefab;

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
        _curActivity.Start(); //Start this activity.

        if (_nextActivity!= null)
        {
            //Add key point for _nextActivity.
            if(_timeMgr.AddKeypoint(_nextActivity.startTime, this))
            {
                DebugManager.Log("added key action point", this, this);
            }else
            {
                DebugManager.LogError("Failed to add keyactionpoint. Instead it was run immediately", this, this);
            }
        }else
        {
            DebugManager.LogError("_nextActivity is null!!", this, this);
        }
    }

    //Update simulation
    override public bool UpdateSim(double time)
    {
        //TODO: Try to only use the provided timestamp instead of referencing gametime
        DebugManager.Log("UpdateSim: calling keyaction registered by BehaviourAI", this, this);
        //Run everything that should be finished until now
        SimulateUntilNextScheduledActivity();
        //Correct activity should now be available with getCurrent()
        GetRunningActivity().Start();
        //Find the next scheduled activity and set it as key point
        _timeMgr.AddKeypoint(TimeStampForNextScheduledActivity(), this);
        return true;
    }

    private void SimulateUntilNextScheduledActivity()
    {
        //First. Simulate current activity to end
        GetRunningActivity().SimulateToEnd();
        //Simulate activities until we reach one with startTime
        while (!GetRunningActivity().hasStartTime)
        {
            //This should do nothing if the activity is already finished. If it's not finished it'll simulate the remaining sessions.
            //If it's a tempactivity it will remove itself from the tempstack and getrunning will refer to a new one (next in stack or _cur)
            //If the next activity doesn't have starttime it will also be set as getrunning. Hence we can run this while loop with getrunning() referring to a new one each iteration.
            GetRunningActivity().SimulateToEnd();
        }
        //When it reaches a scheduled activity with startTime, though, that one will NOT get started and become GetRunning().
        
        //Thus, at this point, the above code should have simulated activities that are temporary or doesn't have starttime.
        NextActivity();//Change to next activity (sets it as _curActivity)
    }

    private double TimeStampForNextScheduledActivity()
    {
        //We will use the current time as reference timestamp for picking correct day when converting schedule timestring to epoch timestamp.
        double curTime = _timeMgr.GetTotalSeconds();
        int nxtIndex = _scheduleIndex;
        //get offset in days
        int nxtActivityDayOffset = (int)Mathf.Floor((_scheduleIndex + 1) / schedule.Length);

        //Loop through schedule until we find an activity with startTime
        //Also, let's limit the loop to the length of the schedule. If no activity with a startTime is found that means there is no activity with startTime in the schedule.
        for (int i = 0; i < schedule.Length; i++)
        {
            //Get next schedule index
            nxtIndex = (nxtIndex + 1) % schedule.Length;
            if(schedule[nxtIndex].time == "")
            {
                //This activity has no scheduled startTime
                continue;
            }
            //Determine startTime for nextActivity
            return _timeMgr.ScheduleToTS(curTime, nxtActivityDayOffset, schedule[nxtIndex].time);
        }

        DebugManager.LogError("Couldn't find an activity with startTime in schedule. Double check the schedule of the avatar", this, this);
        return 0;
    }

    // Update is called once per frame
    void Update()
    {

        UpdateCoffeeCup();

        if (_tempActivities.Count > 0)
        {
            //Handle schedule overriding activities
            UpdateActivity(_tempActivities.Peek());
            return;
        }

        if (_isControlled)
        {
            //Oh snap! This avatar is controlled directly. Let's not care about activities for now then.
            UpdateCharController();
            return;
        }
        
        // Start activities on scheduled times.
        // These two if statements are meant to handle jumps in time. If curTime suddenly is increased a lot
        // the script will try to simulate the schedule until the curTime is reached.

        //Ok. Let's find the stamp for the next activity. Should we switch to it?
        //This logic should now be handled by gametime through simulationobject callbacks
        //if (_nextActivity != null && _nextActivity.hasStartTime && _nextActivity.startTimePassed())
        //{
        //    DebugManager.Log("Next activity's startTime (" + _nextActivity.startTime + ") passed. Finish current one and start the next one", this);
        //    _curActivity.FinishCurrentActivity();
        //    NextActivity();
        //    _curActivity.Start();
        //}

        //same as above but backwards.
        //We are most likely not handling backwards time very well. But that's out of scope for now.
        //if (_curActivity != null && _curActivity.hasStartTime && !_curActivity.startTimePassed())
        //{
        //    DebugManager.Log("Current time " + _timeMgr.GetTotalSeconds() + " is before current activity's startTime " + _curActivity.startTime + ". Revert the activity and start the previous one", this);
        //    _curActivity.Revert();
        //    PreviousActivity();
        //    _curActivity.Start();
        //}

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
                    //We don't want to come back in here after we reached the destination. So let's change state to idle.
                    GetRunningActivity().SetCurrentAvatarState(AvatarActivity.AvatarState.Idle);

                    //Ok. Let's notify the activity that the current session is finished
                    activity.OnDestinationReached();
                    
                    //	_curActivityState = ActivityState.Idle;
                    _controller.OnAvatarReachedPosition(this, _agent.pathEndPosition); //triggers narration relate stuff
                                                                                       //}
                }
                break;
            case AvatarActivity.AvatarState.Posing:
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

    public AvatarActivity GetRunningActivity()
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

    //Soooo. What this function is doing is: Choose which activity should be the current one given the current time. Also sets _prevActivity and _nextActivity
    private void SyncSchedule()
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

            //Setup the scheduleIndices.
            int nxtIndex = (_scheduleIndex + 1) % schedule.Length;
            int prevIndex = (_scheduleIndex + schedule.Length - 1) % schedule.Length;
            //Get the schedule items
            ScheduleItem prevItem = schedule[prevIndex];
            ScheduleItem curItem = schedule[_scheduleIndex];
            ScheduleItem nxtItem = schedule[nxtIndex];

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
            
            DebugManager.Log("syncSchedule finished. timestamp is: " + _timeMgr.GetTotalSeconds(), this.gameObject, this);
            return;
        }
    }

    //
    private void SetCurrentActivity(AvatarActivity activity, double startTime)
    {
        DebugManager.Log("setting current activity: " + activity, this);
        _curActivity = UnityEngine.Object.Instantiate(activity) as AvatarActivity;
        _curActivity.Init(this, startTime);
    }

    private void SetCurrentActivity(AvatarActivity activity)
    {
        DebugManager.Log("setting current activity: " + activity, this);
        _curActivity = UnityEngine.Object.Instantiate(activity) as AvatarActivity;
        _curActivity.Init(this);
    }

    private void SetNextActivity(AvatarActivity activity, double startTime)
    {
        _nextActivity = UnityEngine.Object.Instantiate(activity) as AvatarActivity;
        _nextActivity.Init(this, startTime);
    }

    private void SetNextActivity(AvatarActivity activity)
    {
        _nextActivity = UnityEngine.Object.Instantiate(activity) as AvatarActivity;
        _nextActivity.Init(this);
    }

    private void SetPrevActivity(AvatarActivity activity, double startTime)
    {
        _prevActivity = UnityEngine.Object.Instantiate(activity) as AvatarActivity;
        _prevActivity.Init(this, startTime);
    }

    private void SetPrevActivity(AvatarActivity activity)
    {
        _prevActivity = UnityEngine.Object.Instantiate(activity) as AvatarActivity;
        _prevActivity.Init(this);
    }

    public void StartTemporaryActivity(AvatarActivity activity)
    {
        DebugManager.Log(name + ". StartTemporaryActivity(" + activity + ")", this);
        activity.isTemporary = true;
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
        if (_agent.isActiveAndEnabled)
        {
            _agent.SetDestination(position);
        }
    }

    //
    public void Stop()
    {
        GetRunningActivity().SetCurrentAvatarState(AvatarActivity.AvatarState.Idle);
        SetAgentDestination(transform.position);
        //_charController.Move(Vector3.zero, false, false);
    }

    public void Wait()
    {
        //GetRunningActivity().SetCurrentAvatarState(AvatarActivity.AvatarState.Idle);
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
    public Appliance WalkTo(Affordance affordance, bool isOwned)
    {
        //Appliance targetAppliance = FindNearestAppliance(target, isOwned);
        Appliance targetAppliance = GetApplianceForAffordance(affordance, isOwned);
        WalkTo(targetAppliance, isOwned);
        return targetAppliance;
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
        if (_curRoom != null && !_curRoom.IsObjectInRoom(appliance.gameObject))
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

    public void changePoseTo(string pose)
    {
        //Just in case
        _charController.TurnOffAllBools();

        ////save the currentPosition for when standing up again.
        //_savedIdlePosition = transform.position;

        _charController.SetPose(pose);
    }

    //
    public void PoseAtCurrentTarget(string pose)
    {
        GameObject targetObject = GetRunningActivity().GetCurrentTargetObject();

        ChangePoseAt(pose, targetObject.GetComponent<Appliance>());
    }

    public void ChangePoseAt(string pose, Affordance affordance, bool isOwned)
    {
        if (affordance == null)
        {
            DebugManager.LogError("Hey. You gave me a null affordance when trying to pose. What's up with that?! I'll skip to next session", this, this);
            GetRunningActivity().NextSession();
            return;
        }
        //Appliance appliance = FindNearestAppliance(target, isOwned).GetComponent<Appliance>();
        Appliance appliance = GetApplianceForAffordance(affordance, isOwned);
        ChangePoseAt(pose, appliance);
    }

    public void ChangePoseAt(string pose, Appliance appliance)
    {
        if (appliance == null)
        {
            DebugManager.LogError("Didn't get a PoseAt target appliance. doing activity " + _curActivity.name + ". Skipping to next session", this);
            GetRunningActivity().SetCurrentAvatarState(AvatarActivity.AvatarState.Idle);
            GetRunningActivity().NextSession();
            return;
        }
        //Disable stuff before custom positioning
        _agent.enabled = false;
        GetComponent<Rigidbody>().isKinematic = true;

        //save the currentPosition for when standing up again.
        _savedIdlePosition = transform.position;

        //Get the child  game object with the name Sit Position
        //Transform posePosition = appliance.gameObject.transform.Find("Pose Position");
        //if (posePosition == null)
        //{
        //    DebugManager.LogError("Didn't find a gameobject called Pose Position inside " + appliance.name, appliance.gameObject, this);
        //}
        //else
        //{
        //    //coord.x = sitPosition.position.x;
        //    //coord.z = sitPosition.position.z;
        //    //coord.y = transform.position.y;
        //    transform.position = posePosition.position;//coord;
        //    transform.rotation = posePosition.rotation;
        //    Debug.Log("Setting avatar position to posePosition from appliance object");
        //}
        Appliance.PoseSlot emptySlot = null;
        foreach(Appliance.PoseSlot slot in appliance.posePositions)
        {
            if(slot.occupant == null)//Is it a free slot with no occupant?
            {
                emptySlot = slot;
            }
        }
        if(emptySlot == null)
        {
            //No available position for posing
            //Enable stuff before return
            _agent.enabled = true;
            GetComponent<Rigidbody>().isKinematic = false;
            //GetRunningActivity().SetCurrentAvatarState(AvatarActivity.AvatarState.Posing);
            DebugManager.Log("no free pose slots in/at the appliance!", appliance, this);
            //Set avatarState accordingly!!!! We never posed so set to idle.
            GetRunningActivity().SetCurrentAvatarState(AvatarActivity.AvatarState.Idle);
            //Bail out!
            return;
        }
        else {//We found a slot. Hurray!
            emptySlot.occupant = this;
            transform.position = emptySlot.position;//coord;
            transform.rotation = emptySlot.rotation;

            GetRunningActivity().SetCurrentTargetObject(appliance.gameObject);

            changePoseTo(pose);

            GetRunningActivity().SetCurrentAvatarState(AvatarActivity.AvatarState.Posing);
        }


        
        //_charController.SetPose("Sit");
    }

    void ShowCoffeeCup()
    {
        GameObject cup = thingsInHands[0];
        if (cup) {
            cup.SetActive(true);
            return;
        }
        DebugManager.LogError("din't find the Cup in the avatars game object hierarchy", this, this);
    }

    void HideCoffeeCup()
    {
        if(thingsInHands.Count == 0)
        {
            //DebugManager.LogError("thingsInHands has 0 in length", this, this);
            return;
        }
        GameObject cup = thingsInHands[0];
        if (cup)
        {
            cup.SetActive(false);
            return;
        }
        DebugManager.LogError("din't find the Cup in the avatars game object hierarchy", this, this);
    }

    void UpdateCoffeeCup()
    {
        if (showCoffeeCup)
        {
            ShowCoffeeCup();
        }else
        {
            HideCoffeeCup();
        }
    }

    //public void SitAt(Affordance affordance, bool isOwned)
    //{
    //    if(affordance == null)
    //    {
    //        DebugManager.LogError("Hey. You gave me a null affordance when trying to sit. What's up with that?! I'll skip to next session", this, this);
    //        GetRunningActivity().NextSession();
    //    }
    //    //Appliance appliance = FindNearestAppliance(target, isOwned).GetComponent<Appliance>();
    //    Appliance appliance = GetApplianceForAffordance(affordance, isOwned);
    //    SitAt(appliance);
    //}

    //public void SitAt(Appliance appliance)
    //{
    //    if(appliance == null)
    //    {
    //        DebugManager.LogError("Didn't get a SitAt target appliance. doing activity " + _curActivity.name + ". Skipping to next session", this);
    //        GetRunningActivity().NextSession();
    //        return;
    //    }
    //    //Disable stuff before custom positioning
    //    _agent.enabled = false;
    //    GetComponent<Rigidbody>().isKinematic = true;

    //    //save the currentPosition for when standing up again.
    //    _savedStandingPosition = transform.position;

    //    //Get the child  game object with the name Sit Position
    //    Transform sitPosition = appliance.gameObject.transform.Find("Sit Position");
    //    if (sitPosition == null)
    //    {
    //        DebugManager.LogError("Didn't find a gameobject called Sit Position inside " + appliance.name, appliance.gameObject, this);
    //    }
    //    else
    //    {
    //        //coord.x = sitPosition.position.x;
    //        //coord.z = sitPosition.position.z;
    //        //coord.y = transform.position.y;
    //        transform.position = sitPosition.position;//coord;
    //        transform.rotation = sitPosition.rotation;
    //        Debug.Log("Setting avatar position to sitPosition from appliance object");
    //    }
    //    GetRunningActivity().SetCurrentAvatarState(AvatarActivity.AvatarState.Sitting);


    //    //_charController.SitDown(); //Sets a boolean in the animator object
    //    changePoseTo("Sit");
    //    //_charController.SetPose("Sit");
    //}

    public void ReturnToIdlePose()
    {
        DebugManager.Log("Returning to normal pose.", this, this);
        if (_savedIdlePosition != null)
        {
            transform.position = _savedIdlePosition;
        }
        _agent.enabled = true;
        GetComponent<Rigidbody>().isKinematic = false;

        GetRunningActivity().GetCurrentTargetObject().GetComponent<Appliance>().ReleasePoseSlot(this);

        //_charController.TurnOffAllBools();
        //_charController.StandUp();
        changePoseTo("Idle");

        //We don't set the _curAvatarState here, since standing up is performed before doing other stuffz

    }

    //
    public void SetRunLevel(Affordance affordance, int level, bool userOwnage = false)
    {
		if(affordance == null) {
			DebugManager.LogError("No affordance provided to the SetRunLevel function!", this, this);
		}
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
        //Add this to the list of devices that the avatar turned on.
        GetRunningActivity().turnedOnDevices.Add(device);
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

    public void InteractWithAvatar(Affordance affordance)
    {
        BehaviourAI targetAvatar = GetAvatarForAffordance(affordance);
        InteractWithAvatar(targetAvatar, affordance);
	}

    public void InteractWithAvatar(BehaviourAI targetAvatar, Affordance affordance)
    {
        DebugManager.Log(name + " is starts interacting with " + targetAvatar, this, this);
        if (targetAvatar != null)
        {
            //Only interact with new avatar if you, and the other other avatar, are not already busy interacting.
            if (!GetComponent<AvatarMood>().IsInteracting() && !targetAvatar.GetComponent<AvatarMood>().IsInteracting())
            {
                GetComponent<AvatarMood>().StartNewInteraction(affordance);
                targetAvatar.GetComponent<AvatarMood>().StartNewInteraction(affordance);
                TalkToOtherAvatarEmoji(targetAvatar);
            }
        }
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
    public Appliance GetApplianceForAffordance(Affordance affordance, bool userOwnage = false)
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

    public BehaviourAI GetAvatarForAffordance(Affordance affordance)
    {
        BehaviourAI targetAvatar = null;
        float minDist = float.MaxValue;

        foreach (GameObject avatar in GameObject.FindGameObjectsWithTag("Avatar"))
        {
            if (avatar != gameObject) //Ignore yourself - find another avatar
            {
                List<Affordance> affordances = new List<Affordance>(avatar.GetComponent<Appliance>().avatarAffordances);
                affordances.AddRange(avatar.GetComponent<Appliance>().GetTemporaryAvatarAffordances());
                if (affordances.Contains(affordance))
                {
                    float dist = Vector3.Distance(transform.position, avatar.transform.position);
                    if (dist < minDist)
                    {
                        minDist = dist;
                        targetAvatar = avatar.GetComponent<BehaviourAI>();
                    }
                }
            }
        }

        return targetAvatar;
    }

    public bool HasAffordance(Affordance affordance)
    {
        List<Affordance> affordances = new List<Affordance>(GetComponent<Appliance>().avatarAffordances);
        affordances.AddRange(GetComponent<Appliance>().GetTemporaryAvatarAffordances());
        if (affordances.Contains(affordance))
        {
            return true;
        }
        return false;
    }

    //This function should handle all logic related to the avatar when an activity is finished.
    //It also starts the next activity if it has no starttime.
    public void OnActivityOver()
    {

        string activityName = GetRunningActivity().name;

        DebugManager.Log(name + "'s activity " + activityName + " is over", this);

        AvatarActivity.AvatarState state = GetRunningActivity().GetCurrentAvatarState();
        if (state == AvatarActivity.AvatarState.Posing)
        {
            DebugManager.Log("avatar was posing. Returning to idle pose.", this);
            ReturnToIdlePose();
        }

        //Check if we should turn off stuff when ending this activity.
        if (GetRunningActivity().turnedOnDevices != null)
        {
            foreach (ElectricDevice device in GetRunningActivity().turnedOnDevices)
            {
                if (_stats.TestEnergyEfficiency())
                {
                    DebugManager.Log("The Avatar was energy aware now and turned off the device", device, this);
                    device.SetRunlevel(0);
                }
                else
                {
                    DebugManager.Log("The Avatar was not energy aware now and skipped turning off device", device, this);
                }
            }
        }

        //Notify the gamecontroller that we finished this activity!
        _controller.OnAvatarActivityComplete(GetRunningActivity().title);

        //Are we in a tempactivity?
        if (_tempActivities.Count > 0)
        {
            //Ok. so this temporary activity was finished. Remove it from the tempactivity stack.
            AvatarActivity finishedActivity = _tempActivities.Pop();

            DebugManager.Log("Temporary activity finished!", finishedActivity, this);
            DebugManager.Log("Current target object now is: ", GetRunningActivity().GetCurrentTargetObject(), this);
            
            //We must make the avatar continue to walk towards the "previous" target, since the tempactivity might have changed the agentdestination, creating a mismatch between curTarget and agentdestination. 
            //SetAgentDestination(GetRunningActivity());

            ////We don't want to do moar stuffz in here. Bail out!
            //return;
        }





        //Ok. If the next activity doesn't have a startTime we can go ahead and launch it immediately
        if (!_nextActivity.hasStartTime)
        {
            DebugManager.Log("Next activity have no startTime specified so let's start it immediately", this);
            NextActivity();
            _curActivity.Start();
        }
    }

    //This will get called when activity is about to get finished. We want to check if the avatar is ernegy efficient enough to turn off the things that are turned on.

    //This will get called when avatar (that have rigidbody and collider) collides with a collider. <--- wow, so many colliding words in one sentence!!!
    //Note that OnTriggerExit of old zone could be called AFTER OnTriggerEnter for new zone, hence all checks are done when entering a new room/zone and not when exiting /Martin
    void OnTriggerEnter(Collider other)
    {

        //Did the avatar collide with a roooom?
        if (other.GetComponent<Room>()) {
            //Rooms/zones can have multiple collider boxes which each trigger a collision, hence this check
            if (other.GetComponent<Room>() != _curRoom) {
                if (_curRoom) {
                    OnExitCurrentRoom();
                }
                //If it's a new room and this is the first collider in that room, the counter should be 1
                _nrOfActiveColliders = 1;
                //This will (among other things) set _curRoom, so it doesn't double trig.
                OnEnterNewRoom(other.GetComponent<Room>());
            }
            else {
                //Triggering collision in the same room that we're already inside. Increment the counter. (This is related to that we can have several colliders in the same room.
                _nrOfActiveColliders++;
            }
        }
    }

    void OnTriggerExit(Collider other)
    {

        //Did the avatar collide with a roooom?
        if (other.GetComponent<Room>()) {
            //Rooms/zones can have multiple collider boxes which each trigger a collision, hence this check
            if (other.GetComponent<Room>() == _curRoom) {
                _nrOfActiveColliders--;
                if (_nrOfActiveColliders == 0) {
                    OnExitCurrentRoom();
                    _curRoom = null;
                }
                else if (_nrOfActiveColliders < 0) {
                    DebugManager.LogError("Something is fucked up with collisiondetection. nrOfActiveColliders is less than 0!", this, this);
                }
            }
        }
    }

    void OnWalkToOtherRoom()
    {
        _stats.TestEnergyEfficiency(CheckLighting, AvatarActivity.SessionType.TurnOff);
    }

    void OnEnterNewRoom(Room room)
    {
        //Entering new room
        DebugManager.Log(name + " entered new room " + room.name, room.gameObject, this);
        _curRoom = room;
        _curRoom.OnAvatarEnter(this); //Increase person count in room

        if (GetRunningActivity().GetCurrentTargetObject() != null && _curRoom.IsObjectInRoom(GetRunningActivity().GetCurrentTargetObject()))
        {
            CheckLighting(AvatarActivity.SessionType.TurnOn);
        }
    }

    void OnExitCurrentRoom()
    {
        if (_curRoom != null) //Is null when game starts
        {
            //Exiting current room
            DebugManager.Log(name + " exited current room " + _curRoom, _curRoom.gameObject, this);
            _curRoom.OnAvatarExit(this); //Decrease person count in room
        }
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

            //Appliance lightSwitch = _curRoom.GetLightSwitch();
            Appliance lightSwitch = _curRoom.GetApplianceWithAffordance(_curRoom.avatarAffordanceSwitchLight);
            if (lightSwitch == null)
            {
                DebugManager.LogError(name + " couldn't find a lightswitch in this room!", _curRoom, this);
                return;
            }

            //Init temporary activity
            InitApplianceTemporaryActivity(lightSwitch, wantedAction, "", true, true);

        }
    }

    void TalkToOtherAvatarEmoji(BehaviourAI other)
    {
        AvatarMood.Mood mood = GetComponent<AvatarMood>().GetCurrentMood();
        AvatarConversation.EnvironmentLevel environmentLevel = AvatarConversation.EnvironmentLevel.neutral; //Change to avatar markov state

        //DebugManager.Log(name + " talks " + mood +" to " + other.name, this, this);
        
        AvatarConversation.EmojiLine line = GameObject.Find("AvatarManager").GetComponent<AvatarConversation>().GenerateEmojiLine(environmentLevel, mood);
        transform.FindChild("Canvas/Speech/EmojiReaction").GetComponent<SpriteRenderer>().sprite = line.emojiReaction;
        StartCoroutine(other.ListenToOtherAvatarEmoji(this, environmentLevel, mood));
    }

    public System.Collections.IEnumerator ListenToOtherAvatarEmoji(BehaviourAI other, AvatarConversation.EnvironmentLevel environmentLevel, AvatarMood.Mood moodInput)
    {
        AvatarConversation.EnvironmentLevel environmentLevelNew = environmentLevel;
        
        transform.Find("Canvas/Speech/EmojiReaction").GetComponent<SpriteRenderer>().sprite = null;

        bool continueTalking = HasAffordance(GetComponent<AvatarMood>().GetCurrentInteractionAffordance());

        if (continueTalking)
        {
            //Be polite, be quiet when other creatures are talking
            transform.Find("Canvas/Speech/EmojiReaction").GetComponent<SpriteRenderer>().sprite = null;

            yield return new WaitForSeconds(2);
            AvatarMood.Mood moodNew = GetComponent<AvatarMood>().TryChangeMood(moodInput);
            TalkToOtherAvatarEmoji(other);
        }
        else
        {
            AvatarMood.Mood moodNew = GetComponent<AvatarMood>().TryChangeMood(moodInput);
            EndTalkToOtherAvatar();
            other.EndTalkToOtherAvatar();
        }
    }

    public void EndTalkToOtherAvatar()
    {
        transform.Find("Canvas/Speech/EmojiReaction").GetComponent<SpriteRenderer>().sprite = null;
        GetComponent<AvatarMood>().EndInteraction();
    }

    public void InitApplianceTemporaryActivity(Appliance appliance, AvatarActivity.Session session, bool walkTo)
    {
        AvatarActivity activity = UnityEngine.ScriptableObject.CreateInstance<AvatarActivity>();
        activity.Init(this);//Make sure _curSession is 0 before we start injecting sessions into the activity

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

    public void InitApplianceTemporaryActivity(Appliance appliance, AvatarActivity.SessionType sessionType, string parameter, bool walkTo, bool returnToPrevActivity)
    {
        DebugManager.Log("Yo! Gonna " + sessionType + " that appliance: ", appliance.gameObject, this);

        AvatarActivity activity = UnityEngine.ScriptableObject.CreateInstance<AvatarActivity>();
        activity.Init(this);//Just to make sure _curSession is 0 before we start injecting sessions into the activity

        if (returnToPrevActivity) {
            AvatarActivity runningActivity = GetRunningActivity();
            if (runningActivity) {

                Appliance returnToAppliance = runningActivity.GetCurrentTargetObject().GetComponent<Appliance>();

                if (returnToAppliance != null) { //Try to sit down after returning from switching the light
                    AvatarActivity.Session session = new AvatarActivity.Session();
                    session.title = "Change pose";
                    session.type = AvatarActivity.SessionType.ChangePoseAt;
                    session.parameter = "ChillOut";
                    session.appliance = returnToAppliance;
                    session.currentRoom = false;
                    activity.InsertSession(session);
                }

                if (returnToAppliance != null) {
                    AvatarActivity.Session walkToSession = new AvatarActivity.Session();
                    walkToSession.title = "Walking to appliance";
                    walkToSession.type = AvatarActivity.SessionType.WalkTo;
                    walkToSession.appliance = returnToAppliance;
                    walkToSession.currentRoom = false;
                    activity.InsertSession(walkToSession);
                }
            }
        }

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

    public void ChangeMood(AvatarMood.Mood mood, bool force) {
        AvatarMood component = gameObject.GetComponent<AvatarMood>();
        if(component != null) {
            if (force) {
                component.SetMood(mood);
            }
            else {
                component.TryChangeMood(mood);
            }
        }
    }

    // Save function
    public JSONClass Encode()
    {
        JSONClass json = new JSONClass();
        json.Add("name", name);
        Vector3 position = transform.position;
        json.Add("transform", JsonUtility.ToJson(position));
        json.Add("savedStandingPosition", JsonUtility.ToJson(_savedIdlePosition));
        //json.Add("scheduleIndex", _scheduleIndex.ToString());
        json.Add("_curActivity", _curActivity.Encode());
        json.Add("_nextActivity", _nextActivity.Encode());
        json.Add("_prevActivity", _prevActivity.Encode());
        json.Add("tempActivities", EncodeActivityStack());
        //Should be mooore here!

        //json.Add("object", JsonUtility.ToJson(this));

        return json;
    }

    // Load function
    public void Decode(JSONClass json)
    {
        //_scheduleIndex = json["scheduleIndex"].AsInt;
        //JsonUtility.FromJsonOverwrite(json["object"], this);
        transform.position = JsonUtility.FromJson<Vector3>(json["transform"]);
        _savedIdlePosition = JsonUtility.FromJson<Vector3>(json["savedStandingPosition"]);

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
