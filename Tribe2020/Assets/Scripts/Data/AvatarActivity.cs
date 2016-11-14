using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using SimpleJSON;
using System;

[CreateAssetMenu(fileName = "Behaviour", menuName = "Avatar/Behaviour", order = 1), Serializable]
public class AvatarActivity : ScriptableObject {
	private GameTime _timeMgr;
    
	public string title;
	public List<Session> sessions;
	//public Session SkipSession;

	//public string onSkipCommand;
	private int _currSession;
	private BehaviourAI _ai;

    GameObject _curTargetObj; //Should this be part of activity? Previously in BehaviourAI
    BehaviourAI.AvatarState _curAvatarState = BehaviourAI.AvatarState.Idle;

    protected float _weight = 0;
	protected string _curState = "";

	private float _delay = 0;
	//public float _duration = 0;

	public double startTime = 0;
    [HideInInspector]
    public bool hasStartTime = true;
    //public double endTime = 0;

    //public enum ActivityState { Idle, Walking, Sitting, Waiting, Unscheduled, OverrideIdle, OverrideWalking, TurningOnLight };
    //[SerializeField]
    //public ActivityState curActivityState = ActivityState.Idle;

    //Activity session types
    public enum SessionType { WalkTo, SitDown, WaitForDuration, WaitUntilEnd, SetRunlevel, Interact, Warp, TurnOn, TurnOff };
	//Energy efficieny check types
	public enum EfficiencyType { None, Ligthing, Heating, Cooling, Device };
	//Energy efficieny check types
	public enum CheckType { LessThan, GreaterThan };
	//
	//public enum Target { None, OfficeDesk, HelpDesk, Monitor, SocialSpace, LunchSpace, DishWasher, Coffee, Fridge, Toilet,
	//	Sink, Dryer, Presentation, LampSwitch, Lamp, ThrowTrash, Microwave, Home, Work };

	//Definition of a quest step
	[System.Serializable]
	public class Session {
		public string title;
		public float probability = 1f;
		public SessionType type;
        public Appliance appliance = null;
		//public Target target;
		public Affordance requiredAffordance;
		public string parameter;
		public bool avatarOwnsTarget;
		public bool currentRoom;
		public EfficiencyType relatedEfficieny;
		public CheckType checkType;
		public float efficienyLevel;
	}

	//public virtual void Init(BehaviourAI ai) {

 //       _ai = ai;
	//	_currSession = 0;
	//	//ExecuteCommand(ai, sessions[_currSession]);
	//}


	public virtual void Step(BehaviourAI ai) {
        //Don't try any stuff if where not in a session.
        if (IsThisActivityFinished())
        {
            return;
        }
        if(GetSessionAtIndex(_currSession).type == SessionType.WaitForDuration) {
            if(_delay > 0f) {
                _delay -= Time.deltaTime;
            } else {
                NextSession();
            }
        }
	}

    //Initialize without startTime
    public void Init(BehaviourAI ai)
    {
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
        if (hasStartTime)
        {
            string startTimeView = _timeMgr.TimestampToDateTime(startTime).ToString("HH:mm");
            DebugManager.Log(_ai.name + " starting activity " + name + " start " + startTimeView + ", currTime is " + curTimeView, this);
        }
        else
        {
            DebugManager.Log(_ai.name + " starting activity " + name + " without startTime, curTime is " + curTimeView, this);
        }


        StartSession(GetSessionAtIndex(_currSession));
	}

	//
	public void StartSession(Session session) {
        DebugManager.Log(_ai.name + " started session " + session.title + " of type " + session.type + ". Part of activity " + this.name, this);
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
            case SessionType.SitDown:
                _ai.SitAt(session.requiredAffordance, session.avatarOwnsTarget);
                break;
			case SessionType.WaitUntilEnd:
				_ai.Stop();
				break;
			case SessionType.WalkTo:
                if(session.appliance != null)
                {
                    _ai.WalkTo(session.appliance, session.avatarOwnsTarget);
                }else
                {
                    _ai.WalkTo(session.requiredAffordance, session.avatarOwnsTarget);
                }
				break;
			case SessionType.SetRunlevel:
                if(session.appliance != null)
                {
                    _ai.SetRunLevel(session.appliance, int.Parse(session.parameter));
                }
                else
                {
                    _ai.SetRunLevel(session.requiredAffordance, int.Parse(session.parameter));
                }
				
				NextSession();
				break;
            case SessionType.TurnOn:
                if (session.appliance != null)
                {
                    _ai.TurnOn(session.appliance);
                }
                else
                {
                    _ai.TurnOn(session.requiredAffordance);
                }

                NextSession();
                break;
            case SessionType.TurnOff:
                if (session.appliance != null)
                {
                    _ai.TurnOff(session.appliance);
                }
                else
                {
                    _ai.TurnOff(session.requiredAffordance);
                }

                NextSession();
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
	public void InsertSession(Session session) {
        if (session == null)
        {
            DebugManager.LogError("Session is null!", this);
        }
		sessions.Insert(_currSession, session);

        //Eeeeeh. Whaaat? We don't want to start this here right? /Gunnar
		//StartSession(session);
	}

	//
	public void NextSession() {
        DebugManager.Log("incrementing _currSssion. Index: " + (_currSession + 1) + ", max: " + (sessions.Count - 1), this);
        _currSession++;

        if (_currSession >= sessions.Count) {
            DebugManager.Log("_currSession out of bound. No more sessions in this activity. calling activityOver callback", this);
            _ai.OnActivityOver();
		} else {
            //If we were sitting down AND we are gonna be moving somewhere, we should do that standing up!
            if(_curAvatarState == BehaviourAI.AvatarState.Sitting && GetSessionAtIndex(_currSession).type == SessionType.WalkTo)
            {
                _ai.standUp();
            }
            //Debug.Log("starting session" + sessions[_currSession].title);
            StartSession(GetSessionAtIndex(_currSession));
		}
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
        DebugManager.Log(_ai.name + " reached destination " + GetSessionAtIndex(_currSession).requiredAffordance + ". if current SessionType is walkTo, start next session", this);
        //if (sessions[_currSession].type == SessionType.WalkTo) {//Why do we perform this check? I don't know. Gunnar.
			NextSession();
		//}
	}

	//
	public void FinishCurrentActivity() {
        DebugManager.Log("Gonna finish this activity by simulating the remaining sessions.", this, this);

        //      //Remove the sessions already performed
        //      if (_currSession > 0) {
        //	sessions.RemoveRange(0, _currSession - 1);
        //}

        //Simulate the sessions not yet performed
        //First increment _currsession by one. We don't want to run the current session again.
        _currSession++;
        while(!IsThisActivityFinished()) {
			SimulateSession(GetSessionAtIndex(_currSession));
            _currSession++;
		}

        //Trigger callback for finished activity
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
            return startTime < _timeMgr.GetTotalSeconds();
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

    public BehaviourAI.AvatarState GetCurrentAvatarState()
    {
        return _curAvatarState;
    }

    public void SetCurrentAvatarState(BehaviourAI.AvatarState avatarState)
    {
        _curAvatarState = avatarState;
    }

    //public JSONClass Encode()
    //{
    //    //MemoryStream stream1 = new MemoryStream();
    //    //DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(AvatarActivity));
    //    //ser.WriteObject(stream1, this);

    //    //SimpleJSON.JSONClass json;
    //    //json.Serialize()
    //    //foreach(Session session in sessions)
    //    //{
    //    //    session.
    //    //}
    //}

}

