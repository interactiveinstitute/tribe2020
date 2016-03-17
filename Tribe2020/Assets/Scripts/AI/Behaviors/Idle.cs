using UnityEngine;
using System.Collections;

[CreateAssetMenu(fileName = "Idle", menuName = "Behaviours/Idle", order = 1)]
public class Idle : BaseBehaviour {
	private const string WAITING = "waiting";
	
	public override void Init(BehaviourAI ai) {
		ai.curState = WAITING;
		ai.delay = Random.Range(1, 3);
	}

	public override void Step(BehaviourAI ai){
		base.Step(ai);

		if(ai.delay <= 0){
			ai.OnBehaviorOver();
		}
	}
}
