using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class AvatarAttitude : MonoBehaviour {

    //Singletons
    GameTime _timeMgr;
    AvatarMood _avatarMood;
    ResourceManager _resourceMgr;
    Gems _gems;

    //public float food;
    //public float transport;
    public float engagementEnvironment;
    public float responsivenessEnvironment;
    public float responsivenessMood;

    double _timeLastHappinessRelease;

    private Affordance currentInteractionAffordance = null;

    public AvatarMood.Mood preferedMood;
    Markov<AvatarMood.Mood> markovMood = new Markov<AvatarMood.Mood>();
    Markov<AvatarConversation.EnvironmentLevel> markovEnvironmentLevel = new Markov<AvatarConversation.EnvironmentLevel>();

    public Texture2D _textureFace;

    //Constructor
    void Start() {
        _timeMgr = GameTime.GetInstance();
        _gems = Gems.GetInstance();
		_resourceMgr = ResourceManager.GetInstance();
        _avatarMood = AvatarMood.GetInstance();

        List<AvatarMood.Mood> markovStates = new List<AvatarMood.Mood>();
        markovStates.Add(AvatarMood.Mood.angry);
        markovStates.Add(AvatarMood.Mood.sad);
        //markovStates.Add(Mood.tired);
        markovStates.Add(AvatarMood.Mood.neutral_neg);
        markovStates.Add(AvatarMood.Mood.neutral_pos);
        markovStates.Add(AvatarMood.Mood.surprised);
        markovStates.Add(AvatarMood.Mood.happy);
        markovStates.Add(AvatarMood.Mood.euphoric);
        markovMood.InsertStates(markovStates);

        float standardDeviation = 0.25f;
        markovMood.SetProbability(AvatarMood.Mood.angry, ProbabilityFunctions.gaussian, new float[2] { markovMood.GetT(AvatarMood.Mood.angry), standardDeviation }, true);
        markovMood.SetProbability(AvatarMood.Mood.sad, ProbabilityFunctions.gaussian, new float[2] { markovMood.GetT(AvatarMood.Mood.sad), standardDeviation }, true);
        //markovMood.SetProbability(Mood.tired, ProbabilityFunctions.gaussian, new float[2] { markovMood.GetT(Mood.tired), standardDeviation }, true);
        markovMood.SetProbability(AvatarMood.Mood.neutral_neg, ProbabilityFunctions.gaussian, new float[2] { markovMood.GetT(AvatarMood.Mood.neutral_neg), standardDeviation }, true);
        markovMood.SetProbability(AvatarMood.Mood.neutral_pos, ProbabilityFunctions.gaussian, new float[2] { markovMood.GetT(AvatarMood.Mood.neutral_pos), standardDeviation }, true);
        markovMood.SetProbability(AvatarMood.Mood.surprised, ProbabilityFunctions.gaussian, new float[2] { markovMood.GetT(AvatarMood.Mood.surprised), standardDeviation }, true);
        markovMood.SetProbability(AvatarMood.Mood.happy, ProbabilityFunctions.gaussian, new float[2] { markovMood.GetT(AvatarMood.Mood.happy), standardDeviation }, true);
        markovMood.SetProbability(AvatarMood.Mood.euphoric, ProbabilityFunctions.gaussian, new float[2] { markovMood.GetT(AvatarMood.Mood.euphoric), standardDeviation }, true);

        //markovMood.LogProbabilities();

        markovMood.SetCurrentState(preferedMood);

        UpdateFaceTexture();

    }

    void Update() {
        TryReleaseSatisfactionGem(_timeMgr.time);
    }

    public AvatarMood.Mood TryChangeMood(AvatarMood.Mood moodInput) {
        AvatarMood.Mood moodNew = markovMood.SetToNextState(new AvatarMood.Mood[] { markovMood.GetCurrentState(), moodInput }, new float[]{ 1.0f - responsivenessMood, responsivenessMood});
        UpdateFaceTexture();
        return moodNew;
    }

    public void SetMood(AvatarMood.Mood mood) {
        markovMood.SetCurrentState(mood);
        UpdateFaceTexture();
    }

    public AvatarMood.Mood GetCurrentMood() {
        return markovMood.GetCurrentState();
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
        AvatarMood.Mood mood = GetCurrentMood();
        if (mood == AvatarMood.Mood.happy || mood == AvatarMood.Mood.euphoric) {

            double timeBetweenReleases = 10.0f;
            switch (mood){
                case AvatarMood.Mood.happy:
                    timeBetweenReleases = _resourceMgr.happyComfortInterval;
                break;
                case AvatarMood.Mood.euphoric:
                    timeBetweenReleases = _resourceMgr.euphoricComfrotInterval;
                break;
            }

            if (time > _timeLastHappinessRelease + timeBetweenReleases && _resourceMgr.comfortHarvestCount < _resourceMgr.comfortHarvestMax) {
                _timeLastHappinessRelease = time;
                ReleaseSatisfactionGem();
            }

        }   
    }

    public void ReleaseSatisfactionGem() {
        _gems.Instantiate(_gems.satisfactionGem, transform.position + 2.5f * Vector3.up, ResourceManager.GetInstance().AddComfort, 1, 0.1f);
		_resourceMgr.comfortHarvestCount++;
    }

    void UpdateFaceTexture() {
        _textureFace = _avatarMood.GetFaceTexture(markovMood.GetCurrentState());
        SkinnedMeshRenderer renderer = transform.FindChild("Model/Face").GetComponent<SkinnedMeshRenderer>();
        if (_textureFace != null && renderer != null) {
            renderer.materials[0].mainTexture = _textureFace;
        }
    }

}

