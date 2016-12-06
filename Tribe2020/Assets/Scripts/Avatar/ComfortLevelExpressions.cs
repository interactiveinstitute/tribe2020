using UnityEngine;
using System.Collections;

public class ComfortLevelExpressions : MonoBehaviour {
	public GameObject face;
	public Texture[] myTextures = new Texture[5];
	int maxTextures;
	int arrayPos = 0;

	void Start ()
	{
		maxTextures = myTextures.Length-1;
	}


	void Update ()
	{
		ChangeFaceTexture ();
	}
		
	void ChangeFaceTexture(){
		if (Input.GetKeyDown (KeyCode.U)) {
			face.GetComponent<Renderer> ().material.mainTexture = myTextures [arrayPos];

			if (arrayPos == maxTextures) {
				arrayPos = 0;
			} else {
				arrayPos++;
			}

		} 
	}
		
}
