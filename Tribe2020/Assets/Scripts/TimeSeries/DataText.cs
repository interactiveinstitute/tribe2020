using UnityEngine;
using System.Collections;

public class DataText : Subscriber {

	TextMesh textMesh = null;



	// Use this for initialization
	void Start () {
		GameObject parentObject;
		parentObject = this.transform.root.gameObject;
		textMesh = parentObject.GetComponent<TextMesh>();
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	virtual public void Data_Update(string payload) {

		if (Subproperty == null)
			textMesh.text = payload;
		else {
			
		}
	}

}
