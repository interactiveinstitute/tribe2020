﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AvatarModel : MonoBehaviour {

    public GameObject model;
    public Avatar avatar;
    public AvatarManager.Gender gender;

	// Use this for initialization
	void Awake () {
        GameObject goModel = Instantiate(model);
        Destroy(goModel.GetComponent<Animator>());
        goModel.name = "Model";
        SetLayerRecursive(goModel.transform);       
        goModel.transform.parent = transform;
        goModel.transform.localPosition = Vector3.zero;

        //Set animator rig
        Animator animator = gameObject.GetComponent<Animator>();
        animator.avatar = avatar;
    }

    void Start() {
        
    }

    // Update is called once per frame
    void Update () {
		
	}

    void SetLayerRecursive(Transform transform) {
        transform.gameObject.layer = LayerMask.NameToLayer("Avatars");
        foreach (Transform child in transform) {
            SetLayerRecursive(child);
        }
    }

}
