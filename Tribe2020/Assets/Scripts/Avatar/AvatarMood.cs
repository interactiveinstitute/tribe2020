﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class AvatarMood : MonoBehaviour {

    //Singleton features
    /*private static AvatarMood _instance;
    public static AvatarMood GetInstance() {
        return _instance;
    }*/

    public enum Mood { angry, sad, tired, neutral_neg, neutral_pos, surprised, happy, euphoric };

    //Singletons
    GameTime _timeMgr;
    AvatarMood _avatarMood;
    ResourceManager _resourceMgr;
    Gems _gems;

    public float responsivenessMood;

    double _timeLastHappinessRelease;

    private Affordance currentInteractionAffordance = null;

    public AvatarMood.Mood preferedMood;
    Markov<AvatarMood.Mood> markovMood = new Markov<AvatarMood.Mood>();
    Markov<AvatarConversation.EnvironmentLevel> markovEnvironmentLevel = new Markov<AvatarConversation.EnvironmentLevel>();

    //Sort use instead of constructor
    /*void Awake() {
        _instance = this;
    }*/

    //Constructor
    void Start() {
        _timeMgr = GameTime.GetInstance();
        _gems = Gems.GetInstance();
        _resourceMgr = ResourceManager.GetInstance();

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
    }

    void Update() {
        TryReleaseSatisfactionGem(_timeMgr.time);
    }

    public AvatarMood.Mood TryChangeMood(AvatarMood.Mood moodInput) {
        AvatarMood.Mood moodNew = markovMood.SetToNextState(new AvatarMood.Mood[] { markovMood.GetCurrentState(), moodInput }, new float[] { 1.0f - responsivenessMood, responsivenessMood });
        UpdateFaceTextureByCurrentMood();
        return moodNew;
    }

    public void SetMood(AvatarMood.Mood mood) {
        markovMood.SetCurrentState(mood);
        UpdateFaceTextureByCurrentMood();
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
            switch (mood) {
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

    void UpdateFaceTextureByCurrentMood() {
        gameObject.GetComponent<ComfortLevelExpressions>().UpdateFaceTextureByCurrentMood();
    }
}
