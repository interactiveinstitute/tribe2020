﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "Behaviour", menuName = "Avatar/Behaviour", order = 1)]
public class AvatarActivity : ScriptableObject {
	private GameTime _timeMgr;

	public string title;
	public List<Session> sessions;
	//public Session SkipSession;

	//public string onSkipCommand;
	private int _currSession;
	private BehaviourAI _ai;

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
    public enum SessionType { WalkTo, SitUntilEnd, WaitForDuration, WaitUntilEnd, SetRunlevel, Interact, Warp, TurnOn, TurnOff };
	//Energy efficieny check types
	public enum EfficiencyType { None, Ligthing, Heating, Cooling, Device };
	//Energy efficieny check types
	public enum CheckType { LessThan, GreaterThan };
	//
	public enum Target { None, OfficeDesk, HelpDesk, Monitor, SocialSpace, LunchSpace, DishWasher, Coffee, Fridge, Toilet,
		Sink, Dryer, Presentation, LampSwitch, Lamp, ThrowTrash, Microwave, Home, Work };

	//Definition of a quest step
	[System.Serializable]
	public class Session {
		public string title;
		public float probability = 1f;
		public SessionType type;
        public Appliance appliance = null;
		public Target target;
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
        if(sessions[_currSession].type == SessionType.WaitForDuration) {
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
            DebugManager.Log(_ai.name + " starting (activity.run()) activity " + name + " start " + startTimeView + ", currTime is " + curTimeView, this);
        }
        else
        {
            DebugManager.Log(_ai.name + " starting (activity.run()) activity " + name + " without startTime, curTime is " + curTimeView, this);
        }


        StartSession(sessions[_currSession]);
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
            case SessionType.SitUntilEnd:
                _ai.SitAt(session.target, session.avatarOwnsTarget);
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
                    _ai.WalkTo(session.target, session.avatarOwnsTarget);
                }
				break;
			//case SessionType.Interact:
			//	NextSession();
			//	break;
			case SessionType.Warp:
                if (session.appliance != null)
                {
                    _ai.WarpTo(session.appliance, session.avatarOwnsTarget);
                }
                else
                {
                    _ai.WarpTo(session.target, session.avatarOwnsTarget);
                }
				break;
			case SessionType.SetRunlevel:
                if(session.appliance != null)
                {
                    _ai.SetRunLevel(session.appliance, session.parameter);
                }
                else
                {
                    _ai.SetRunLevel(session.target, session.parameter);
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
                    _ai.TurnOn(session.target);
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
                    _ai.TurnOff(session.target);
                }

                NextSession();
                break;
            default:
                DebugManager.Log("unknown SessionType", this);
                break;
		}
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
        DebugManager.Log("incrementing _currSssion", this);
        _currSession++;

		if(_currSession >= sessions.Count) {
            DebugManager.Log("_currSession out of bound. No more sessions in this activity. calling activityOver callback", this);
            _ai.OnActivityOver();
		} else {
            //Debug.Log("starting session" + sessions[_currSession].title);
            StartSession(sessions[_currSession]);
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
        DebugManager.Log(_ai.name + " reached destination " + sessions[_currSession].target + ". if current SessionType is walkTo, start next session", this);
        //if (sessions[_currSession].type == SessionType.WalkTo) {//Why do we perform this check? I don't know. Gunnar.
			NextSession();
		//}
	}

	//
	public void FinishCurrentActivity() {
        DebugManager.Log("Gonna finish this activity. Sessions are currently: " + sessions, this);

        //Remove the sessions already performed
        if (_currSession > 0) {
			sessions.RemoveRange(0, _currSession - 1);
		}

        DebugManager.Log("After deletion sessions are: " + sessions, this);
        //Simulate the sessions not yet performed
        foreach (Session session in sessions) {
			SimulateSession(session);
		}

        //Trigger callback for finished activity
		_ai.OnActivityOver();
	}

	//
	public void Revert() {
	}

	//
	public void SimulateSession(Session session) {
		switch(session.type) {
			case SessionType.WaitForDuration:
				break;
			case SessionType.WaitUntilEnd:
				break;
			case SessionType.WalkTo:
                //Debug.Log("Simulating WalkTo. Teleporting to " + session.target);
				_ai.WarpTo(session.target, session.avatarOwnsTarget);
				break;
			//case SessionType.Interact:
   //             //Debug.Log("Simulating Interaction. Interacting with " + session.target);
   //             _ai.CheckLighting(_ai.FindNearestDevice(session.target, session.avatarOwnsTarget));
			//	break;
			default:
                break;
		}
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
}
