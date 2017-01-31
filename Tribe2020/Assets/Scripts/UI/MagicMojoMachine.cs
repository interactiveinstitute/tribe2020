// Script for testing the graphics by changing it on interval of 1 sec

using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;


public class MagicMojoMachine : MonoBehaviour {

    [Header("Mood text")]
    public Text MoodText;

	[Header("Mood Icon")]
	public Image MoodImage;

    [Header("Sliders")]
    public Slider SatisfactionSlider;
    public Slider TemperatureSlider;
    public Slider KnowledgeSlider;
    public Slider AttitudeSlider;
    public Slider NormSensitivitySlider;

    [Header("Images for the moods")]
    public Sprite MoodSprite1;
    public Sprite MoodSprite2;
    public Sprite MoodSprite3;
    public Sprite MoodSprite4;
    public Sprite MoodSprite5;

	[Header("Image for ee label")]
	public Image eeLabelImage;

	[Header("Energy efficiency labels")]
	public Sprite eeLabel1;
	public Sprite eeLabel2;
	public Sprite eeLabel3;
	public Sprite eeLabel4;
	public Sprite eeLabel5;
	public Sprite eeLabel6;
	public Sprite eeLabel7;
	public Sprite eeLabel8;



    List<string> moodList = new List<string>();
    List<Sprite> moodImageList = new List<Sprite>();
	List<Sprite> eeLabelList = new List<Sprite>();

    // Use this for initialization
    void Start () {

        InvokeRepeating("IntervalUpdate", 0, 1);

        moodList.Add("Billy is very angry");
        moodList.Add("Billy is a sad blue boy");
        moodList.Add("Billy is happy");
        moodList.Add("Billy is suicidal");
        moodList.Add("Billy is BigBillyBossMan");
		moodList.Add ("Billy is Billy");

        moodImageList.Add(MoodSprite1);
        moodImageList.Add(MoodSprite2);
        moodImageList.Add(MoodSprite3);
        moodImageList.Add(MoodSprite4);
        moodImageList.Add(MoodSprite5);

		eeLabelList.Add(eeLabel1);
		eeLabelList.Add(eeLabel2);
		eeLabelList.Add(eeLabel3);
		eeLabelList.Add(eeLabel4);
		eeLabelList.Add(eeLabel5);
		eeLabelList.Add(eeLabel6);
		eeLabelList.Add(eeLabel7);
		eeLabelList.Add(eeLabel8);

    }




    void IntervalUpdate() {

        int randomArrayIndex = Random.Range(0, 5);

        MoodImage.sprite = moodImageList[randomArrayIndex];
        MoodText.text = moodList[randomArrayIndex];
		eeLabelImage.sprite = eeLabelList[randomArrayIndex];

        SatisfactionSlider.value = Random.Range(0, 40);
        TemperatureSlider.value = Random.Range(0, 40);
        KnowledgeSlider.value = Random.Range(0, 40);
        AttitudeSlider.value = Random.Range(0, 40);
        NormSensitivitySlider.value = Random.Range(0, 40);

    }

}
