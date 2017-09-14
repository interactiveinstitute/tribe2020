using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AcopalypseBargraph : MonoBehaviour {
	Text [] newText ;

	// Use this for initialization
	void Start () {

		newText = GetComponentsInChildren<Text> ();
		newText [0].text = "Wenesday";
		newText [1].text = "Thursday";
		newText [2].text = "Friday";
		newText [3].text = "Saturday";
		newText [4].text = "Sunday";
		newText [5].text = "Yesterday";
		newText [6].text = "Today";

	}
	
	// Update is called once per frame
	void Update () {
		
		
	}
}
