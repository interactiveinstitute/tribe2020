using UnityEngine;
using System.Collections;

public class Billboard : MonoBehaviour {
	public bool isActive = true;

    [Header("Constant scale")]
    public bool constantScale;
    public float scale;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		if(isActive)
			transform.rotation = Quaternion.LookRotation(Camera.main.transform.forward);

        if (constantScale) {
            float distance = Vector3.Distance(Camera.main.transform.position, transform.position);
            GetComponentInChildren<RectTransform>().localScale = scale * distance * Vector3.one;
        }
    }
}
