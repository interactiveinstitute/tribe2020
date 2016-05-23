using UnityEngine;
using System.Collections;

public class Sit : MonoBehaviour {


    private Animator anim;

	// Use this for initialization
	void Start () {
        anim = gameObject.GetComponentInChildren<Animator>();
    }
	
	// Update is called once per frame
	void Update () {

        if (Input.GetKey("f"))
        {
            anim.SetInteger("Sit", 1);
        }
        else
        {
            anim.SetInteger("Sit", 0);
        }

    }
}
