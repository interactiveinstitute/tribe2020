using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AvatarConversation : MonoBehaviour
{

    public enum EnvironmentLevel { negative, neutral, positive };

    [System.Serializable]
    public struct EmojiLine
    {
        public Sprite emojiTopic;
        public Sprite emojiReaction;
        public EnvironmentLevel environmentLevel;
        public AvatarAttitude.Mood mood;
    }

    /*
    public enum speechType { greeting, goodbye, goodbyeFinal, reaction, topic, question, answer};
    public Dictionary<speechType, List<string>> speeches = new Dictionary<speechType, List<string>>();
    public Dictionary<speechType, List<Texture>> emojis = new Dictionary<speechType, List<Texture>>();
    */

    //Markov<AvatarAttitude.Moods> markovMood = new Markov<AvatarAttitude.Moods>();
    //Markov<AvatarAttitude.EnvironmentLevel> markovEnvironmentLevel = new Markov<AvatarAttitude.EnvironmentLevel>();

    Dictionary<AvatarAttitude.Mood, List<Sprite>> emojisReactions = new Dictionary<AvatarAttitude.Mood, List<Sprite>>();
    public List<Sprite> emojisReactionsAngry = new List<Sprite>();
    public List<Sprite> emojisReactionsSad = new List<Sprite>();
    public List<Sprite> emojisReactionsTired = new List<Sprite>();
    public List<Sprite> emojisReactionsNeutralNegative = new List<Sprite>();
    public List<Sprite> emojisReactionsNeutralPositive = new List<Sprite>();
    public List<Sprite> emojisReactionsSurprised = new List<Sprite>();
    public List<Sprite> emojisReactionsHappy = new List<Sprite>();
    public List<Sprite> emojisReactionsEuphoric = new List<Sprite>();

    Dictionary<EnvironmentLevel, List<Sprite>> emojisTopics = new Dictionary<EnvironmentLevel, List<Sprite>>();
    public List<Sprite> emojisTopicsNegative = new List<Sprite>();
    public List<Sprite> emojisTopicsNeutral = new List<Sprite>();
    public List<Sprite> emojisTopicsPositive = new List<Sprite>();

    // Use this for initialization
    void Start()
    {
        /*
        markovMood.InsertState(AvatarAttitude.Moods.happy);
        markovMood.InsertState(AvatarAttitude.Moods.angry);
        markovMood.InsertState(AvatarAttitude.Moods.sad);
        markovMood.InsertState(AvatarAttitude.Moods.tired);
        markovMood.InsertState(AvatarAttitude.Moods.euphoric);
        markovMood.EvenProbability();
        markovMood.RestartRandom();

        markovEnvironmentLevel.InsertState(AvatarAttitude.EnvironmentLevel.negative);
        markovEnvironmentLevel.InsertState(AvatarAttitude.EnvironmentLevel.neutral);
        markovEnvironmentLevel.InsertState(AvatarAttitude.EnvironmentLevel.positive);
        markovEnvironmentLevel.EvenProbability();
        markovEnvironmentLevel.RestartRandom();
        */

        emojisReactions.Add(AvatarAttitude.Mood.angry, emojisReactionsAngry);
        emojisReactions.Add(AvatarAttitude.Mood.sad, emojisReactionsSad);
        emojisReactions.Add(AvatarAttitude.Mood.tired, emojisReactionsTired);
        emojisReactions.Add(AvatarAttitude.Mood.neutral_neg, emojisReactionsNeutralNegative);
        emojisReactions.Add(AvatarAttitude.Mood.neutral_pos, emojisReactionsNeutralPositive);
        emojisReactions.Add(AvatarAttitude.Mood.surprised, emojisReactionsSurprised);
        emojisReactions.Add(AvatarAttitude.Mood.happy, emojisReactionsHappy);
        emojisReactions.Add(AvatarAttitude.Mood.euphoric, emojisReactionsEuphoric);

        emojisTopics.Add(EnvironmentLevel.negative, emojisTopicsNegative);
        emojisTopics.Add(EnvironmentLevel.neutral, emojisTopicsNeutral);
        emojisTopics.Add(EnvironmentLevel.positive, emojisTopicsPositive);

        /*
        List<string> greetings = new List<string>();
        greetings.Add("Hi!");
        greetings.Add("Hello!");
        speeches.Add(speechType.greeting, greetings);

        List<string> goodbyes = new List<string>();
        goodbyes.Add("Nice talking to you, goodbye!");
        goodbyes.Add("Bye!");
        speeches.Add(speechType.goodbye, goodbyes);

        List<string> goodbyesFinal = new List<string>();
        goodbyesFinal.Add("Bye bye!");
        goodbyesFinal.Add("See you!");
        speeches.Add(speechType.goodbyeFinal, goodbyesFinal);

        List<string> reactions = new List<string>();
        reactions.Add("OK!");
        reactions.Add("Wow!");
        reactions.Add("I really like that!");
        reactions.Add("That is so cool!");
        speeches.Add(speechType.reaction, reactions);

        List<string> topics = new List<string>();
        topics.Add("I killed my hamster last night!");
        topics.Add("I am a walrus!");
        topics.Add("When I go home I will drink some blood!");
        speeches.Add(speechType.topic, topics);

        List<string> questions = new List<string>();
        questions.Add("Do you know what heaven looks like in the dark?");
        questions.Add("Is your dress black/blue or white/yellow?");
        questions.Add("Do you like me?");
        speeches.Add(speechType.question, questions);

        List<string> answers = new List<string>();
        answers.Add("Yes!");
        answers.Add("No!");
        answers.Add("I believe so!");
        answers.Add("What do you think?");
        speeches.Add(speechType.answer, answers);
        */
    }

    // Update is called once per frame
    void Update()
    {
    }

    /*
    public string GetLine(speechType key)
    {
        int i = Random.Range(0, speeches[key].Count);
        return speeches[key][i];
    }
    */

    public Sprite GetEmojiReaction(AvatarAttitude.Mood mood)
    {
        int i = Random.Range(0, emojisReactions[mood].Count);
        return emojisReactions[mood][i];
    }

    public Sprite GetEmojiTopic(EnvironmentLevel environmentLevel)
    {
        int i = Random.Range(0, emojisTopics[environmentLevel].Count);
        return emojisTopics[environmentLevel][i];
    }

    public EmojiLine GenerateEmojiLine(EnvironmentLevel environmentLevel, AvatarAttitude.Mood mood)
    {
        EmojiLine line = new EmojiLine();
        line.emojiTopic = GetEmojiTopic(environmentLevel);
        line.emojiReaction = GetEmojiReaction(mood);
        line.environmentLevel = environmentLevel;
        line.mood = mood;
        return line;
    }

}