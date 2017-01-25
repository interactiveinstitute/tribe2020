using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestController : MonoBehaviour {
	public Narrative theNarrative;

	public enum Limitation {
		EMPTY, OKPressed, Swiped, Tapped, ApplianceSelected, ApplianceDeselected, QuestListOpened,
		QuestListClosed, QuestOpened, MeasurePerformed, FindView, AvatarArrived, AvatarSessionOver, AvatarActivityOver,
		ResourceHarvested, BattleOver, InspectorOpened, InspectorClosed, InboxOpened, InboxClosed, MailOpened, MailClosed,
		OpenEnergyPanel, CloseEnergyPanel, OpenComfortPanel, CloseComfortPanel, LightSwitchedOff, LightSwitchedOn, AvatarSelected,
		SelectedOverview, SelectedGridView, CameraMoveOver, CameraAnimationEvent
	}

	public int updateCount = 0;

	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update () {
		updateCount++;
		if(updateCount == 2) {
			theNarrative.steps[0].unityEvent.Invoke();
		}
	}

	//
	public void PlaySound(string sound) {
		Debug.Log("PlaySound." + sound);
	}

	//
	public void PlayAnimation(string animation) {
		Debug.Log("PlayAnimation." + animation);
	}

	//
	public void ShowMessage(string message) {
		Debug.Log("ShowMessage." + message);
	}

	//
	public void LimitInteraction(string limitation) {
		Debug.Log(limitation.ToString());
	}
}
