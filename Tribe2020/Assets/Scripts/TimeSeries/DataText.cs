using UnityEngine;
using System.Collections;

public class DataText : TimeDataObject {

	public TextMesh textMesh = null;
	public string Subproperty = null;
	public int SubpropertyId = 0;


	// Use this for initialization
	void Start () {
		GameObject parentObject;
		parentObject = this.transform.root.gameObject;
		textMesh = parentObject.GetComponent<TextMesh>();
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	override public void JsonUpdate(Connection Sub, JSONObject json) {

		if (Subproperty == null)
			textMesh.text = json.str;
		else {
			textMesh.text = json.GetField (Subproperty).str;
		}
	}

	override public void TimeDataUpdate(Connection Sub,DataPoint data) {

		if (SubpropertyId > data.Values.Length)
			return; 

		if (data.Values [SubpropertyId] != null) {
			textMesh.text = data.Values [SubpropertyId].ToString ();
			return;
		}

		if (data.Texts [SubpropertyId] != null) {
			textMesh.text = data.Texts [SubpropertyId];
		}

	}





}
