using UnityEngine;
using System.Collections;
using System;

public class Gems : MonoBehaviour {

    public GameObject satisfactionGem;

    private static Gems _instance;
    public static Gems GetInstance() {
        return _instance;
    }

    void Awake() {
        _instance = this;
    }

    // Use this for initialization
    void Start () {
        
    }
	
	// Update is called once per frame
	void Update () {
	
	}

    public void Instantiate(GameObject gemPrefab, Vector3 position, Action<Gem> callback = null, int value = 1, float scaleFactor = 0.0f) {

        GameObject closeGem = FindCloseGem(position);
        if (closeGem != null) {
            closeGem.GetComponent<Gem>().AddValue(value);
        }
        else {
            GameObject gem = (GameObject)Instantiate(gemPrefab, position, new Quaternion(), transform);
            gem.GetComponent<Gem>().SetOnTapCallback(ResourceManager.GetInstance().AddComfort);
            gem.GetComponent<Gem>().SetValue(value);
            gem.GetComponent<Gem>().SetScaleFactor(scaleFactor);
        }
    }

    GameObject FindCloseGem(Vector3 position) {

        float minDist = 1.0f;
        GameObject minObject = null;
        foreach(Transform child in transform) {
            float dist = Vector2.Distance(new Vector2(position.x, position.z), new Vector2(child.position.x, child.position.z));
            if (dist < minDist) {
                minDist = dist;
                minObject = child.gameObject;
            }
        }

        return minObject;

    }


}
