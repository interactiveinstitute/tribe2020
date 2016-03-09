using UnityEngine;
using System.Collections;

[System.Serializable]
public class WarmUp : AgentBehavior {
	private const string WALK_TO_WARMER_PLACE = "walk_to_warmer_place";
	private const string WARMING_UP = "warming_up";

	public WarmUp(float weight): base(weight){
	}
	
	public override void Start(){
//		Debug.Log("now warming up");
	}

	public override void Update(SimpleAI ai){
		base.Update(ai);

		//TODO
		ai.OnBehaviorOver();
	}
}
