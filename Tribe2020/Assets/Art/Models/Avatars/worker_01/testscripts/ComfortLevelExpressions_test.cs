using UnityEngine;
using System.Collections;


namespace UnityStandardAssets.Characters.ThirdPerson
{
public class ComfortLevelExpressions_test : MonoBehaviour {



	public GameObject face;
	public Texture[] myTextures = new Texture[5];
	int maxTextures;
	int arrayPos = 0;
	private ThirdPersonCharacter_test comfort;

	void Start ()
	{
		maxTextures = myTextures.Length-1;
			comfort = GetComponent<ThirdPersonCharacter_test>();

	}


	void Update ()
	{
		ChangeFaceTexture ();
	}
		
	void ChangeFaceTexture(){
			if (comfort.comfortlevel_amount > 0.2f) {
			face.GetComponent<Renderer> ().material.mainTexture = myTextures [0];
			}

			if ((comfort.comfortlevel_amount <= 0.2f) && (comfort.comfortlevel_amount >= -0.2f)) {
			face.GetComponent<Renderer> ().material.mainTexture = myTextures [1];
		}

			if ((comfort.comfortlevel_amount <= -0.2f) && (comfort.comfortlevel_amount >= -0.6f)) {
			face.GetComponent<Renderer> ().material.mainTexture = myTextures [2];
		}

			if (comfort.comfortlevel_amount <= -0.6f) {
			face.GetComponent<Renderer> ().material.mainTexture = myTextures [3];
		}

		} 

		
}
}