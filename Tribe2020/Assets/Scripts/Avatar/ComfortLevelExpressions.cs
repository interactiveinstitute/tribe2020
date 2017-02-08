using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ComfortLevelExpressions : MonoBehaviour {
	public GameObject face;

    public List<AvatarMood.Mood> _faceTextureIndices;
    public List<Texture2D> _faceTextures;
    Texture2D _textureFace;


	void Start ()
	{
        //face = transform.FindChild("Model/Face").gameObject;
        //UpdateFaceTextureByCurrentMood();
    }


    void Update ()
	{
	}

    Texture2D GetFaceTextureByMood(AvatarMood.Mood mood) {
        int i = _faceTextureIndices.IndexOf(mood);
        return i != -1 ? _faceTextures[i] : null;
    }

    public void SetFaceTexture(AvatarMood.Mood mood) {
        _textureFace = GetFaceTextureByMood(mood);
        if (_textureFace != null) {
            //face.GetComponent<SkinnedMeshRenderer>().material.mainTexture = _textureFace;
        }
    }

    public void UpdateFaceTextureByCurrentMood() {
        AvatarMood.Mood mood = gameObject.GetComponent<AvatarMood>().GetCurrentMood();
        SetFaceTexture(mood);
    }

}
