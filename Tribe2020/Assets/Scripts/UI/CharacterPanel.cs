using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CharacterPanel : MonoBehaviour {

    public GameObject moodImage;
    public GameObject satisfactionPanel;
    public GameObject knowledgePanel;
    public GameObject attitutePanel;
    public GameObject normSensitivityPanel;
    AvatarManager _avatarManager;

	// Use this for initialization
	void Start () {
        _avatarManager = AvatarManager.GetInstance();
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void BuildPanel(GameObject goAvatar) {
        SetMood(goAvatar.GetComponent<AvatarMood>().GetCurrentMood());
        SetKnowledge(goAvatar.GetComponent<AvatarStats>().knowledge);
        SetAttitude(goAvatar.GetComponent<AvatarStats>().attitude);
        SetNormSensitivity(goAvatar.GetComponent<AvatarStats>().normSensititvity);
    }

    void SetMood(AvatarMood.Mood mood) {
        moodImage.GetComponent<Image>().sprite = _avatarManager.conversation.GetEmojiReaction(mood);
    }

    public void SetSatisfaction(float value) {
        satisfactionPanel.GetComponentInChildren<Slider>().value = value * 100;
    }

    public void SetKnowledge(float value) {
        knowledgePanel.GetComponentInChildren<Slider>().value = value * 100;
    }

    public void SetAttitude(float value) {
        attitutePanel.GetComponentInChildren<Slider>().value = value * 100;
    }

    public void SetNormSensitivity(float value) {
        normSensitivityPanel.GetComponentInChildren<Slider>().value = value * 100;
    }
}
