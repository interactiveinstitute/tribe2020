using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class AvatarMood : MonoBehaviour {

    //Singleton features
    private static AvatarMood _instance;
    public static AvatarMood GetInstance() {
        return _instance;
    }

    public List<Mood> _faceTextureIndices;
    public List<Texture2D> _faceTextures;

    public enum Mood { angry, sad, tired, neutral_neg, neutral_pos, surprised, happy, euphoric };

    //Sort use instead of constructor
    void Awake() {
        _instance = this;
    }

    // Use this for initialization
    void Start () {
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public Texture2D GetFaceTexture(Mood mood) {
        int i = _faceTextureIndices.IndexOf(mood);
        return i != -1 ? _faceTextures[i] : null;
    }
}
