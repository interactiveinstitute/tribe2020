using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityStandardAssets.Characters.ThirdPerson;


public class AvatarMood : MonoBehaviour {

	public enum Mood { angry, sad, tired, neutral_neg, neutral_pos, surprised, happy, euphoric };

	//Singletons
	GameTime _timeMgr;
	AvatarManager _avatarMood;
	ResourceManager _resourceMgr;
	Gems _gems;

	//Dirty flag - for rendering character panel
	bool _isUpdated;

	//Components
	private ThirdPersonCharacter _charController;

	public float responsivenessMood;

	double _timeLastHappinessRelease;
	double _timeLastMoodChange;

	private Affordance currentInteractionAffordance = null;

	public Mood preferedMood;
	Markov<Mood> markovMood = new Markov<Mood>();
	Markov<AvatarConversation.EnvironmentLevel> markovEnvironmentLevel = new Markov<AvatarConversation.EnvironmentLevel>();

	//
	void Awake() {
		//Markov
		List<Mood> markovStates = new List<Mood>();
		markovStates.Add(Mood.angry);
		markovStates.Add(Mood.sad);
		//markovStates.Add(Mood.tired);
		markovStates.Add(Mood.neutral_neg);
		markovStates.Add(Mood.neutral_pos);
		markovStates.Add(Mood.surprised);
		markovStates.Add(Mood.happy);
		markovStates.Add(Mood.euphoric);
		markovMood.InsertStates(markovStates);

		float standardDeviation = 0.25f;
		markovMood.SetProbability(Mood.angry, ProbabilityFunctions.gaussian, new float[2] { markovMood.GetT(Mood.angry), standardDeviation }, true);
		markovMood.SetProbability(Mood.sad, ProbabilityFunctions.gaussian, new float[2] { markovMood.GetT(Mood.sad), standardDeviation }, true);
		//markovMood.SetProbability(Mood.tired, ProbabilityFunctions.gaussian, new float[2] { markovMood.GetT(Mood.tired), standardDeviation }, true);
		markovMood.SetProbability(Mood.neutral_neg, ProbabilityFunctions.gaussian, new float[2] { markovMood.GetT(Mood.neutral_neg), standardDeviation }, true);
		markovMood.SetProbability(Mood.neutral_pos, ProbabilityFunctions.gaussian, new float[2] { markovMood.GetT(Mood.neutral_pos), standardDeviation }, true);
		markovMood.SetProbability(Mood.surprised, ProbabilityFunctions.gaussian, new float[2] { markovMood.GetT(Mood.surprised), standardDeviation }, true);
		markovMood.SetProbability(Mood.happy, ProbabilityFunctions.gaussian, new float[2] { markovMood.GetT(Mood.happy), standardDeviation }, true);
		markovMood.SetProbability(Mood.euphoric, ProbabilityFunctions.gaussian, new float[2] { markovMood.GetT(Mood.euphoric), standardDeviation }, true);

		//markovMood.LogProbabilities();

		markovMood.SetCurrentState(preferedMood);
		//UpdateFaceTextureByCurrentMood();
	}

	//Constructor
	void Start() {
		_timeMgr = GameTime.GetInstance();
		_gems = Gems.GetInstance();
		_resourceMgr = ResourceManager.GetInstance();

		_charController = GetComponent<ThirdPersonCharacter>();

		
	}

	void Update() {
		TryReleaseSatisfactionGem(_timeMgr.time);
		TryResetMood(_timeMgr.time);
	}

	void SetUpdated() {
		_isUpdated = true;
	}

	public bool IsUpdated() {
		bool returnValue = _isUpdated;
		_isUpdated = false;
		return returnValue;
	}

	public Mood TryChangeMood(Mood moodInput) {
		if(_timeMgr){
			Mood moodNew = markovMood.SetToNextState(
				new Mood[] { markovMood.GetCurrentState(), moodInput }, new float[] { 1.0f - responsivenessMood, responsivenessMood });
			_timeLastMoodChange = _timeMgr.time;
			UpdateLooksByCurrentMood();
			SetUpdated();
			return moodNew;
		}
		return Mood.neutral_pos;
	}

	public void SetMood(Mood mood) {
		markovMood.SetCurrentState(mood);
		_timeLastMoodChange = _timeMgr.time;
		UpdateLooksByCurrentMood();
		SetUpdated();
	}

	public Mood GetCurrentMood() {
		return markovMood.GetCurrentState();
	}

	public void TryResetMood(double time) {
		Mood mood = GetCurrentMood();
		if(mood != preferedMood) {
			if(time > _timeLastMoodChange + _resourceMgr.moodResetTime) {
				SetMood(preferedMood);
			}
		}
	}

	public void StartNewInteraction(Affordance affordance) {
		currentInteractionAffordance = affordance;
		markovMood.Restart();
	}

	public void EndInteraction() {
		currentInteractionAffordance = null;
		markovMood.End();
	}

	public bool IsInteracting() {
		return currentInteractionAffordance != null;
		//return markovMood.IsActive();
	}

	public Affordance GetCurrentInteractionAffordance() {
		return currentInteractionAffordance;
	}

	public void TryReleaseSatisfactionGem(double time) {
		Mood mood = GetCurrentMood();
		if(mood == Mood.happy || mood == Mood.euphoric) {

			double timeBetweenReleases = 10.0f;
			switch(mood) {
				case Mood.happy:
					timeBetweenReleases = _resourceMgr.happyComfortInterval;
					break;
				case Mood.euphoric:
					timeBetweenReleases = _resourceMgr.euphoricComfrotInterval;
					break;
			}

			if(time > _timeLastHappinessRelease + timeBetweenReleases && _resourceMgr.comfortHarvestCount < _resourceMgr.comfortHarvestMax) {
				_timeLastHappinessRelease = time;
				ReleaseSatisfactionGem();
			}

		}
	}

	public void ReleaseSatisfactionGem() {
		if(_gems) {
			_gems.Instantiate(_gems.satisfactionGem, transform.position + 2.1f * Vector3.up, ResourceManager.GetInstance().AddComfort, 1, 0.1f);
			_resourceMgr.comfortHarvestCount++;
		}
	}

	void UpdateLooksByCurrentMood() {
		AvatarModel avatarModel = gameObject.GetComponent<AvatarModel>();
		if(avatarModel != null) {
			avatarModel.UpdateFaceTextureByCurrentMood();
		}
		_charController.SetMood((int)GetCurrentMood());
	}

}
