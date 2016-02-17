using UnityEngine;
using System.Collections;

public class DrinkCoffee : AgentBehavior {
	private const string WALK_TO_COFFEE = "walk_to_coffee";
	private const string DRINK_COFFEE = "drink_coffee";

	public DrinkCoffee(float weight): base(weight){
	}
	
	public override void Start(){
		_curState = WALK_TO_COFFEE;
//		Debug.Log(WALK_TO_COFFEE);
	}

	public override void Update(SimpleAI ai){
		base.Update(ai);

		if (_curState == WALK_TO_COFFEE) {
			ai.GoTo ("coffee");
		}

		//Finished drinking the coffee
		if (_curState == DRINK_COFFEE && _delay <= 0) {
			ai.OnBehaviorOver ();
		}
	}

	public override void OnHasReached(string tag){
		//Reached the coffee machine
		if (_curState == WALK_TO_COFFEE && tag == "coffee") {
			_curState = DRINK_COFFEE;
			_delay = 3;
//			Debug.Log(DRINK_COFFEE);
		}
	}
}
