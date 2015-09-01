using UnityEngine;
using System.Collections;

public class AgentBehavior{
	protected float _weight = 0f;
	protected string _curState = "";
	protected float _delay = 0f;

	public AgentBehavior(float weight){
		_weight = weight;
	}

	public virtual void Start(){
	}

	public virtual void Update(SimpleAI ai){
		if (_delay > 0f) {
			_delay -= Time.deltaTime;
		}
	}

	public virtual void OnHasReached(string tag){
	}

	public virtual bool IsDone(){
		return true;
	}
}
