using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Acopalypsemeter : MonoBehaviour {

    //public GameObject Percent, Pointer;
    ResourceManager Resources;

    public Text PercentText;
    public RectTransform Pointer;
    public double change;
    public double direction;





    // Use this for initialization
    void Start () {
        Resources = ResourceManager.GetInstance();
      
    }
	
	// Update is called once per frame
	void Update () {
        double change_capped = change;
        change = Resources.CO2Change -1;
        PercentText.text = (change  * 100).ToString("N0") + "%";

        //Capp values
        if (change_capped > 0.05)
            change_capped = 0.05;
        else if (change_capped < -0.57)
            change_capped = -0.57;

        //More than 20% savings
        if (change_capped < -0.2)
            direction = -1*(((change_capped + 0.5) / 0.3) * 90) - 90-180;
        else
            direction = -1* (((change_capped + 0.2) / 0.2) * 90);

        
        Pointer.localEulerAngles = (int)direction  * Vector3.forward;
    }
}
