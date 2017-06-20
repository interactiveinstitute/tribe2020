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
        public AvatarMood.Mood mood;
    }

    Dictionary<AvatarMood.Mood, List<Sprite>> emojisReactions = new Dictionary<AvatarMood.Mood, List<Sprite>>();
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

	//
	void Awake() {
		emojisReactions.Add(AvatarMood.Mood.angry, emojisReactionsAngry);
		emojisReactions.Add(AvatarMood.Mood.sad, emojisReactionsSad);
		emojisReactions.Add(AvatarMood.Mood.tired, emojisReactionsTired);
		emojisReactions.Add(AvatarMood.Mood.neutral_neg, emojisReactionsNeutralNegative);
		emojisReactions.Add(AvatarMood.Mood.neutral_pos, emojisReactionsNeutralPositive);
		emojisReactions.Add(AvatarMood.Mood.surprised, emojisReactionsSurprised);
		emojisReactions.Add(AvatarMood.Mood.happy, emojisReactionsHappy);
		emojisReactions.Add(AvatarMood.Mood.euphoric, emojisReactionsEuphoric);

		emojisTopics.Add(EnvironmentLevel.negative, emojisTopicsNegative);
		emojisTopics.Add(EnvironmentLevel.neutral, emojisTopicsNeutral);
		emojisTopics.Add(EnvironmentLevel.positive, emojisTopicsPositive);
	}

    // Use this for initialization
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
    }

    public Sprite GetEmojiReaction(AvatarMood.Mood mood)
    {
        int i = Random.Range(0, emojisReactions[mood].Count);
        return emojisReactions[mood][i];
    }

    public Sprite GetEmojiTopic(EnvironmentLevel environmentLevel)
    {
		if(!emojisTopics.ContainsKey(environmentLevel)) {
			Debug.Log(environmentLevel);
		}

		int i = Random.Range(0, emojisTopics[environmentLevel].Count);
        return emojisTopics[environmentLevel][i];
    }

    public EmojiLine GenerateEmojiLine(EnvironmentLevel environmentLevel, AvatarMood.Mood mood)
    {
        EmojiLine line = new EmojiLine();
        line.emojiTopic = GetEmojiTopic(environmentLevel);
        line.emojiReaction = GetEmojiReaction(mood);
        line.environmentLevel = environmentLevel;
        line.mood = mood;
        return line;
    }

}