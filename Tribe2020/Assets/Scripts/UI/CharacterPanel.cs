using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CharacterPanel : MonoBehaviour {
	private PilotView _pilotView;
	private AvatarManager _avatarManager;

	public Text avatarTitle;
	public Text avatarDescription;
	public Image avatarMood;
	public Text avatarTemperatureTitle;
	public Text avatarTemperature;
	public Text avatarEfficiencyTitle;
	public Image avatarEfficiency;
	public Image avatarEfficiencyLabel;
	public Text avatarSatisfactionTitle;
	public Slider avatarSatisfaction;
	public Text avatarKnowledgeTitle;
	public Slider avatarKnowledge;
	public Text avatarAttitudeTitle;
	public Slider avatarAttitude;
	public Text avatarNormTitle;
	public Slider avatarNorm;
	public Text avatarEEMTitle;
	public Transform avatarEEMContainer;

	public BehaviourAI currentAvatar = null;

	// Use this for initialization
	void Start () {
        _avatarManager = AvatarManager.GetInstance();
        _pilotView = PilotView.GetInstance();
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void BuildPanel(GameObject goAvatar) {

        currentAvatar = goAvatar.GetComponent<BehaviourAI>();

        SetMood(goAvatar.GetComponent<AvatarMood>().GetCurrentMood());
        SetEnergyEffeciency(goAvatar.GetComponent<AvatarStats>().GetEnergyEfficiency());
        SetKnowledge(goAvatar.GetComponent<AvatarStats>().knowledge);
        SetAttitude(goAvatar.GetComponent<AvatarStats>().attitude);
        SetNormSensitivity(goAvatar.GetComponent<AvatarStats>().normSensititvity);
    }

    public void OnClose() {
        currentAvatar = null;
    }

    void SetMood(AvatarMood.Mood mood) {
        _pilotView.avatarMood.GetComponent<Image>().sprite = _avatarManager.conversation.GetEmojiReaction(mood);
    }

    void SetEnergyEffeciency(float value) {
        int nLabels = _pilotView.EELabels.Count;
        int index = Mathf.Min(Mathf.FloorToInt((1.0f - value) * nLabels), nLabels - 1);
        _pilotView.avatarEfficiencyLabel.GetComponent<Image>().sprite = _pilotView.EELabels[index];
    }

    void SetSatisfaction(float value) {
        _pilotView.avatarSatisfaction.value = value * 100;
    }

    void SetKnowledge(float value) {
        _pilotView.avatarKnowledge.value = value * 100;
    }

    void SetAttitude(float value) {
        _pilotView.avatarAttitude.value = value * 100;
    }

    void SetNormSensitivity(float value) {
        _pilotView.avatarNorm.value = value * 100;
    }
}
