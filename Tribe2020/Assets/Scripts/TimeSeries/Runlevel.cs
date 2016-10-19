using UnityEngine;
using System.Collections;
using System;

[System.Serializable] 
public class Runlevel {

	public string Name;
	public float Power;

	[Header("Change materials")]
	public Renderer Target;
	public Material[] materials;
	public AudioClip sound; 
	public bool LoopSound;
	public Material[] default_materials; 

	public Light[] LightsOn;
	public Light[] LightsOff;


}
