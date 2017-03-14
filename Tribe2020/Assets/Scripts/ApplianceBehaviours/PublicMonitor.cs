using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PublicMonitor : MonoBehaviour {

    public Material screenMaterial;

    private MeshRenderer renderer;
    float timeLastChange;

	// Use this for initialization
	void Start () {
        renderer = GetComponentInChildren<MeshRenderer>();
	}
	
	// Update is called once per frame
	void Update () {
        if (GetComponent<ElectricDevice>().Power > 0.0f) {
            if (Time.time - timeLastChange > 1) {
                renderer.materials[1].color = new Color(Random.value, Random.value, Random.value);
                timeLastChange = Time.time;
            }
        }
    }
}
