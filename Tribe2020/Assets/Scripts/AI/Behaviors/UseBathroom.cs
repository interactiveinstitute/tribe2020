using UnityEngine;
using System.Collections;

[CreateAssetMenu(fileName = "Behaviours", menuName = "Behaviours/Use Bathroom", order = 1)]
public class UseBathroom : BaseBehaviour {
    private const string START = "start_bathroom";
	private const string WALK_TO_TOILET = "walk_to_toilet";
	private const string USE_TOILET = "use_toilet";
	private const string TOILET = "appliance_toilet";

	//
	public override void Init(BehaviourAI ai) {
		ai.curState = START;
	}

	//
	public override void Step(BehaviourAI ai) {
		base.Step(ai);

		if(ai.curState == START) {
			ai.curState = WALK_TO_TOILET;
			ai.GoTo(TOILET);
		}

		//Update delay while drinking coffee
		if(ai.curState == USE_TOILET) {
			ai.delay -= Time.deltaTime;
		}

		//Finished using the toilet
		if(ai.curState == USE_TOILET && ai.delay <= 0) {
			Debug.Log("Done " + USE_TOILET);
			ai.OnBehaviorOver();
		}
	}

	//
	public override void OnHasReached(BehaviourAI ai, string tag){
		//Reached the toilet
		if (ai.curState == WALK_TO_TOILET/* && tag == "go_to_bathroom"*/) {
			ai.curState = USE_TOILET;
			ai.delay = 60;
		}
	}
}
