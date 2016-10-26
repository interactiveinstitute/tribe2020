using UnityEngine;
using System.Collections;
using System;

[System.Serializable] 
public class Runlevel {

	public string Name;
	public float Power;

	[Header("Change materials")]
	[Space(10)]
	[Tooltip("If the material to be changed is not on the same gameobject as this script this property needs to be set.")]
	public Renderer Target;
	[Tooltip("The materials insert. If you leave the field empty the default material will be used. To for example change only the second material set the size to 2 and leave the first field blank.")]
	public Material[] materials;
	public AudioClip sound; 
	public bool LoopSound;

    public Light[] LightsOn;
	public Light[] LightsOff;
    private Material[] default_materials;
    
    //Gunnars way of maing this one hidden in the editor
    public Material[] Default_materials{
        get { return default_materials; }
        set { default_materials = value; }
    }

}
