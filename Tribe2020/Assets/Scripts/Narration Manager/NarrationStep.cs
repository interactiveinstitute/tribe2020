using UnityEngine;
using System.Collections;

[System.Serializable]
public class NarrationStep {
	//Quest step types
	public enum StepType {
		Prompt, Popup, SendMail, PlayAnimation, PlaySound, ControlAvatar, StartAvatarActivity,
		ChangeTimeScale, QuestComplete, ControlInterface, Wait, UnlockView, PilotComplete,
		PilotFailed
	};

	//Quest events
	public enum StepCondition {
		EMPTY, OKPressed, Swiped, Tapped, ApplianceSelected, ApplianceDeselected, QuestListOpened,
		QuestListClosed, QuestOpened, MeasurePerformed, FindView, AvatarArrived, AvatarSessionOver, AvatarActivityOver,
		ResourceHarvested, BattleOver, InspectorOpened, InspectorClosed, InboxOpened, InboxClosed, MailOpened, MailClosed,
		OpenEnergyPanel, CloseEnergyPanel, OpenComfortPanel, CloseComfortPanel, LightSwitchedOff, LightSwitchedOn, AvatarSelected
	};

	

	public string title;
	public StepType type;
	public Controller.InputState inputState;

	[TextArea(2, 10)]
	public string valueField;
	
	public Object objectField;
	public bool showAtBottom;

	public StepCondition condition;
	public string conditionField;

	public bool EnableObject = false;
}
