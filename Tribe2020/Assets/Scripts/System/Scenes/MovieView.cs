using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class MovieView : MonoBehaviour {
	[SerializeField]
	private Text _subtitles;

	// Use this for initialization
	void Start() {
		
	}
	
	// Update is called once per frame
	void Update() {
		
	}

	//
	public void ShowSubtitle(string subtitle) {
		_subtitles.text = subtitle;
	}
}
