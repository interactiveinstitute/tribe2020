﻿using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class AvatarLabel : MonoBehaviour {

	// Use this for initialization
	void Start () {
        GetComponent<Text>().text = transform.parent.parent.GetComponent<AvatarStats>().avatarName;
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
