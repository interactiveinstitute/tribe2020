using UnityEngine;
using System.Collections;

[CreateAssetMenu(fileName = "OfficeWork", menuName = "Behaviours/Office Work", order = 1)]
public class OfficeWork : BaseBehaviour {
    private const string START = "start_office";
	private const string WALK_TO_OFFICE = "walk_to_office";
	private const string WORK_AT_OFFICE = "work_at_office";
	private const string OFFICE = "appliance_office";

	//
	public override void Init(BehaviourAI ai){
		ai.curState = START;
	}

	//
	public override void Step(BehaviourAI ai) {
		base.Step(ai);

		//Walk to coffee
		if(ai.curState == START) {
			ai.curState = WALK_TO_OFFICE;
			ai.GoTo(OFFICE);
		}

		//Update delay while drinking coffee
		if(ai.curState == WORK_AT_OFFICE) {
			ai.delay -= Time.deltaTime;
		}

		//Finished drinking the coffee
			if(ai.curState == WORK_AT_OFFICE && ai.delay <= 0) {
			Debug.Log("Done " + WORK_AT_OFFICE);
			ai.OnBehaviorOver ();
		}
	}

	//
	public override void OnHasReached(BehaviourAI ai, string tag){
		//Reached the coffee machine
		if (ai.curState == WALK_TO_OFFICE/* && tag == "drink_coffee"*/) {
			ai.curState = WORK_AT_OFFICE;
			ai.delay = 60;
		}
	}
}
