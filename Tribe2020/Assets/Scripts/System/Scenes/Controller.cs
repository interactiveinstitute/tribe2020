using UnityEngine;
using System.Collections;

public class Controller : MonoBehaviour {
	//Singleton features
	protected static Controller _instance;
	public static Controller GetInstance() {
		return _instance;
	}

	//
	public enum InputState {
		ALL, ONLY_PROMPT, ONLY_SWIPE, ONLY_TAP, ONLY_APPLIANCE_SELECT, ONLY_APPLIANCE_DESELECT,
		ONLY_OPEN_INBOX, ONLY_CLOSE_INBOX, ONLY_ENERGY, ONLY_COMFORT, ONLY_SWITCH_LIGHT, ONLY_APPLY_EEM, ONLY_HARVEST, NOTHING,
		ONLY_CLOSE_MAIL, ONLY_SELECT_OVERVIEW, ONLY_SELECT_GRIDVIEW
	};

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	//
	//public virtual void ShowMessage(string key, string message, bool showButton) {
	//}

	//
	public virtual void ClearView() {
	}

	//
	public virtual void ControlInterface(string id, string action) {
	}

	//
	public virtual void OnNewViewpoint(string title, int viewCount, int aboveCount, int curView) {
	}

	//
	public virtual void OnNewViewpoint(Viewpoint curView, Viewpoint[][] viewMatrix, bool overview) {
	}

	//
	public virtual void ShowCongratualations(string text) {
	}

	//
	public virtual void ControlAvatar(string id, string action, Vector3 pos) {
	}

	//
	public virtual void ControlAvatar(string id, Object action) {
	}

	//
	public virtual void PlaySound(string sound) {
	}

	//
	public virtual void SetTimeScale(int scale) {
	}

	//
	public virtual string GetCurrentDate() {
		return "";
	}

	//
	public virtual void SetControlState(InputState state) {
	}

	//
	public virtual void UnlockView(int x, int y) { }

	//
	public virtual void SaveGameState() {
	}

	//
	public virtual void LoadGameState() {
	}
}
