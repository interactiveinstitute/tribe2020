using UnityEngine;
using System.Collections;

[CreateAssetMenu(fileName = "Behaviours", menuName = "Behaviours/Drink Coffee", order = 1)]
public class DrinkCoffee : BaseBehaviour {
    private const string START = "start_coffee";
	private const string WALK_TO_COFFEE = "walk_to_coffee";
	private const string DRINK_COFFEE = "drink_coffee";
	private const string COFFEE = "appliance_coffee";

	//
	public override void Init(BehaviourAI ai){
		ai.curState = START;
	}

	//
	public override void Step(BehaviourAI ai) {
		base.Step(ai);

		//Walk to coffee
		if(ai.curState == START) {
			ai.curState = WALK_TO_COFFEE;
			ai.GoTo(COFFEE);
		}

		//Update delay while drinking coffee
		if(ai.curState == DRINK_COFFEE) {
			ai.delay -= Time.deltaTime;
		}

		//Finished drinking the coffee
		if(ai.curState == DRINK_COFFEE && ai.delay <= 0) {
			Debug.Log("Done " + DRINK_COFFEE);
			ai.OnBehaviorOver ();
		}
	}

	//
	public override void OnHasReached(BehaviourAI ai, string tag){
		//Reached the coffee machine
		if (ai.curState == WALK_TO_COFFEE/* && tag == "drink_coffee"*/) {
			ai.curState = DRINK_COFFEE;
			ai.delay = 60;
		}
	}
}
