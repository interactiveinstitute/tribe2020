using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class NarrativeInteractionPoint : MonoBehaviour, IPointerClickHandler, IPointerDownHandler {

    NarrationManager _narrationMgr;
    public Appliance app;

    // Use this for initialization
    void Start () {
        _narrationMgr = NarrationManager.GetInstance();
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    //
    public void OnPointerClick(PointerEventData eventData) {

        //Pass on event to appliance
        app.OnPointerClick(eventData);

    }

    public void OnPointerDown(PointerEventData eventData) {
    }

}
