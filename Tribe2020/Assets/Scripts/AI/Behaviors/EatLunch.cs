using UnityEngine;
using System.Collections;

[System.Serializable]
public class EatLunch : AgentBehavior {
    private const string START = "start_coffee";
	private const string WALK_TO_COFFEE = "walk_to_coffee";
	private const string DRINK_COFFEE = "drink_coffee";

	public EatLunch(float weight): base(weight){
	}
	
	public override void Start(){
		_curState = START;
//		Debug.Log(WALK_TO_COFFEE);
	}

	public override void Update(SimpleAI ai){
		base.Update(ai);

		if (_curState == START) {
            _curState = WALK_TO_COFFEE;
			ai.GoTo ("drink_coffee");
		}

		//Finished drinking the coffee
		if (_curState == DRINK_COFFEE && _delay <= 0) {
            Debug.Log("DrinkCoffee is over");
            ai.OnBehaviorOver ();
		}
	}

	public override void OnHasReached(string tag){
		//Reached the coffee machine
		if (_curState == WALK_TO_COFFEE/* && tag == "drink_coffee"*/) {
			_curState = DRINK_COFFEE;
			_delay = 3;
//			Debug.Log(DRINK_COFFEE);
		}
	}
}
