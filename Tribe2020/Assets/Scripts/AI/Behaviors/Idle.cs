using UnityEngine;
using System.Collections;

public class Idle : AgentBehavior {
	private const string WAITING = "waiting";

	public Idle(float weight): base(weight){
	}
	
	public override void Start(){
		_delay = Random.Range(1, 3);

		Debug.Log("now idle" + _delay);
	}

	public override void Update(SimpleAI ai){
		base.Update(ai);

		Debug.Log (_delay);

		if(_delay <= 0){
			ai.OnBehaviorOver();
		}
	}
}
