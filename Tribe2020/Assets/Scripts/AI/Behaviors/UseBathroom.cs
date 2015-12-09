using UnityEngine;
using System.Collections;

public class UseBathroom : AgentBehavior {
	private const string WALK_TO_TOILET = "walk_to_toilet";
	private const string USE_TOILET = "use_toilet";

	public UseBathroom(float weight): base(weight){
	}
	
	public override void Start(){
		_curState = WALK_TO_TOILET;
//		Debug.Log(WALK_TO_TOILET);
	}

	public override void Update(SimpleAI ai){
		base.Update(ai);

		if (_curState == WALK_TO_TOILET) {
			ai.GoTo ("toilet");
		}

		//Finished using the toilet
		if (_curState == USE_TOILET && _delay <= 0) {
			ai.OnBehaviorOver ();
		}
	}

	public override void OnHasReached(string tag){
		//Reached the toilet
		if (_curState == WALK_TO_TOILET && tag == "toilet") {
			_curState = USE_TOILET;
			_delay = 5;
//			Debug.Log(USE_TOILET);
		}
	}
}
