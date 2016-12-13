using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class AvatarAttitude {

    public enum Mood { angry, sad, tired, neutral_neg, neutral_pos, surprised, happy, euphoric };

    //public float food;
    //public float transport;
    public float engagementEnvironment;
    public float responsivenessEnvironment;
    public float responsivenessMood;

    private Affordance currentInteractionAffordance = null;

    public Mood preferedMood;
    Markov<Mood> markovMood = new Markov<Mood>();
    Markov<AvatarConversation.EnvironmentLevel> markovEnvironmentLevel = new Markov<AvatarConversation.EnvironmentLevel>();

    //Constructor
    public void Init()
    {
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

        markovMood.LogProbabilities();

        markovMood.SetCurrentState(preferedMood);

        //markovMood.Restart(preferedMood);
    }

    public Mood TryChangeMood(Mood moodInput)
    {
        return markovMood.SetToNextState(new Mood[] { markovMood.GetCurrentState(), moodInput }, new float[]{ 1.0f - responsivenessMood, responsivenessMood});
    }

    public Mood GetCurrentMood()
    {
        return markovMood.GetCurrentState();
    }

    public void StartNewInteraction(Affordance affordance)
    {
        currentInteractionAffordance = affordance;
        markovMood.Restart();
    }

    public void EndInteraction()
    {
        currentInteractionAffordance = null;
        markovMood.End();
    }

    public bool IsInteracting()
    {
        return currentInteractionAffordance != null;
        //return markovMood.IsActive();
    }

    public Affordance GetCurrentInteractionAffordance()
    {
        return currentInteractionAffordance;
    }

}

