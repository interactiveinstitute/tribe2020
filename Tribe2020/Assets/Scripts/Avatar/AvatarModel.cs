using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SimpleJSON;

public class AvatarModel : MonoBehaviour {

	public int modelId;
	AvatarModels.AvatarModelBundle _modelBundle;
	public Color hairColor;
	public Color skinColor;
	public Color clothesColor1;
	public Color clothesColor2;
	public Color pantsColor;
	public Color shoeColor;

	// Use this for initialization
	void Awake() {
		AvatarManager am = FindObjectOfType<AvatarManager>();
		_modelBundle = am.models.models[modelId];
	}

	//
	void Start() {
	}

	// Update is called once per frame
	void Update() {

	}

	//
	public void InstantiateModel() {
		Transform existingModel = transform.FindChild("Model");
		AvatarManager am = FindObjectOfType<AvatarManager>();

		if(existingModel) {
			DestroyImmediate(existingModel.gameObject);
		}

		if(existingModel == null && (modelId >= 0 && modelId < am.models.models.Count)) {
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

		ExtractMaterialColors();
	}

	//
	public void ExtractMaterialColors() {
		Transform existingModel = transform.FindChild("Model");
		if(existingModel != null) {
			AvatarManager avatarMgr = FindObjectOfType<AvatarManager>();
			int modelId = GetComponent<AvatarModel>().modelId;
			_modelBundle = avatarMgr.models.models[modelId];

			//Load color from skin parts
			foreach(AvatarModels.BodyPartSkin bodyPartSkin in _modelBundle.bodyPartsSkin) {
				skinColor = transform.FindChild("Model/" + bodyPartSkin.model.name).
					GetComponent<SkinnedMeshRenderer>().sharedMaterial.color;
			}

			//Load various clothes colors from various body parts
			foreach(AvatarModels.BodyPartClothes bodyPart in _modelBundle.bodyPartsClothes) {
				switch(bodyPart.model.name) {
					case "Hair":
						hairColor = transform.FindChild("Model/" + bodyPart.model.name).
							GetComponent<SkinnedMeshRenderer>().sharedMaterial.color;
						break;
					case "Suit":
						clothesColor1 = transform.FindChild("Model/" + bodyPart.model.name).
							GetComponent<SkinnedMeshRenderer>().sharedMaterial.color;
						break;
					case "Pants":
						pantsColor = transform.FindChild("Model/" + bodyPart.model.name).
							GetComponent<SkinnedMeshRenderer>().sharedMaterial.color;
						break;
					case "Shoes":
						shoeColor = transform.FindChild("Model/" + bodyPart.model.name).
							GetComponent<SkinnedMeshRenderer>().sharedMaterial.color;
						break;
					default:
						clothesColor2 = transform.FindChild("Model/" + bodyPart.model.name).
							GetComponent<SkinnedMeshRenderer>().sharedMaterial.color;
						break;
				}
			}
		}
	}

	//
	private void SetLayerRecursive(Transform transform) {
		transform.gameObject.layer = LayerMask.NameToLayer("Avatars");
		foreach(Transform child in transform) {
			SetLayerRecursive(child);
		}
	}

	//
	public void RandomizeClothes() {

		Transform existingModel = transform.FindChild("Model");
		if(existingModel != null) {
			AvatarManager am = FindObjectOfType<AvatarManager>();
			int modelId = GetComponent<AvatarModel>().modelId;
			_modelBundle = am.models.models[modelId];

			Material materialSkin = _modelBundle.materialsSkin[Random.Range(0, _modelBundle.materialsSkin.Count)];
			foreach(AvatarModels.BodyPartSkin bodyPartSkin in _modelBundle.bodyPartsSkin) {
				SetClothesMaterial(bodyPartSkin.model.name, materialSkin);
			}

			foreach(AvatarModels.BodyPartClothes bodyPart in _modelBundle.bodyPartsClothes) {
				Material material = bodyPart.materials[Random.Range(0, bodyPart.materials.Count)];
				SetClothesMaterial(bodyPart.model.name, material);
			}
		}

		ExtractMaterialColors();
	}

	//
	public void SetClothesMaterial(string modelName, Material material) {
		transform.FindChild("Model/" + modelName).GetComponent<SkinnedMeshRenderer>().material = material;
	}

	//
	private Texture2D GetFaceTextureByMood(AvatarMood.Mood mood) {
		foreach(AvatarModels.FaceTexture faceTexture in _modelBundle.faceTextures) {
			if(faceTexture.mood == mood) {
				return faceTexture.texture;
			}
		}
		return null;
	}

	//
	public void SetFaceTexture(AvatarMood.Mood mood) {
		Texture2D textureFace = GetFaceTextureByMood(mood);
		if(textureFace != null) {
			transform.FindChild("Model/" + _modelBundle.faceObject.name).GetComponent<SkinnedMeshRenderer>().material.mainTexture = textureFace;
		}
	}

	//
	public void UpdateFaceTextureByCurrentMood() {
		AvatarMood.Mood mood = gameObject.GetComponent<AvatarMood>().GetCurrentMood();
		SetFaceTexture(mood);
	}

	//
	public void DeserializeFromJSON(JSONNode modelJSON) {
		AvatarManager avatarMgr = FindObjectOfType<AvatarManager>();
		modelId = modelJSON["modelID"].AsInt;
		_modelBundle = avatarMgr.models.models[modelId];

		//Remove prefab model
		Transform existingModel = transform.FindChild("Model");
		if(existingModel) {
			DestroyImmediate(existingModel.gameObject);
		}

		//Instantiate and set model hierarchy
		GameObject goModel = Instantiate(_modelBundle.model);
		goModel.name = "Model";
		SetLayerRecursive(goModel.transform);
		goModel.transform.parent = transform;
		goModel.transform.localPosition = Vector3.zero;

		//Set animator rig
		Animator animator = gameObject.GetComponent<Animator>();
		animator.avatar = _modelBundle.avatar;

		//Set skin material
		Material materialSkin = _modelBundle.materialsSkin[modelJSON["skinMaterial"].AsInt];
		foreach(AvatarModels.BodyPartSkin bodyPartSkin in _modelBundle.bodyPartsSkin) {
			SetClothesMaterial(bodyPartSkin.model.name, materialSkin);
		}

		//Set clothes materials
		for(int i = 0; i < _modelBundle.bodyPartsClothes.Count; i++) {
			Material material = _modelBundle.bodyPartsClothes[i].materials[modelJSON["clotheMaterials"][i].AsInt];
			SetClothesMaterial(_modelBundle.bodyPartsClothes[i].model.name, material);
		}

		ExtractMaterialColors();
	}

	//
	public JSONClass SerializeAsJSON() {
		AvatarManager avatarMgr = FindObjectOfType<AvatarManager>();
		AvatarModels avatarModels = avatarMgr.GetComponent<AvatarModels>();
		JSONClass json = new JSONClass();

		json.Add("modelID", "" + modelId);

		//Serialize skin material
		Material skinMat = transform.FindChild("Model/" + _modelBundle.bodyPartsSkin[0].model.name).
				GetComponent<SkinnedMeshRenderer>().sharedMaterial;
		json.Add("skinMaterial", avatarModels.models[modelId].materialsSkin.IndexOf(skinMat) + "");

		//Serialize clothes materials
		JSONArray clotheMaterials = new JSONArray();
		for(int i = 0; i < _modelBundle.bodyPartsClothes.Count; i++) {
			string modelName = _modelBundle.bodyPartsClothes[i].model.name;
			int matIndex = _modelBundle.bodyPartsClothes[i].materials.IndexOf(
				transform.FindChild("Model/" + modelName).GetComponent<SkinnedMeshRenderer>().sharedMaterial);
			clotheMaterials.Add("" + matIndex);
		}
		json.Add("clotheMaterials", clotheMaterials);

		return json;
	}
}
