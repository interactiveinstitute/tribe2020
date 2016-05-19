using UnityEngine;
using System.Collections;

public class MenuView : MonoBehaviour {
	public Animator animator;

	// Use this for initialization
	void Start () {
	    
	}
	
	// Update is called once per frame
	void Update () {
		animator.SetFloat("Speed", 100);
	}
}
