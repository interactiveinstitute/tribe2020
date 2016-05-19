using UnityEngine;
using System.Collections;

public class MenuController : MonoBehaviour {
	public GameObject animatedCharacter;

	private CustomSceneManager _sceneMgr;

	// Use this for initialization
	void Start () {
		_sceneMgr = CustomSceneManager.GetInstance();
	}
	
	// Update is called once per frame
	void Update () {
		Animator anim = animatedCharacter.GetComponent<Animator>();
		anim.SetFloat("Speed", 100);
	}

	//
	public void LoadScene(string scene) {
		_sceneMgr.LoadScene(scene);
	}
}
