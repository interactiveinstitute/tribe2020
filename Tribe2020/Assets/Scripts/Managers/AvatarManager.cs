using UnityEngine;
using System.Collections.Generic;

public class AvatarManager : MonoBehaviour {
	//Singleton features
	private static AvatarManager _instance;
	public static AvatarManager GetInstance() {
		if(_instance != null) {
			return _instance;
		}
		return null;
	}

	private List<BehaviourAI> _avatars;

	//Sort use instead of constructor
	void Awake() {
		_instance = this;
	}

	// Use this for initialization
	void Start () {
		_avatars = new List<BehaviourAI>();

		GameObject[] avatarObjs = GameObject.FindGameObjectsWithTag("Avatar");
		foreach(GameObject avatarObj in avatarObjs) {
			_avatars.Add(avatarObj.GetComponent<BehaviourAI>());
		}
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	//
	public void MakeAvatarWalkTo(Vector3 target) {
		_avatars[0].WalkTo(target);
	}

	//
	public void MakeAvatarPerformActivity(AvatarActivity activity) {
		_avatars[0].StartActivity(activity);
	}
}
