using UnityEngine;
using System.Collections.Generic;

public class AudioManager : MonoBehaviour{
	//Singleton features
	private static AudioManager _instance;
	public static AudioManager GetInstance(){
		return _instance;
	}

	[System.Serializable]
	public struct AudioWrapper {
		public string key;
		public AudioSource value;
	}
	public bool mute;

	[Space(10)]
	public List<AudioWrapper> sounds;
	public string defaultMusic;

	//Sort use instead of constructor
	void Awake(){
		_instance = this;
	}

	// Use this for initialization
	void Start(){
		PlaySound(defaultMusic);
	}
	
	// Update is called once per frame
	void Update(){
	
	}

	//
	public void PlaySound(string key) {
		foreach(AudioWrapper sound in sounds) {
			if(key == sound.key) {
				sound.value.Play();
			}
		}
	}
}
