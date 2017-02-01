using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Floor : MonoBehaviour {

    Light[] _lights;

	// Use this for initialization
	void Start () {
        _lights = GetComponentsInChildren<Light>();
        ToggleLightActive(false);
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    void OnAvatarExitRoom() {
    }

    int GetAvatarCount() {
        int nAvatars = 0;
        foreach(Transform child in transform) {
            nAvatars += transform.GetComponent<Room>().GetAvatarCount();
        }
        return nAvatars;
    }

    bool IsEmpty() {
        return GetAvatarCount() == 0;
    }

    public void ToggleLightActive(bool state) {
        foreach(Light light in _lights) {
            light.gameObject.SetActive(state);
        }
    }

}
