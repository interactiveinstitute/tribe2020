using UnityEngine;
using System.Collections;
using System;

[System.Serializable] 
public class Runlevel {

	public string Name;
	public float Power;

	public Material[] materials;
	public AudioClip sound; 
	public bool LoopSound;

	public Light[] LightsOn;
	public Light[] LightsOff;
}
