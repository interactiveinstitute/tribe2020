using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AvatarModels : MonoBehaviour {

    [System.Serializable]
    public struct AvatarModelBundle {
        public AvatarManager.Gender gender;
        public GameObject model;
        public Avatar avatar;
        public List<TexturedBodyPart> bodyParts;
    }

    [System.Serializable]
    public struct TexturedBodyPart {
        public GameObject model;

        public List<Material> materials;
    }

    public List<AvatarModelBundle> models;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
