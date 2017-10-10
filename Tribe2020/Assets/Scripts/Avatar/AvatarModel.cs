using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
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

	[Header("Portrait")]
	public Sprite hairImage;
	public Sprite backHairImage;
	public Sprite clothesImage;

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
		SetModel(modelId);
		ExtractMaterialColors();
	}

	//Set model for model index
	public void SetModel(int modelIndex) {
		Transform existingModel = transform.Find("Model");
		AvatarManager am = FindObjectOfType<AvatarManager>();

		if(existingModel) {
			DestroyImmediate(existingModel.gameObject);
		}

		if(existingModel == null && (modelIndex >= 0 && modelIndex < am.models.models.Count)) {
			_modelBundle = am.models.models[modelIndex];

			GameObject goModel = Instantiate(_modelBundle.model);
			goModel.name = "Model";
			SetLayerRecursive(goModel.transform);
			goModel.transform.parent = transform;
			goModel.transform.localPosition = Vector3.zero;
			goModel.transform.localRotation = Quaternion.identity;

			//Set animator rig
			Animator animator = gameObject.GetComponent<Animator>();
			animator.avatar = _modelBundle.avatar;
			animator.Rebind();
		}
	}

	//Set skin and clothing materials for material indexes
	public void SetMaterials(int skinMaterial, int[] clothingMaterials) {
		//Set skin material
		Material materialSkin = _modelBundle.materialsSkin[skinMaterial];
		foreach(AvatarModels.BodyPartSkin bodyPartSkin in _modelBundle.bodyPartsSkin) {
			SetClothesMaterial(bodyPartSkin.model.name, materialSkin);
		}

		//Set clothing materials
		for(int i = 0; i < _modelBundle.bodyPartsClothes.Count; i++) {
			Material material = _modelBundle.bodyPartsClothes[i].materials[clothingMaterials[i]];
			SetClothesMaterial(_modelBundle.bodyPartsClothes[i].model.name, material);
		}
	}

	//
	public void ExtractMaterialColors() {
		Transform existingModel = transform.Find("Model");
		if(existingModel != null) {
			AvatarManager avatarMgr = FindObjectOfType<AvatarManager>();
			int modelId = GetComponent<AvatarModel>().modelId;
			_modelBundle = avatarMgr.models.models[modelId];

			//Load color from skin parts
			foreach(AvatarModels.BodyPartSkin bodyPartSkin in _modelBundle.bodyPartsSkin) {
				skinColor = transform.Find("Model/" + bodyPartSkin.model.name).
					GetComponent<SkinnedMeshRenderer>().sharedMaterial.color;
			}

			//Load various clothes colors from various body parts
			foreach(AvatarModels.BodyPartClothes bodyPart in _modelBundle.bodyPartsClothes) {
				switch(bodyPart.model.name) {
					case "Hair":
						hairColor = transform.Find("Model/" + bodyPart.model.name).
							GetComponent<SkinnedMeshRenderer>().sharedMaterial.color;
						break;
					case "Suit":
						clothesColor1 = transform.Find("Model/" + bodyPart.model.name).
							GetComponent<SkinnedMeshRenderer>().sharedMaterial.color;
						break;
					case "Pants":
						pantsColor = transform.Find("Model/" + bodyPart.model.name).
							GetComponent<SkinnedMeshRenderer>().sharedMaterial.color;
						break;
					case "Shoes":
						shoeColor = transform.Find("Model/" + bodyPart.model.name).
							GetComponent<SkinnedMeshRenderer>().sharedMaterial.color;
						break;
					default:
						clothesColor2 = transform.Find("Model/" + bodyPart.model.name).
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

		Transform existingModel = transform.Find("Model");
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
		transform.Find("Model/" + modelName).GetComponent<SkinnedMeshRenderer>().material = material;
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
			transform.Find("Model/" + _modelBundle.faceObject.name).GetComponent<SkinnedMeshRenderer>().material.mainTexture = textureFace;
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

		SetModel(modelId);

		int skinMatIndex = modelJSON["skinMaterial"].AsInt;

		//Serialize portrait fragments
		int hairIndex = modelJSON["hairIndex"].AsInt;
		int backHairIndex = modelJSON["backHairIndex"].AsInt;
		int clothesIndex = modelJSON["clothesIndex"].AsInt;

		if(hairIndex >= 0) { hairImage = avatarMgr.hairs[hairIndex]; }
		if(backHairIndex >= 0) { backHairImage = avatarMgr.backHairs[backHairIndex]; }
		if(clothesIndex >= 0) { clothesImage = avatarMgr.clothes[clothesIndex]; }

		int[] clothingMatIndexes = new int[modelJSON["clotheMaterials"].Count];
		for(int i = 0; i < _modelBundle.bodyPartsClothes.Count; i++) {
			clothingMatIndexes[i] = modelJSON["clotheMaterials"][i].AsInt;
		}
		SetMaterials(skinMatIndex, clothingMatIndexes);

		ExtractMaterialColors();
	}

	//
	public JSONClass SerializeAsJSON() {
		AvatarManager avatarMgr = FindObjectOfType<AvatarManager>();
		AvatarModels avatarModels = avatarMgr.GetComponent<AvatarModels>();
		JSONClass json = new JSONClass();

		json.Add("modelID", "" + modelId);

		//Serialize skin material
		Material skinMat = transform.Find("Model/" + _modelBundle.bodyPartsSkin[0].model.name).
				GetComponent<SkinnedMeshRenderer>().sharedMaterial;
		json.Add("skinMaterial", avatarModels.models[modelId].materialsSkin.IndexOf(skinMat) + "");

		//Serialize portrait fragments
		json.Add("hairIndex", "" + avatarMgr.hairs.IndexOf(hairImage));
		json.Add("backHairIndex", "" + avatarMgr.backHairs.IndexOf(backHairImage));
		json.Add("clothesIndex", "" + avatarMgr.clothes.IndexOf(clothesImage));

		//Serialize clothes materials
		JSONArray clotheMaterials = new JSONArray();
		for(int i = 0; i < _modelBundle.bodyPartsClothes.Count; i++) {
			string modelName = _modelBundle.bodyPartsClothes[i].model.name;
			int matIndex = _modelBundle.bodyPartsClothes[i].materials.IndexOf(
				transform.Find("Model/" + modelName).GetComponent<SkinnedMeshRenderer>().sharedMaterial);
			clotheMaterials.Add("" + matIndex);
		}
		json.Add("clotheMaterials", clotheMaterials);

		return json;
	}
}
