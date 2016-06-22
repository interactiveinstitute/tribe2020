using UnityEngine;
using System.Collections;

public class View : MonoBehaviour {
	//Singleton features
	protected static View _instance;
	public static View GetInstance() {
		return _instance;
	}

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	//
	public virtual void ShowMessage(string message, bool showAtBottom, bool showOkButton = true) {
	}

	//
	public virtual void ControlInterface(string id, string action) {
	}

	//
	public virtual void ShowCongratualations(string text) {
	}

	//
	public virtual void ClearView() {
	}
}
