using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;


public class MagicMojoMachine : MonoBehaviour {

    [Header("Mood text")]
    public Text MoodText;

    [Header("Sliders")]
    public Slider SatisfactionSlider;
    public Slider TemperatureSlider;
    public Slider KnowledgeSlider;
    public Slider AttitudeSlider;
    public Slider NormSensitivitySlider;

    [Header("Mood Icon")]
    public Image MoodImage;

    [Header("Images for the moods")]
    public Sprite MoodSprite1;
    public Sprite MoodSprite2;
    public Sprite MoodSprite3;
    public Sprite MoodSprite4;
    public Sprite MoodSprite5;



    List<string> moodList = new List<string>();
    List<Sprite> moodImageList = new List<Sprite>();

    // Use this for initialization
    void Start () {

        InvokeRepeating("IntervalUpdate", 0, 1);

        moodList.Add("Billy is very angry");
        moodList.Add("Billy is a sad blue boy");
        moodList.Add("Billy is happy");
        moodList.Add("Billy is suicidal");
        moodList.Add("Billy is BigBillyBossMan");

        moodImageList.Add(MoodSprite1);
        moodImageList.Add(MoodSprite2);
        moodImageList.Add(MoodSprite3);
        moodImageList.Add(MoodSprite4);
        moodImageList.Add(MoodSprite5);


    }




    void IntervalUpdate() {

        int randomArrayIndex = Random.Range(0, 5);

        MoodImage.sprite = moodImageList[randomArrayIndex];
        MoodText.text = moodList[randomArrayIndex];

        SatisfactionSlider.value = Random.Range(0, 40);
        TemperatureSlider.value = Random.Range(0, 40);
        KnowledgeSlider.value = Random.Range(0, 40);
        AttitudeSlider.value = Random.Range(0, 40);
        NormSensitivitySlider.value = Random.Range(0, 40);

    }
	

}
