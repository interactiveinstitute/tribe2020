using UnityEngine;
using System.Collections;

[CreateAssetMenu(fileName = "Behaviours", menuName = "Behaviours/Drink Coffee", order = 1)]
public class DrinkCoffee : BaseBehaviour {
    private const string START = "start_coffee";
	private const string WALK_TO_COFFEE = "walk_to_coffee";
	private const string DRINK_COFFEE = "drink_coffee";

	public override void Init(BehaviourAI ai){
		ai.curState = START;
//		Debug.Log(WALK_TO_COFFEE);
	}

	public override void Step(BehaviourAI ai) {
		base.Step(ai);

		if(ai.curState == START) {
			ai.curState = WALK_TO_COFFEE;
			ai.GoTo ("drink_coffee");
		}

		if(ai.curState == DRINK_COFFEE) {
			ai.delay -= Time.deltaTime;
		}

		//Finished drinking the coffee
			if(ai.curState == DRINK_COFFEE && ai.delay <= 0) {
			Debug.Log("DrinkCoffee is over");
			ai.OnBehaviorOver ();
		}
	}

	//public void Update(){
	//	base.Update();

	//	if (_curState == START) {
 //           _curState = WALK_TO_COFFEE;
	//		//ai.GoTo ("drink_coffee");
	//	}

	//	//Finished drinking the coffee
	//	if (_curState == DRINK_COFFEE && _delay <= 0) {
 //           Debug.Log("DrinkCoffee is over");
 //           //ai.OnBehaviorOver ();
	//	}
	//}

	public override void OnHasReached(BehaviourAI ai, string tag){
		//Reached the coffee machine
		if (ai.curState == WALK_TO_COFFEE/* && tag == "drink_coffee"*/) {
			ai.curState = DRINK_COFFEE;
			ai.delay = 3;
//			Debug.Log(DRINK_COFFEE);
		}
	}
}
