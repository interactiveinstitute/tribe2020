using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AvatarLooks : MonoBehaviour {

    [System.Serializable]
    private struct LooksMaleOptions {
        public List<Material> hair;
        public List<Material> skin; 
        public List<Material> shirts;
        public List<Material> suits;
        public List<Material> ties;
        public List<Material> pants;
        public List<Material> shoes;
    }

    [System.Serializable]
    public struct LooksMale {
        public Material hair;
        public Material skin;
        public Material shirt;
        public Material suit;
        public Material tie;
        public Material pants;
        public Material shoes;
    }

    [System.Serializable]
    private struct LooksFemaleOptions {
        public List<Material> dress;
        public List<Material> shoes;
    }

    [System.Serializable]
    public struct LooksFemale {
        public Material dress;
        public Material shoes;
    }

    [SerializeField]
    LooksMaleOptions male;
    [SerializeField]
    LooksFemaleOptions female;

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    Material GetRandomMaterial(List<Material> materials) {
        return materials[Random.Range(0, materials.Count)];
    }

    public LooksMale GenerateMaleLooks() {
        LooksMale looks = new LooksMale();

        looks.hair = GetRandomMaterial(male.hair);
        looks.skin = GetRandomMaterial(male.skin);
        looks.shirt = GetRandomMaterial(male.shirts);
        looks.suit = GetRandomMaterial(male.suits);
        looks.tie = GetRandomMaterial(male.ties);
        looks.pants = GetRandomMaterial(male.pants);
        looks.shoes = GetRandomMaterial(male.shoes);

        return looks;
    }

    public LooksFemale GenerateFemaleLooks() {
        LooksFemale looks = new LooksFemale();

        looks.dress = GetRandomMaterial(female.dress);
        looks.shoes = GetRandomMaterial(female.shoes);

        return looks;
    }

}
