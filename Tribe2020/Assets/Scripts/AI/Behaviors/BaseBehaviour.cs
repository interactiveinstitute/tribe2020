using UnityEngine;
using System.Collections;

public class BaseBehaviour : ScriptableObject {
	public string name;

	protected float _weight = 0f;
	protected string _curState = "";
	public float _delay = 0f;

	public virtual void Init(BehaviourAI ai) {
	}

	public virtual void Step(BehaviourAI ai) {
		if(_delay > 0f) {
			_delay -= Time.deltaTime;
		}
	}

	public virtual void Update() {
		if(_delay > 0f) {
			_delay -= Time.deltaTime;
		}
	}

	public virtual void OnHasReached(BehaviourAI ai, string tag) {
	}

	public virtual bool IsDone() {
		return true;
	}
}
