using UnityEngine;
using System.Collections;

public class AnimationCallback : MonoBehaviour {
	public NarrationInterface listener;

	// Use this for initialization
	void Start () {
		NarrationManager narrationMgr = NarrationManager.GetInstance();
		listener = narrationMgr.GetInterface();
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	//
	public void OnAnimationEvent(string animationEvent){
		listener.OnAnimationEvent(animationEvent);
	}
}
