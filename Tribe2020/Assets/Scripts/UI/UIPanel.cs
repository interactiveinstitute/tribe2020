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

}
