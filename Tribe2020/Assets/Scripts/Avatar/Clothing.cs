using UnityEngine;

public class Clothing : MonoBehaviour {

    public AvatarLooks.LooksMale looksMale;
    public AvatarLooks.LooksFemale looksFemale;

    void Awake() {
    }

    void Start() {

        AvatarManager.Gender gender = gameObject.GetComponent<AvatarModel>().gender;

        if(gender == AvatarManager.Gender.Male) {
            SetMaleMaterials();
        }
        else if(gender == AvatarManager.Gender.Female) {
            SetFemaleMaterials();
        }

    }

    public void RandomizeClothes()
    {
        AvatarManager am = FindObjectOfType<AvatarManager>();

        AvatarManager.Gender gender = gameObject.GetComponent<AvatarModel>().gender;

        if (gender == AvatarManager.Gender.Male) {
            looksMale = am.looks.GenerateMaleLooks();
        }
        else if (gender == AvatarManager.Gender.Female) {
            looksFemale = am.looks.GenerateFemaleLooks();
        }

    }

    void SetMaleMaterials() {
        SetMaterial("Hair", looksMale.hair);

        SetMaterial("Head", looksMale.skin);
        SetMaterial("Neck_Cube.016", looksMale.skin);
        SetMaterial("Hands", looksMale.skin);

        SetMaterial("Shirt_Cube.013", looksMale.shirt);
        SetMaterial("Suit", looksMale.suit);
        SetMaterial("Tie_Cube.012", looksMale.pants);

        SetMaterial("Pants", looksMale.pants);
        SetMaterial("Shoes", looksMale.shoes);
    }

    void SetFemaleMaterials() {

    }

    void SetMaterial(string modelName, Material material) {
        transform.FindChild("Model/" + modelName).GetComponent<SkinnedMeshRenderer>().material = material;
    }
}
