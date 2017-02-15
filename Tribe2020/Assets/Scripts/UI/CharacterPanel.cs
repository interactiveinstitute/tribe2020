using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CharacterPanel : MonoBehaviour {

    PilotView _pilotView;
    AvatarManager _avatarManager;

	// Use this for initialization
	void Start () {
        _avatarManager = AvatarManager.GetInstance();
        _pilotView = PilotView.GetInstance();
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void BuildPanel(GameObject goAvatar) {
        SetMood(goAvatar.GetComponent<AvatarMood>().GetCurrentMood());
        SetEnergyEffeciency(goAvatar.GetComponent<AvatarStats>().GetEnergyEfficiency());
        SetKnowledge(goAvatar.GetComponent<AvatarStats>().knowledge);
        SetAttitude(goAvatar.GetComponent<AvatarStats>().attitude);
        SetNormSensitivity(goAvatar.GetComponent<AvatarStats>().normSensititvity);
    }

    void SetMood(AvatarMood.Mood mood) {
        _pilotView.avatarMood.GetComponent<Image>().sprite = _avatarManager.conversation.GetEmojiReaction(mood);
    }

    public void SetEnergyEffeciency(float value) {
        int nLabels = _pilotView.EELabels.Count;
        int index = Mathf.Min(Mathf.FloorToInt(value * nLabels), nLabels - 1);
        _pilotView.avatarEfficiencyLabel.GetComponent<Image>().sprite = _pilotView.EELabels[index];
    }

    public void SetSatisfaction(float value) {
        _pilotView.avatarSatisfaction.value = value * 100;
    }

    public void SetKnowledge(float value) {
        _pilotView.avatarKnowledge.value = value * 100;
    }

    public void SetAttitude(float value) {
        _pilotView.avatarAttitude.value = value * 100;
    }

    public void SetNormSensitivity(float value) {
        _pilotView.avatarNorm.value = value * 100;
    }
}
