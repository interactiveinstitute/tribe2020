using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class AvatarSpeech : MonoBehaviour
{

    public enum speechType { greeting, goodbye, goodbyeFinal, reaction, topic, question, answer};
    public Dictionary<speechType, List<string>> speeches = new Dictionary<speechType, List<string>>();

    // Use this for initialization
    void Start()
    {
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
    }

    // Update is called once per frame
    void Update()
    {

    }

    public string GetLine(speechType key)
    {
        int i = Random.Range(0, speeches[key].Count);
        return speeches[key][i];
    }

}