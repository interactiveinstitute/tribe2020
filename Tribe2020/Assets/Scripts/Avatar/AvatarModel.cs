using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AvatarModel : MonoBehaviour {

    public int modelId;
    AvatarModels.AvatarModelBundle _modelBundle;

	// Use this for initialization
	void Awake () {
        AvatarManager am = FindObjectOfType<AvatarManager>();
        _modelBundle = am.models.models[modelId];
    }

    void Start() {  
    }

    // Update is called once per frame
    void Update () {
		
	}

    public void InstantiateModel() {

        Transform existingModel = transform.FindChild("Model");
        AvatarManager am = FindObjectOfType<AvatarManager>();

        if (existingModel == null && (modelId >= 0 && modelId < am.models.models.Count)) {

            _modelBundle = am.models.models[modelId];

            GameObject goModel = Instantiate(_modelBundle.model);
            goModel.name = "Model";
            SetLayerRecursive(goModel.transform);
            goModel.transform.parent = transform;
            goModel.transform.localPosition = Vector3.zero;

            //Set animator rig
            Animator animator = gameObject.GetComponent<Animator>();
            animator.avatar = _modelBundle.avatar;
        }
    }

    void SetLayerRecursive(Transform transform) {
        transform.gameObject.layer = LayerMask.NameToLayer("Avatars");
        foreach (Transform child in transform) {
            SetLayerRecursive(child);
        }
    }

    public void RandomizeClothes() {

        Transform existingModel = transform.FindChild("Model");
        if (existingModel != null) {
            AvatarManager am = FindObjectOfType<AvatarManager>();
            int modelId = GetComponent<AvatarModel>().modelId;
            _modelBundle = am.models.models[modelId];

            Material materialSkin = _modelBundle.materialsSkin[Random.Range(0, _modelBundle.materialsSkin.Count)];
            foreach (AvatarModels.BodyPartSkin bodyPartSkin in _modelBundle.bodyPartsSkin) {
                SetClothesMaterial(bodyPartSkin.model.name, materialSkin);
            }

            foreach (AvatarModels.BodyPartClothes bodyPart in _modelBundle.bodyPartsClothes) {
                Material material = bodyPart.materials[Random.Range(0, bodyPart.materials.Count)];
                SetClothesMaterial(bodyPart.model.name, material);
            }
        }
    }

    void SetClothesMaterial(string modelName, Material material) {
        transform.FindChild("Model/" + modelName).GetComponent<SkinnedMeshRenderer>().material = material;
    }

    Texture2D GetFaceTextureByMood(AvatarMood.Mood mood) {
        foreach(AvatarModels.FaceTexture faceTexture in _modelBundle.faceTextures) {
            if(faceTexture.mood == mood) {
                return faceTexture.texture;
            }
        }
        return null;
    }

    public void SetFaceTexture(AvatarMood.Mood mood) {
        Texture2D textureFace = GetFaceTextureByMood(mood);
        if (textureFace != null) {
            transform.FindChild("Model/" + _modelBundle.faceObject.name).GetComponent<SkinnedMeshRenderer>().material.mainTexture = textureFace;
        }
    }

    public void UpdateFaceTextureByCurrentMood() {
        AvatarMood.Mood mood = gameObject.GetComponent<AvatarMood>().GetCurrentMood();
        SetFaceTexture(mood);
    }

}
