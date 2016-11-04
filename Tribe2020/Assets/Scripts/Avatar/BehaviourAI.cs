using UnityEngine;
using System.Collections.Generic;
using System;
using UnityStandardAssets.Characters.ThirdPerson;
using SimpleJSON; // For encoding and decoding avatar states

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(ThirdPersonCharacter))]
public class BehaviourAI : MonoBehaviour
{
    public enum AvatarState { Idle, Walking, Sitting, Waiting, Unscheduled, OverrideIdle, OverrideWalking, TurningOnLight };
    [SerializeField]
    private AvatarState _curAvatarState = AvatarState.Idle;

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

    private AvatarActivity tempActivity;
    private bool _isTemporarilyUnscheduled = false;

    private bool _isControlled = false;

    //private GameObject[] _appliances;
    private static Appliance[] _devices;
    private Room _curRoom;

    private float _startTime;
    //private bool _isSync = false;
    //private bool _isScheduleOver = false;

    //Definition of a schedule item
    [System.Serializable]
    public struct ScheduleItem
    {
        public string time;
        public AvatarActivity activity;
    }
    public ScheduleItem[] schedule;
    private int _scheduleIndex = 0;

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
    }

    // Update is called once per frame
    void Update()
    {
        if (_isTemporarilyUnscheduled)
        {
            //Handle override actions
            UpdateActivity(tempActivity);
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

        //Ok. Let's find the stamp for the next activity.
        if (_nextActivity.hasStartTime && _nextActivity.startTimePassed())
        {
            Debug.Log("Next activity's startTime (" + _nextActivity.startTime + ") passed. Finish current one and start the next one");
            _curActivity.FinishCurrentActivity();
            NextActivity();
            _curActivity.Start();
        }

        //same as above but backwards.
        //We are most likely not handling backwards time very well. But that's out of scope for now.
        if (_curActivity.hasStartTime && !_curActivity.startTimePassed())
        {
            Debug.Log("Current time is before current activity's startTime. Revert the activity and start the previous one");
            _curActivity.Revert();
            PreviousActivity();
            _curActivity.Start();
        }

        UpdateActivity();

    }

    private void UpdateActivity(AvatarActivity activity)
    {

        //do delta time stuffz
        activity.Step(this);

        //do activity stuff!
        switch (_curAvatarState)
        {
            //Not doing anything, do something feasible in the pilot
            case AvatarState.Idle:
                _charController.Move(Vector3.zero, false, false);
                //	activity.Init(this);
                //	Debug.Log(name + " began " + activity.name + " at " + time.Hour + ":" + time.Minute);
                break;
            // Walking towards an object, check if arrived to proceed
            //case AvatarState.OverrideWalking:
            //Debug.Log("override walking!");
            case AvatarState.Walking:
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
            case AvatarState.Sitting:
                //Be aware. This SitDown method is another than the one in this class.
                //_charController.SitDown(); //Sets a boolean in the animator object
                _charController.Move(Vector3.zero, false, false);
                break;
            // Waiting
            case AvatarState.Waiting:
                _charController.Move(Vector3.zero, false, false);
                break;
            //case AvatarState.OverrideIdle:
            //    _charController.Move(Vector3.zero, false, false);
            //    break;
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

    //If no activitty reference given, Update _curActivity.
    private void UpdateActivity()
    {
        UpdateActivity(_curActivity);
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

        Debug.Log("starting schedule sync");

        //loop through the schedule until we get to an actvity that should happen in the future
        for (; _scheduleIndex < schedule.Length; _scheduleIndex++)
        {
            //Handle schedule posts without time. Such activities should happen as soon as the activity before is finished.
            if (schedule[_scheduleIndex].time == null || schedule[_scheduleIndex].time == "")
            {
                //Ok. so this schedule post has no time specified. Let's loop further.
                continue;
            }
            string[] timeParse = schedule[_scheduleIndex].time.Split(':');
            int schedHour = int.Parse(timeParse[0]);
            int schedMinute = int.Parse(timeParse[1]);

            //Is this activity in the future. OR. Is it the last in the schedule?
            if (curHour <= schedHour || _scheduleIndex + 1 == schedule.Length)
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
                    startTime = _timeMgr.ScheduleToTS(curTime, 0, curItem.time);
                    SetCurrentActivity(curItem.activity, startTime);
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
                }
                else
                {
                    SetPrevActivity(prevItem.activity);
                }




                Debug.Log("startActivity set for " + name + ". It's " + curItem.activity + " with startTimeStamp " + startTime);
                return;
            }
        }
    }

    //
    public void SetCurrentActivity(AvatarActivity activity, double startTime)
    {
        Debug.Log("setting current activity: " + activity);
        _curActivity = UnityEngine.Object.Instantiate(activity) as AvatarActivity;
        _curActivity.Init(this, startTime);
    }

    public void SetCurrentActivity(AvatarActivity activity)
    {
        Debug.Log("setting current activity: " + activity);
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
    public void StartActivity(AvatarActivity activity)
    {
        //Debug.Log(name + ".StartActivity(" + activity + ") with end time " + _timeMgr.time + " + " + (60 * 3));
        _curAvatarState = AvatarState.Idle;
        //activity.endTime = _timeMgr.time + 60 * 3;
        _curActivity = activity;
        _curActivity.Init(this);
    }

    public void StartTemporaryActivity(AvatarActivity activity)
    {
        Debug.Log(name + ".StartTemporaryActivity(" + activity + ")");
        tempActivity = activity;
        tempActivity.Init(this);
        _isTemporarilyUnscheduled = true;
        tempActivity.Start();
    }

    //Alright. Let's pick the next activity in the schedule. This function updates the references of _prev, _cur and _next -activity.
    public void NextActivity()
    {
        //We will use the current time as reference timestamp for picking correct day when converting schedule timestring to epoch timestamp.
        double curTime = _timeMgr.GetTotalSeconds();

        Debug.Log("Next activity called. " + _nextActivity.name);
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

    //
    public void Stop()
    {
        _curAvatarState = AvatarState.Idle;
        _charController.Move(Vector3.zero, false, false);
    }

    public void Wait()
    {
        _curAvatarState = AvatarState.Idle;
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

        _agent.SetDestination(target);
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
    public void WalkTo(AvatarActivity.Target target, bool isOwned)
    {
        _curTargetObj = FindNearestDevice(target, isOwned);
        if (_curTargetObj == null)
        {
            Debug.LogError("Didn't find a WalkTo target " + target + ". doing activity " + _curActivity.name);
            return;
        }

        _agent.SetDestination(_curTargetObj.GetComponent<Appliance>().interactionPos);
        _curAvatarState = AvatarState.Walking;
    }

    //Let's use the reference to the appliance
    public void WalkTo(Appliance appliance, bool isOwned)
    {

        //GameObject _curTargetObj = appliance.gameObject;
        //if (_curTargetObj == null)
        //{
        //    Debug.LogError("Didn't find a WalkTo appliance, doing activity " + _curActivity.name);
        //    return;
        //}

        if(appliance == null)
        {
            Debug.LogError("Daaa fuuuck! no appliance i just got from you!");
        }
        
        _agent.SetDestination(appliance.interactionPos);
        _curAvatarState = AvatarState.Walking;
    }

    //
    public void TeleportTo(AvatarActivity.Target target, bool isOwned)
    {
        _curTargetObj = FindNearestDevice(target, isOwned);
        if (_curTargetObj == null) { return; }

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
    public void WarpToDestination()
    {
        _agent.Warp(_agent.destination);
    }

    //
    public void SitDown()
    {
        _agent.enabled = false;
        GetComponent<Rigidbody>().isKinematic = true;
        //Vector3 coord;
        //Let's search for a gameObject called Sit Position
        Transform sitPosition = _curTargetObj.transform.Find("Sit Position");
        if (sitPosition == null)
        {
            Debug.LogError("Didn't find a gameobject called Sit Position inside " + _curTargetObj.name);
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
        _curAvatarState = AvatarState.Sitting;
        _charController.SitDown(); //Sets a boolean in the animator object
    }

    public void standUp()
    {
        //First position the avatar at the interaction point. Then turn navmeshagent back on.
        Debug.Log("Standing up. _curTargetObj is " + _curTargetObj, _curTargetObj);
        transform.position = _curTargetObj.GetComponent<Appliance>().interactionPos;
        _agent.enabled = true;
        GetComponent<Rigidbody>().isKinematic = false;
        _charController.StandUp();

        //We don't set the _curAvatarState here, since standing up is performed before doing other stuffz

    }

    //
    public void Delay(float seconds)
    {
        //Debug.Log(name + ".Delay(" + seconds + ")");
        _curAvatarState = AvatarState.Waiting;
    }

    //
    public void SetRunLevel(AvatarActivity.Target target, string parameter)
    {
        GameObject device = FindNearestDevice(target, false);
        if (device == null)
        {
            Debug.LogError("Didn't find device for setting runlevel: " + target);
            return;
        }

        ElectricMeter meter = device.GetComponent<ElectricMeter>();
        if (meter == null)
        {
            Debug.LogError("Didn't find electric meter for setting runlevel", device);
            return;
        }

        device.GetComponent<ElectricMeter>().On();
        //.SetRunlevel(int.Parse(parameter));
    }

    public void SetRunLevel(Appliance appliance, string parameter)
    {
        GameObject device = appliance.gameObject;
        if (device == null)
        {
            Debug.LogError("Didn't find device for setting runlevel");
            return;
        }

        ElectricMeter meter = device.GetComponent<ElectricMeter>();
        if (meter == null)
        {
            Debug.LogError("Didn't find electric meter for setting runlevel");
            return;
        }

        device.GetComponent<ElectricMeter>().On();
        //.SetRunlevel(int.Parse(parameter));
    }

    // Searches devices for device with nearest Euclidean distance which fullfill affordance and ownership
    public GameObject FindNearestDevice(AvatarActivity.Target affordance, bool isOwned)
    {
        GameObject target = null;
        float minDist = float.MaxValue;

        foreach (Appliance device in _devices)
        {
            List<AvatarActivity.Target> affordances = device.avatarAffordances;
            if (affordances.Contains(affordance) && (!isOwned || device.owners.Contains(_stats.avatarName)))
            {
                float dist = Vector3.Distance(transform.position, device.transform.position);
                if (dist < minDist)
                {
                    minDist = dist;
                    target = device.gameObject;
                }
            }
        }

        if (target == null)
        {
            Debug.Log(name + " could not find the affordance " + affordance.ToString());
        }

        return target;
    }

    //
    public void OnActivityOver()
    {
        Debug.Log(name + "'s activity " + _curActivity.name + " is over");
        if (_curAvatarState == AvatarState.Sitting)
        {
            Debug.Log("avatar was sitting. Standing up.");
            standUp();
        }
        _curAvatarState = AvatarState.Idle;

        if (_isTemporarilyUnscheduled)
        {
            //Notify the gamecontroller that we finished this activity!
            _controller.OnAvatarActivityComplete(tempActivity.name);

            //Ok. so this overriding activity was finished. Return to schedule.
            _isTemporarilyUnscheduled = false;

            Debug.Log("Teemporary activity finished!", tempActivity);

            //We don't want to do moar stuffz in here. Bail out!
            return;
        }

        //Notify the gamecontroller that we finished this activity!
        _controller.OnAvatarActivityComplete(_curActivity.name);

        //Ok. If the next activity doesn't have a startTime we can go ahead and launch it immediately
        if (!_nextActivity.hasStartTime)
        {
            Debug.Log("Next activity have no startTime specified so let's start it immediately");
            NextActivity();
            _curActivity.Start();
        }
    }

    //This will get called when avatar (that have rigidbody and collider) collides with a collider. <--- wow, so many colliding words in one sentence!!!
    void OnTriggerEnter(Collider other)
    {
        //Did the avatar collide with a roooom?
        if (other.GetComponent<Room>())
        {
            _curRoom = other.GetComponent<Room>();
            CheckLighting();
        }

        //Debug.Log("Avatar.OnTriggerEnter: " + other.name);
    }

    //
    public void CheckLighting()
    {
        if (_curRoom)
        {
            if (_curRoom.lux < 1)
            {
                //Debug.Log(name + " thinks it's to dark in the " + _curRoom.name);
                Appliance lightSwitch = _curRoom.GetLightSwitch();
                if (lightSwitch)
                {
                    //Debug.Log(transform.parent.name +  " found a light switch");
                    TurnOnLight(lightSwitch);
                    return;
                }
                Debug.LogError(name + " couldn't find a lightswitch in this room!", _curRoom);
            }
            else
            {
                //Debug.Log(name + " is ok with light in " + _curRoom.name);
            }
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
    public void TurnOnLight(Appliance lightSwitch)
    {
        Debug.Log("Yo! Gonna flip that lamp switch!");
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

        AvatarActivity.Session turnOnLight = new AvatarActivity.Session();
        turnOnLight.title = "Turning on light";
        turnOnLight.type = AvatarActivity.SessionType.SetRunlevel;
        turnOnLight.target = AvatarActivity.Target.LampSwitch; //Is this needed if we instead set appliance below? I didn't remove this, if it would mess something else up. /Martin
        turnOnLight.appliance = lightSwitch;
        turnOnLight.parameter = "1";

        roomLightActivity.InsertSession(walkToLightSwitch);
        roomLightActivity.InsertSession(turnOnLight);

        //Alright. We've built a super nice activity for turning on the light. Ledz ztart itt!
        StartTemporaryActivity(roomLightActivity);
    }

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
        json.Add("transform", transform.ToString());
        json.Add("scheduleIndex", _scheduleIndex.ToString());
        //Should be mooore here!

        return json;
    }

    // Load function
    public void Decode(JSONClass json)
    {
        _scheduleIndex = json["scheduleIndex"].AsInt;

        //Should be more here!
    }
}
