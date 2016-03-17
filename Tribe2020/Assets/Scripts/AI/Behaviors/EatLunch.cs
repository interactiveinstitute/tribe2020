using UnityEngine;
using System.Collections;

[CreateAssetMenu(fileName = "EatLunch", menuName = "Behaviours/Eat Lunch", order = 1)]
public class EatLunch : BaseBehaviour {
    private const string START = "start_lunch";
	private const string WALK_TO_LUNCH = "walk_to_lunch";
	private const string EAT_LUNCH = "eat_lunch";
	private const string FRIDE = "appliance_fridge";
	
	//
	public override void Init(BehaviourAI ai) {
		ai.curState = START;
	}

	//
	public override void Step(BehaviourAI ai){
		base.Step(ai);

		if (ai.curState == START) {
            ai.curState = WALK_TO_LUNCH;
			ai.GoTo (FRIDE);
		}

		//Finished drinking the coffee
		if (ai.curState == EAT_LUNCH && ai.delay <= 0) {
            Debug.Log("Done " + EAT_LUNCH);
            ai.OnBehaviorOver ();
		}
	}

	//
	public override void OnHasReached(BehaviourAI ai, string tag){
		//Reached the coffee machine
		if (ai.curState == WALK_TO_LUNCH/* && tag == "drink_coffee"*/) {
			ai.curState = EAT_LUNCH;
			ai.delay = 3;
//			Debug.Log(DRINK_COFFEE);
		}
	}
}
