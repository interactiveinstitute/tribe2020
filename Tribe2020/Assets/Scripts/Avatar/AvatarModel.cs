using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AvatarModel : MonoBehaviour {

    public int modelId;

	// Use this for initialization
	void Awake () {     
    }

    void Start() {  
    }

    // Update is called once per frame
    void Update () {
		
	}

    void OnValidate() {

        Transform existingModel = transform.FindChild("Model");

        if (existingModel == null) {

            AvatarManager am = FindObjectOfType<AvatarManager>();
            AvatarModels models = am.models;
            AvatarModels.AvatarModelBundle modelBundle = models.models[modelId];

            GameObject goModel = Instantiate(modelBundle.model);
            goModel.name = "Model";
            SetLayerRecursive(goModel.transform);
            goModel.transform.parent = transform;
            goModel.transform.localPosition = Vector3.zero;

            //Set animator rig
            Animator animator = gameObject.GetComponent<Animator>();
            animator.avatar = modelBundle.avatar;
        }

    }

    void SetLayerRecursive(Transform transform) {
        transform.gameObject.layer = LayerMask.NameToLayer("Avatars");
        foreach (Transform child in transform) {
            SetLayerRecursive(child);
        }
    }

    public void RandomizeClothes() {
        AvatarManager am = FindObjectOfType<AvatarManager>();
        int modelId = GetComponent<AvatarModel>().modelId;
        AvatarModels.AvatarModelBundle modelBundle = am.models.models[modelId];

        foreach (AvatarModels.TexturedBodyPart bodyPart in modelBundle.bodyParts) {
            Material material = bodyPart.materials[Random.Range(0, bodyPart.materials.Count)];
            SetMaterial(bodyPart.model.name, material);
        }

    }

    void SetMaterial(string modelName, Material material) {
        transform.FindChild("Model/" + modelName).GetComponent<SkinnedMeshRenderer>().material = material;
    }

}
