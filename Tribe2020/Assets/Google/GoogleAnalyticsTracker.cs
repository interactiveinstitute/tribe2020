using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoogleAnalyticsTracker : MonoBehaviour {

    public GoogleAnalyticsV4 googleAnalytics;
    public string screenName;

    // Use this for initialization
    void Start () {
        googleAnalytics.StartSession();
        googleAnalytics.LogScreen(screenName);
        googleAnalytics.DispatchHits();
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    private void OnDestroy() {
        googleAnalytics.Dispose();
    }
}
