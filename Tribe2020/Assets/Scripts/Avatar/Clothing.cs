using UnityEngine;
using System.Collections.Generic;

public class Clothing : MonoBehaviour {

    void Awake() {
    }

    void Start() {
    }

    public void RandomizeClothes()
    {
        AvatarManager am = FindObjectOfType<AvatarManager>();
        int modelId = GetComponent<AvatarModel>().modelId;
        AvatarModels.AvatarModelBundle modelBundle = am.models.models[modelId];

        foreach(AvatarModels.TexturedBodyPart bodyPart in modelBundle.bodyParts) {
            Material material = bodyPart.materials[Random.Range(0, bodyPart.materials.Count)];

            Debug.Log(bodyPart.model.name);
            SetMaterial(bodyPart.model.name, material);
        }

    }

    void SetMaterial(string modelName, Material material) {
        transform.FindChild("Model/" + modelName).GetComponent<SkinnedMeshRenderer>().material = material;
    }

}
