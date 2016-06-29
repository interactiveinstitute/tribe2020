using UnityEngine;
using System.Collections;

public class AudioManager : MonoBehaviour{
	//Singleton features
	private static AudioManager _instance;
	public static AudioManager GetInstance(){
		return _instance;
	}

	public bool mute;

	public AudioSource musicLoop;
	public AudioSource button;

	//Sort use instead of constructor
	void Awake(){
		_instance = this;
	}

	// Use this for initialization
	void Start(){
		musicLoop.Play();
	}
	
	// Update is called once per frame
	void Update(){
	
	}

	public void PlaySound(string sound){
		if(mute) { return; }

		switch(sound){
		case "music":
			musicLoop.Play(); break;
		case "button":
			button.Play(); break;
		}
	}
}
