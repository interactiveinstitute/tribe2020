using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class AvatarBehaviour : MonoBehaviour {
	public string Name;
	public string[] Requirements;
	public float[] Level;
	public string[] Yields;


	public string[][] rest;

	// Use this for initialization
	void Run () {
		gameObject.SendMessage(Name);
	}
	




}
