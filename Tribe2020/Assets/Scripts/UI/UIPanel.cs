using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class UIPanel : MonoBehaviour {
	public string title;

	public Vector2 originalPosition;
	public Vector2 targetPosition;
	public RectTransform toggleButton;
	public PilotController.InputState relatedAction;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public void SetActive(bool state) {
        Graph[] graphs = GetComponentsInChildren<Graph>();
        foreach (Graph graph in graphs) {
            graph.active = state;
        }
    }

	//
	public void LerpTowardsTarget() {
		LerpTowards(targetPosition);
	}

	//
	public void LerpTowardsOrigin() {
		LerpTowards(originalPosition);
	}

	//
	public void LerpTowards(Vector2 target) {
		RectTransform rTransform = transform as RectTransform;

		if(Vector2.Distance(target, rTransform.anchoredPosition) > 0.1f) {
			Vector2 newPos = rTransform.anchoredPosition;
			newPos.x = Mathf.Lerp(newPos.x, target.x, 0.25f);
			newPos.y = Mathf.Lerp(newPos.y, target.y, 0.25f);
			rTransform.anchoredPosition = newPos;
		} else if(rTransform.anchoredPosition != target) {
			rTransform.anchoredPosition = target;
			if(title != "Menu" && title != "Viewpoints" && originalPosition == target) {
				gameObject.SetActive(false);
			}
		}
	}

}
