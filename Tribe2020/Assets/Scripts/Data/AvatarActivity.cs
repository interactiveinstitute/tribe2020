﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "Behaviour", menuName = "Avatar/Behaviour", order = 1)]
public class AvatarActivity : ScriptableObject {
	private GameTime _timeMgr;

	public List<Session> sessions;
	public Session SkipSession;

	public string onSkipCommand;
	private int _curStep;
	private BehaviourAI _ai;

	protected float _weight = 0;
	protected string _curState = "";

	public float _delay = 0;
	public float _duration = 0;

	public double startTime = 0;
	public double endTime = 0;

	//Activity session types
	public enum SessionType { WalkTo, WaitForDuration, WaitUntilEnd, SetRunlevel, Interact, Teleport };
	//Energy efficieny check types
	public enum EfficiencyType { None, Ligthing, Heating, Cooling, Device };
	//Energy efficieny check types
	public enum CheckType { LessThan, GreaterThan };
	//
	public enum Target { None, OfficeDesk, SocialSpace, LunchSpace, DishWasher, Coffee, Fridge, Toilet,
		Sink, Dryer, Presentation, LampSwitch, Lamp, ThrowTrash, Microwave, Home };

	//Definition of a quest step
	[System.Serializable]
	public class Session {
		public string title;
		public float probability = 1f;
		public SessionType type;
		public Target target;
		public string parameter;
		public bool avatarOwnsTarget;
		public bool currentRoom;
		public EfficiencyType relatedEfficieny;
		public CheckType checkType;
		public float efficienyLevel;
	}

	public virtual void Init(BehaviourAI ai) {
		

		_curStep = 0;
		//ExecuteCommand(ai, sessions[_curStep]);
	}

	public virtual void Step(BehaviourAI ai) {
        if (sessions[_curStep].type == SessionType.WaitForDuration)
        {
            if (_delay > 0f)
            {
                _delay -= Time.deltaTime;
            }else
            {
                NextSession();
            }
        }
	}

    public void Wait() {
        if (_delay > 0f)
        {
            _delay -= Time.deltaTime;
        }else
        {
            NextSession();
        }
    }

	//
	public void Init(BehaviourAI ai, double startTime, double endTime) {
		_timeMgr = GameTime.GetInstance();

		_ai = ai;
		this.startTime = startTime;
		this.endTime = endTime;

		_curStep = 0;
		

		//ExecuteCommand(ai, sessions[_curStep]);
	}

	//
	public void Run() {
		string startTimeView = _timeMgr.TimestampToDateTime(startTime).ToString("HH:mm");
		string endTimeView = _timeMgr.TimestampToDateTime(endTime).ToString("HH:mm");
        string currTime = _timeMgr.GetDateTime().ToString("HH:mm");//TimestampToDateTime(_timeMgr.time).ToString("HH:mm");
		Debug.Log(_ai.name + " doing " + name + " start " + startTimeView + ", end " + endTimeView + ", currTime is "+ currTime);

		StartSession(sessions[_curStep]);
	}

	//
	public void StartSession(Session session) {
		Debug.Log(_ai.name + " started session " + session.title);
		switch(session.type) {
			case SessionType.WaitForDuration:
                _delay = int.Parse(session.parameter);
                _ai.Wait();
				break;
			case SessionType.WaitUntilEnd:
				_ai.Stop();
				break;
			case SessionType.WalkTo:
				_ai.WalkTo(session.target, session.avatarOwnsTarget);
				break;
			case SessionType.Interact:
				NextSession();
				break;
			case SessionType.Teleport:
				_ai.TeleportTo(session.target, session.avatarOwnsTarget);
				break;
			case SessionType.SetRunlevel:
				_ai.SetRunLevel(session.target, session.parameter);
				NextSession();
				break;
		}
	}

	//
	public void InsertSession(Session session) {
		sessions.Insert(_curStep, session);
		StartSession(session);
	}

	//
	public void NextSession() {
		_curStep++;

		if(_curStep >= sessions.Count) {
			_ai.OnActivityOver();
		} else {
			StartSession(sessions[_curStep]);
		}
	}

	//
	public virtual void Update() {
		//_duration += Time.deltaTime;
		//if(_duration >= _endTime) {
		//	Simulate();
		//}

		if(sessions[_curStep].type == SessionType.WaitForDuration) {
			_delay -= Time.deltaTime;
			if(_delay <= 0) {
				NextSession();
			}
		}
	}

	//
	public void OnDestinationReached() {
		Debug.Log(_ai.name + " reached destination " + sessions[_curStep].target);
		if(sessions[_curStep].type == SessionType.WalkTo) {
			NextSession();
		}
	}

	//
	public void QuickRun() {
		if(_curStep > 0) {
			sessions.RemoveRange(0, _curStep - 1);
		}

		foreach(Session session in sessions) {
			SimulateSession(session);
		}

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
				_ai.TeleportTo(session.target, session.avatarOwnsTarget);
				break;
			case SessionType.Interact:
				_ai.CheckLighting(_ai.FindNearestDevice(session.target, session.avatarOwnsTarget));
				break;
			default:
                break;
		}
	}

	public virtual void OnHasReached(BehaviourAI ai, string tag) {
	}

	public virtual bool IsDone() {
		return true;
	}

	//
	public void ExecuteCommand(BehaviourAI ai, string command) {
		string[] cmdParse = command.Split(',');
		string cmdFunction = cmdParse[0];
		string[] cmdArgs = new string[cmdParse.Length - 1];
		for(int i = 0; i < cmdParse.Length - 1; i++) {
			cmdArgs[i] = cmdParse[i + 1];
		}

		//Debug.Log(ai.gameObject.GetComponent<AvatarStats>().avatarName + ", command: " + cmdFunction + ", " + cmdArgs[0]);
		if(cmdFunction == "Delay") {
			ai.gameObject.SendMessage(cmdFunction, float.Parse(cmdArgs[0]));
		} else {
			ai.gameObject.SendMessage(cmdFunction, cmdArgs);
		}
	}

	//
	public void SimulateExecution(BehaviourAI ai) {


		//Debug.Log("SimulateExecution:" + onSkipCommand);

		//ExecuteCommand(ai, onSkipCommand);
	}

	//
	public void ResumeSession(BehaviourAI ai) {
		_curStep = 0;
		StartSession(sessions[_curStep]);
		//ExecuteCommand(ai, sessions[_curStep]);
	}

	//
	public void NextStep(BehaviourAI ai) {
		//Debug.Log(ai.name + ".CurrentStep" + _curStep + ", " + sessions.Count + ", " + sessions[_curStep]);
		_curStep++;

		//if(_curStep == sessions.Count) {
		//	OnActivityDone(ai);
		//} else {
		//	//Debug.Log(ai.name + ".NextStep" + _curStep + ", " + sessions.Count + ", " + sessions[_curStep]);
		//	//Debug.Log("NextStep" + _curStep + ", " + sessions.Count + ", " + sessions[_curStep]);
		//	ExecuteCommand(ai, sessions[_curStep]);
		//}
	}

	//
	public void OnActivityDone(BehaviourAI ai) {
		//Debug.Log("OnBehaviourDone");
		ai.OnActivityOver();
	}
}
