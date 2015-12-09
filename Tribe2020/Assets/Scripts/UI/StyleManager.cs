using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class StyleManager : MonoBehaviour {

	// Use this for initialization
	void Start(){
		SetStyle();
	}
	
	// Update is called once per frame
	void Update(){
	}

	public void SetStyle(){
//		Debug.Log("text count" + gameObject.GetComponents<Text>().Length);
		Text[] ts = Object.FindObjectsOfType(typeof(Text)) as Text[];
		foreach(Text t in ts){
			t.fontSize = 16;
		}
	}
}
