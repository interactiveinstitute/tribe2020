using UnityEngine;
using System.Collections.Generic;

public class AudioManager : MonoBehaviour{
	//Singleton features
	private static AudioManager _instance;
	public static AudioManager GetInstance(){
		return _instance;
	}

	private AudioInterface _interface;

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
	public void SetInterface(AudioInterface i) {
		_interface = i;
	}

	//
	public void PlaySound(string key) {
		if(mute) { return; }

		foreach(AudioWrapper sound in sounds) {
			if(key == sound.key) {
				sound.value.Play();
			}
		}
	}

	//
	public void StopSound(string key) {
		foreach(AudioWrapper sound in sounds) {
			if(key == sound.key) {
				sound.value.Stop();
			}
		}
	}
}
