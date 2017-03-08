using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnergyEffeciencyLabels : MonoBehaviour {

    public enum Name { AAAA, AAA, AA, A, B, C, D, E, F}

    [System.Serializable]
    public struct EnergyEffeciencyLabel {
        public Name name;
        public Sprite sprite;
    }

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

}
