using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Acopalypsemeter : MonoBehaviour {

    public GameObject Percent, Pointer;
    ResourceManager Resources;

    Text percent_text;

 

       
       


    // Use this for initialization
    void Start () {
        Resources = ResourceManager.GetInstance();
        percent_text = Percent.GetComponent<Text>();

    }
	
	// Update is called once per frame
	void Update () {
        percent_text.text = (Resources.CO2Change * 100).ToString("N0") + "%";

    }
}
