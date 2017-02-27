using UnityEngine;
using System.Collections;

public class DataText : DataNode {

	public TextMesh textMesh;
	//public string Subproperty = null;
	public int SubpropertyId = 0;


	// Use this for initialization
	void Start () {
		GameObject parentObject;
		parentObject = this.transform.root.gameObject;
        if (textMesh == null)
		    textMesh = parentObject.GetComponent<TextMesh>();
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	//override public void JsonUpdate(Subscription Sub, JSONObject json) {

	//	if (Subproperty == null)
	//		textMesh.text = json.str;
	//	else {
	//		textMesh.text = json.GetField (Subproperty).str;
	//	}
	//}

	override public void TimeDataUpdate(Subscription Sub, DataPoint data) {

		if (data == null)
			return;

		if (data.Values == null)
			return;
		
		if (SubpropertyId >= data.Values.Length)
			return; 

		//Debug.Log (data.Values.Length);
		//Debug.Log (SubpropertyId);
		if (data.Values [SubpropertyId] != null) {
  
            textMesh.text = data.Values[SubpropertyId].ToString();
			return;
		}

		if (data.Texts [SubpropertyId] != null) {
			textMesh.text = data.Texts [SubpropertyId];
		}

	}





}
