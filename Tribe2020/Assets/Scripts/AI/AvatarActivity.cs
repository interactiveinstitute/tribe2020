using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "Behaviour", menuName = "Avatar/Behaviour", order = 1)]
public class AvatarActivity : ScriptableObject {
	//public string name;

	public List<string> sessions;
	public string onSkipCommand;
	private int _curStep;

	protected float _weight = 0f;
	protected string _curState = "";
	public float _delay = 0f;

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
		ExecuteCommand(ai, onSkipCommand);
	}

	//
	public void NextStep(BehaviourAI ai) {
		_curStep++;

		if(_curStep == sessions.Count) {
			OnBehaviourDone(ai);
		} else {
			ExecuteCommand(ai, sessions[_curStep]);
		}
	}

	//
	public void OnBehaviourDone(BehaviourAI ai) {
		ai.OnActivityOver();
	}
}
