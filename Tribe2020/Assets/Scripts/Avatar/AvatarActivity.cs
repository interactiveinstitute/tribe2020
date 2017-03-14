using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using SimpleJSON;
using System;

[CreateAssetMenu(fileName = "Behaviour", menuName = "Avatar/Behaviour", order = 1), Serializable]
public class AvatarActivity : ScriptableObject {
	private GameTime _timeMgr;
    
	public string title;
    //[HideInInspector] Apparently can't do that here, since it'll also affect the custom editor. Instead we're manually excluding this in the editor script (AvatarActivityEditor).
	public List<Session> sessions;

    [SerializeField]
	private int _currSession;
	private BehaviourAI _ai;
    [SerializeField]
    private GameObject _curTargetObj;
    private Appliance _activityAppliance;
    private RetrievedAffordance affordanceInUse;
    [Serializable]
    public enum AvatarState { Idle, Walking, Posing };
    [SerializeField]
    AvatarState _curAvatarState = AvatarState.Idle;
    [SerializeField]
    public List<ElectricDevice> turnedOnDevices;

    [SerializeField]
	private float _delay = 0;
	//public float _duration = 0;

	public double startTime = 0;
    [SerializeField]
    public bool hasStartTime = true;
    //public double endTime = 0;

    public bool isTemporary = false;

    //Activity session types
    public enum SessionType { WalkTo, SitDown, WaitForDuration, WaitUntilEnd, SetRunlevel, Interact, Warp, TurnOn, TurnOff, ChangePose, ChangePoseAt, InteractWithAvatar, WaitForAffordanceAvailable, ReleaseTakenAffordance, IncreaseNrOfAffordanceSlots, DecreaseNrOfAffordanceSlots, SetActivityAppliance, ClearActivityAppliance };
	//Energy efficieny check types
	public enum CheckType { LessThan, GreaterThan };
	//
	//public enum Target { None, OfficeDesk, HelpDesk, Monitor, SocialSpace, LunchSpace, DishWasher, Coffee, Fridge, Toilet,
	//	Sink, Dryer, Presentation, LampSwitch, Lamp, ThrowTrash, Microwave, Home, Work };

	//Definition of a quest step
	[System.Serializable]
	public class Session {
		public string title;
		//public float probability = 1f;
		public SessionType type;
        public Appliance appliance = null;
		//public Target target;
		public Affordance requiredAffordance;
        public string parameter;
		public bool avatarOwnsTarget;
		public bool currentRoom;
        public bool setActivityAppliance;
        public bool useActivityAppliance = true;

        public bool testEnergyEffeciency;

        public enum SetMoodOptions { No, Try, Force }
        public SetMoodOptions setAvatarMood;
        public AvatarMood.Mood mood;
		//public EfficiencyType relatedEfficieny;
		//public CheckType checkType;
		//public float efficienyLevel;
	}

    public class RetrievedAffordance
    {
        public Appliance owningAppliance;
        public Affordance affordance;
    }

	//public virtual void Init(BehaviourAI ai) {

 //       _ai = ai;
	//	_currSession = 0;
	//	//ExecuteCommand(ai, sessions[_currSession]);
	//}

    //Nomally things that should be continuosly updated (versus set/initialised with startsession) should be handled in behaviourAI. The wait related updates are an exception to this.
	public virtual void Step(BehaviourAI ai) {
        //Don't try any stuff if where not in a session.
        if (IsThisActivityFinished())
        {
            return;
        }

        //decrement timer
        if (_delay >= 0f)
        {
            //TODO: Make this update from simulation time. deltaTime is related to frames.
            _delay -= _timeMgr.simulationDeltaTime;
        }

        //Do stuff at certain conditions
        Session session = GetCurrentSession();
        switch (session.type) {
            case SessionType.WaitForAffordanceAvailable:
                if (_delay < 0f)
                {
                    DebugManager.Log("Gave up waiting for appliance with affordance", this, this);
                    JumpToAffordanceRelease();
                }else {
                    Appliance device = _ai.GetApplianceWithAffordance(session.requiredAffordance, session.avatarOwnsTarget, session.avatarOwnsTarget);
                    if (device)
                    {
                        if (TryToTakeAffordanceSlot(session.requiredAffordance, device))
                        {
                            if (session.setActivityAppliance)
                            {
                                _activityAppliance = session.appliance;
                            }
                            DebugManager.Log("Found an available appliance with affordance " + session.requiredAffordance, this, this);
                            //AvatarActivity.Session walkToAvailableAffordance = new Session();
                            //walkToAvailableAffordance.type = SessionType.WalkTo;
                            NextSession();
                        }
                    }
                }
                break;
            case SessionType.WaitForDuration:
                if (_delay < 0f)
                {
                    NextSession();
                }
                break;
            default:
                break;
        }
	}

    //Initialize without startTime
    public void Init(BehaviourAI ai)
    {
        AvatarActivity runningActivity = ai.GetRunningActivity();
        if (runningActivity) {
            SetCurrentAvatarState(runningActivity.GetCurrentAvatarState());
            SetCurrentTargetObject(runningActivity.GetCurrentTargetObject());
        }

        _timeMgr = GameTime.GetInstance();
        _ai = ai;
        hasStartTime = false;
        _currSession = 0;
        //If this activity for some reason doesn't have a list of sessions, create an empty one.
        if (sessions == null)
        {
            sessions = new List<Session>();
        }
    }

    //Initialize
    public void Init(BehaviourAI ai, double startTime) {
		_timeMgr = GameTime.GetInstance();
		_ai = ai;
		this.startTime = startTime;
        hasStartTime = true;
		_currSession = 0;
        //If this activity for some reason doesn't have a list of sessions, create an empty one.
        if (sessions == null)
        {
            sessions = new List<Session>();
        }
    }

    //
    public void Start() {
        string curTimeView = _timeMgr.GetDateTime().ToString("HH:mm");
        double curTimeStamp = _timeMgr.GetTotalSeconds();
        if (hasStartTime)
        {
            string startTimeView = _timeMgr.TimestampToDateTime(startTime).ToString("HH:mm");
            DebugManager.Log(_ai.name + " starting activity " + name + " startTime " + startTime + ", currTime is " + curTimeStamp, this);
        }
        else
        {
            DebugManager.Log(_ai.name + " starting activity " + name + " without startTime, curTime is " + curTimeView, this);
        }

        StartSession(GetCurrentSession());
	}

	//
	public void StartSession(Session session) {
        DebugManager.Log(_ai.name + " started session " + session.title + " of type " + session.type + ". Part of activity " + this.name, this);

        //Skip session if energy effeciency test fails
        if(session.testEnergyEffeciency && !_ai.GetComponent<AvatarStats>().TestEnergyEfficiency()) {
            NextSession();
            return;
        }

        //Set mood
        switch (session.setAvatarMood) {
            case Session.SetMoodOptions.Try:
                _ai.ChangeMood(session.mood, false);
            break;
            case Session.SetMoodOptions.Force:
                _ai.ChangeMood(session.mood, true);
            break;
        }
        
        //Setup appliance reference
        if (session.useActivityAppliance && _activityAppliance && !session.setActivityAppliance)
        {
            session.appliance = _activityAppliance;
        }
        if (!session.appliance && session.requiredAffordance)
        {
            session.appliance = _ai.GetApplianceWithAffordance(session.requiredAffordance, session.avatarOwnsTarget, session.currentRoom);
        }
        if(session.appliance && session.setActivityAppliance)
        {
            _activityAppliance = session.appliance;
        }
        
        //If we were posing AND we are gonna be moving somewhere, we should do that from an idle pose!
        if (_curAvatarState == AvatarState.Posing && GetSessionAtIndex(_currSession).type == SessionType.WalkTo) {
            _ai.ReturnToIdlePose();
        }

        switch (session.type) {
			case SessionType.WaitForDuration:
                if (session.parameter == "")
                {
                    DebugManager.LogError("No duration set for parameter in session WaitForDuration in activity " + name + "avatar " + _ai.name, this);
                    _delay = 20; //Didn't get a wait parameter. Setting a default.
                }
                else
                {
                    _delay = int.Parse(session.parameter);
                }
                _ai.Wait();
				break;
            //case SessionType.SitDown:
            //    _ai.SitAt(session.requiredAffordance, session.avatarOwnsTarget);
            //    break;
			case SessionType.WaitUntilEnd:
				_ai.Wait();
				break;
			case SessionType.WalkTo:
                _ai.WalkTo(session.appliance, session.avatarOwnsTarget);
            break;
			case SessionType.SetRunlevel:
                _ai.SetRunLevel(session.appliance, int.Parse(session.parameter));
				NextSession();
				break;
            case SessionType.TurnOn:
                _ai.TurnOn(session.appliance);
                NextSession();
                break;
            case SessionType.TurnOff:
                _ai.TurnOff(session.appliance);
                NextSession();
                break;
            case SessionType.SitDown:
				_ai.PoseAtCurrentTarget("Sit");
                NextSession();
                break;
            case SessionType.ChangePose:
                if(session.parameter == null || session.parameter == "")
                {
                    DebugManager.LogError("No parameter supplied for setting pose", this, this);
                }
                _ai.changePoseTo(session.parameter);
                NextSession();
                break;
            case SessionType.ChangePoseAt:
                if (session.parameter == null || session.parameter == "")
                {
                    DebugManager.LogError("No parameter supplied for setting pose", this, this);
                }

                _ai.ChangePoseAt(session.parameter, session.appliance);
                NextSession();
                break;
            case SessionType.InteractWithAvatar:
                _ai.InteractWithAvatar(session.requiredAffordance);
                NextSession();
                break;
            case SessionType.WaitForAffordanceAvailable:
                if (session.parameter == "")
                {
                    DebugManager.LogError("No duration set for parameter in session WaitForAffordance in activity " + name + "avatar " + _ai.name, this, this);
                    _delay = UnityEngine.Random.Range(40, 900); //Didn't get a wait parameter. Setting a random duration between 40 sec and 15 min.
                }
                else
                {
                    _delay = int.Parse(session.parameter);
                }
                //_ai.Wait(); //Keeeeeep stilll!
                break;
            case SessionType.ReleaseTakenAffordance:
                if (!ReleaseTakenAffordanceSlot())
                {
                    DebugManager.LogError("Was not able to release affordance", this, this);
                }
                NextSession();
                break;
            case SessionType.IncreaseNrOfAffordanceSlots:
                if (session.appliance || session.requiredAffordance)
                {
                    int count = 1;
                    if (session.parameter != "")
                        count = int.Parse(session.parameter);
                    if (!session.appliance.IncreaseNrOfAffordanceSlots(session.requiredAffordance, count))
                        DebugManager.LogError("Couldn't increase affordance slot " + session.requiredAffordance + " of " + session.appliance, this, this);
                }
                NextSession();
                break;
            case SessionType.DecreaseNrOfAffordanceSlots:
                if (session.appliance || session.requiredAffordance)
                {
                    int count = 1;
                    if (session.parameter != "")
                        count = int.Parse(session.parameter);
                    if (!session.appliance.DecreaseNrOfAffordanceSlots(session.requiredAffordance, count))
                        DebugManager.LogError("Something went wrong when decreasing nrofaffordance slots " + session.requiredAffordance + " of " + session.appliance, this, this);
                }
                NextSession();
                break;
            case SessionType.SetActivityAppliance:
                if (session.appliance)
                {
                    _activityAppliance = session.appliance;
                }
                else
                {
                    DebugManager.LogError("Couldn't set activityAppliance for some reason!", this, this);
                }
                NextSession();
                break;
            case SessionType.ClearActivityAppliance:
                _activityAppliance = null;
                break;
            default:
                DebugManager.Log("unknown SessionType", this);
                break;
		}
	}

    //
    public void SimulateSession(Session session)
    {
        DebugManager.Log("Simulating session of type " + session.type + " with affordance " + session.requiredAffordance, this, this);

        //Make case statements for the session types that differ from normal tempo. If no difference, this function defaults to run the sessions normally (by calling StartSession in the default case)
        switch (session.type)
        {
            case SessionType.WaitForDuration:
                break;
            case SessionType.WaitUntilEnd:
                break;
            case SessionType.WalkTo:
                //Debug.Log("Simulating WalkTo. Teleporting to " + session.target);
                _ai.WarpTo(session.requiredAffordance, session.avatarOwnsTarget);
                break;
            default:
                StartSession(session);
                break;
        }
    }

    public Session GetCurrentSession()
    {
        return sessions.Count > 0 ? sessions[_currSession] : null;
    }

    private Session GetSessionAtIndex(int index)
    {
        if(index < sessions.Count)
        {
            return sessions[index];
        }
        DebugManager.LogError("Session index out of bound! received index was " + index + ". Falling back to return first session instead", this, this);
        if(sessions.Count > 0)
        {
            return sessions[0];
        }
        DebugManager.LogError("Session list is empty!!!! Something is terribly wroooong!", this, this);
        return new Session();
    }

	//
	public void InsertSessionAtCurrentIndex(Session session) {
        if (session == null)
        {
            DebugManager.LogError("Session is null!", this);
        }
		sessions.Insert(_currSession, session);
	}

    public void InsertSessionAtEnd(Session session)
    {
        if (session == null)
        {
            DebugManager.LogError("Session is null!", this);
        }
        sessions.Add(session);
    }

    //
    public void NextSession() {
        DebugManager.Log("incrementing _currSssion. Index: " + (_currSession + 1) + ", max: " + (sessions.Count - 1), this);
        _currSession++;

        if (_currSession >= sessions.Count) {
            DebugManager.Log("_currSession out of bound. No more sessions in this activity. calling activityOver callback", this);
            _ai.OnActivityOver();
		} else {
            //If we are gonna be moving somewhere, we should do that from an idle pose!
            if(GetCurrentSession().type == SessionType.WalkTo)
            {
                _ai.ReturnToIdlePose();
            }
            //Debug.Log("starting session" + sessions[_currSession].title);
            StartSession(GetCurrentSession());
		}
	}

    // Was not sure what to name this function. The important point is that it won't run the remaining sessions but still mark the activity as finished.
    public void JumpToAffordanceRelease()
    {
        //AvatarActivity.Session waitSession = new AvatarActivity.Session();
        //waitSession.title = "wait until end";
        //waitSession.type = AvatarActivity.SessionType.WaitUntilEnd;
        //waitSession.parameter = "";
        //InsertSessionAtEnd(waitSession);
        //_currSession = sessions.Count - 1;
        //StartSession(GetCurrentSession());
        for(int i = _currSession; i < sessions.Count; i++)
        {
            if(sessions[i].type == SessionType.ReleaseTakenAffordance)
            {
                _currSession = i;
                break;
            }
        }


        //Ok. we are either at the end. Or. We are at the session that releases the affordance. Either way, jump to the session after (if at end this will trigger activityfinished)
        NextSession();
    }

    //This function gets called by the appliance that holds the affordance slot. When calling this function the appliance should already have removed this activity from the list of "subscribing" activities.
    public bool LoseAffordanceSlot()
    {
        if(affordanceInUse != null)
        {
            affordanceInUse = null;
            JumpToAffordanceRelease();
            return true;
        }
        return false;
    }

    private bool ReleaseTakenAffordanceSlot()
    {
        if(affordanceInUse != null) {
                affordanceInUse.owningAppliance.ReleaseAffordanceSlot(affordanceInUse.affordance, this);
                return true;
        }
        return false;
    }

    private bool TryToTakeAffordanceSlot(Affordance affordance, Appliance device)
    {
        if (device.TakeAffordanceSlot(affordance, this))
        {
            affordanceInUse = new RetrievedAffordance();
            affordanceInUse.affordance = affordance;
            affordanceInUse.owningAppliance = device;
            return true;
        }
        return false;
    }

    public void Simulate()
    {
        //DebugManager.Log("Simulating activity " + name, this, this);
        //foreach(Session session in sessions)
        //{
        //    SimulateSession(session);
        //}

    }

	//
	public virtual void Update() {
		//_duration += Time.deltaTime;
		//if(_duration >= _endTime) {
		//	Simulate();
		//}

		//if(sessions[_currSession].type == SessionType.WaitForDuration) {
		//	_delay -= Time.deltaTime;
		//	if(_delay <= 0) {
		//		NextSession();
		//	}
		//}
	}

	//
	public void OnDestinationReached() {
        DebugManager.Log(_ai.name + " reached destination " + GetCurrentSession().requiredAffordance + ". Start next session", this);
        //if (sessions[_currSession].type == SessionType.WalkTo)//We do this check. In case the character
        //{
            NextSession();
        //}
        //SetCurrentAvatarState(AvatarState.Idle);
    }

	//
	public void SimulateToEnd() {
        DebugManager.Log("Gonna finish this activity by simulating the remaining sessions.", this, this);

        //      //Remove the sessions already performed
        //      if (_currSession > 0) {
        //	sessions.RemoveRange(0, _currSession - 1);
        //}

        //Simulate the sessions not yet performed
        //First increment _currsession by one. We don't want to run the current session again.
        _currSession++;
        while(!IsThisActivityFinished()) {
			SimulateSession(GetCurrentSession());
            _currSession++;
		}

        //Trigger callback for finished activity
        //Be aware that this callback will also move to and start next activity in schedule if it has no startime.
		_ai.OnActivityOver();
	}

	//
	public void Revert() {
	}

	public virtual void OnHasReached(BehaviourAI ai, string tag) {
	}

	public virtual bool IsDone() {
		return true;
	}


    //Checks if current time is more than startTime of this activity
    public bool startTimePassed()
    {
        if (hasStartTime)
        {
            return startTime <= _timeMgr.GetTotalSeconds();
        }
        return false;
    }

	//
	//public void ExecuteCommand(BehaviourAI ai, string command) {
	//	string[] cmdParse = command.Split(',');
	//	string cmdFunction = cmdParse[0];
	//	string[] cmdArgs = new string[cmdParse.Length - 1];
	//	for(int i = 0; i < cmdParse.Length - 1; i++) {
	//		cmdArgs[i] = cmdParse[i + 1];
	//	}

	//	//Debug.Log(ai.gameObject.GetComponent<AvatarStats>().avatarName + ", command: " + cmdFunction + ", " + cmdArgs[0]);
	//	if(cmdFunction == "Delay") {
	//		ai.gameObject.SendMessage(cmdFunction, float.Parse(cmdArgs[0]));
	//	} else {
	//		ai.gameObject.SendMessage(cmdFunction, cmdArgs);
	//	}
	//}

	//
	//public void SimulateExecution(BehaviourAI ai) {


	//	//Debug.Log("SimulateExecution:" + onSkipCommand);

	//	//ExecuteCommand(ai, onSkipCommand);
	//}

	//
	//public void ResumeSession(BehaviourAI ai) {
	//	_currSession = 0;
	//	StartSession(sessions[_currSession]);
	//	//ExecuteCommand(ai, sessions[_currSession]);
	//}

	////
	//public void NextStep(BehaviourAI ai) {
	//	//Debug.Log(ai.name + ".CurrentStep" + _currSession + ", " + sessions.Count + ", " + sessions[_currSession]);
	//	_currSession++;

	//	//if(_currSession == sessions.Count) {
	//	//	OnActivityDone(ai);
	//	//} else {
	//	//	//Debug.Log(ai.name + ".NextStep" + _currSession + ", " + sessions.Count + ", " + sessions[_currSession]);
	//	//	//Debug.Log("NextStep" + _currSession + ", " + sessions.Count + ", " + sessions[_currSession]);
	//	//	ExecuteCommand(ai, sessions[_currSession]);
	//	//}
	//}

	////
	//public void OnActivityDone(BehaviourAI ai) {
	//	//Debug.Log("OnBehaviourDone");
	//	ai.OnActivityOver();
	//}

    private bool IsThisActivityFinished()
    {
        //If we have got past the last session, the activity must be finished. Right?
        return _currSession >= sessions.Count;
    }

    public GameObject GetCurrentTargetObject()
    {
        return _curTargetObj;
    }

    public void SetCurrentTargetObject(GameObject targetObject)
    {
        _curTargetObj = targetObject;
    }

    public AvatarState GetCurrentAvatarState()
    {
        return _curAvatarState;
    }

    public void SetCurrentAvatarState(AvatarState avatarState)
    {
        _curAvatarState = avatarState;
    }

    public string Encode()
    {
        return JsonUtility.ToJson(this);
        //MemoryStream stream1 = new MemoryStream();
        //DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(AvatarActivity));
        //ser.WriteObject(stream1, this);

        //SimpleJSON.JSONClass json;
        //json.Serialize()
        //foreach(Session session in sessions)
        //{
        //    session.
        //}
    }

    public void Decode(string json)
    {
        JsonUtility.FromJsonOverwrite(json, this);
    }

}

