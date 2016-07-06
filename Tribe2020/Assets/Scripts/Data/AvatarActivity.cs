using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "Behaviour", menuName = "Avatar/Behaviour", order = 1)]
public class AvatarActivity : ScriptableObject {
	//public string name;

	public List<string> sessions;
	public List<Session> sessions2;
	public string onSkipCommand;
	private int _curStep;

	protected float _weight = 0f;
	protected string _curState = "";
	public float _delay = 0f;

	//Activity session types
	public enum SessionType { WalkTo, WaitForDuration, WaitUntilEnd };
	//Energy efficieny check types
	public enum EfficiencyType { None, Ligthing, Heating, Cooling, Device };
	//Energy efficieny check types
	public enum CheckType { LessThan, GreaterThan };

	//Definition of a quest step
	[System.Serializable]
	public class Session {
		public string title;
		public SessionType type;
		public string parameter;
		public bool avatarOwnsTarget;
		public EfficiencyType relatedEfficieny;
		public CheckType checkType;
		public float efficienyLevel;
	}

	public virtual void Init(BehaviourAI ai) {
		_curStep = 0;
		ExecuteCommand(ai, sessions[_curStep]);
	}

	public virtual void Step(BehaviourAI ai) {
		//if(_delay > 0f) {
		//	_delay -= Time.deltaTime;
		//}
	}

	public virtual void Update() {
		//if(_delay > 0f) {
		//	_delay -= Time.deltaTime;
		//}
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

		ExecuteCommand(ai, onSkipCommand);
	}

	//
	public void ResumeSession(BehaviourAI ai) {
		ExecuteCommand(ai, sessions[_curStep]);
	}

	//
	public void NextStep(BehaviourAI ai) {
		//Debug.Log(ai.name + ".CurrentStep" + _curStep + ", " + sessions.Count + ", " + sessions[_curStep]);
		_curStep++;

		if(_curStep == sessions.Count) {
			OnActivityDone(ai);
		} else {
			//Debug.Log(ai.name + ".NextStep" + _curStep + ", " + sessions.Count + ", " + sessions[_curStep]);
			//Debug.Log("NextStep" + _curStep + ", " + sessions.Count + ", " + sessions[_curStep]);
			ExecuteCommand(ai, sessions[_curStep]);
		}
	}

	//
	public void OnActivityDone(BehaviourAI ai) {
		//Debug.Log("OnBehaviourDone");
		ai.OnActivityOver();
	}
}
