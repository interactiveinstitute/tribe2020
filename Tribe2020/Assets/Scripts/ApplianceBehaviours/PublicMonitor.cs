using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PublicMonitor : MonoBehaviour {

    private MeshRenderer meshRenderer;
    float timeLastChange;

    bool _isOn;

	// Use this for initialization
	void Start () {
        meshRenderer = GetComponentInChildren<MeshRenderer>();
	}
	
	// Update is called once per frame
	void Update () {
		if (GetComponent<ElectricDevice>().GetPower() > 0.0f) {
            _isOn = true;
            if (Time.time - timeLastChange > 1) {
                meshRenderer.materials[1].color = new Color(Random.value, Random.value, Random.value);
                timeLastChange = Time.time;
            }
        }
        else if(_isOn) {
            _isOn = false;
            meshRenderer.materials[1].color = Color.black;
        }
    }
}
