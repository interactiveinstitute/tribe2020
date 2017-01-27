using UnityEngine;
using UnityEngine.UI;
//using System.Collections;

public class UpdateSliderValue : MonoBehaviour {

    [Header("Add textfield and sliders")]
    public Text text;
    public Slider slider;

    public void UpdateText() {

        text.text = slider.value.ToString();

    }

}