using UnityEngine;
using System.Collections.Generic;

public class MovieController : Controller {
	[SerializeField]
	private MovieView _movieView;
	[SerializeField]
	private VideoPlane _video;

	private CustomSceneManager _sceneMgr;

	public GameObject videoObject;
	private MediaPlayerCtrl _videoController;
	[SerializeField]
	private LocalisationManager _localMgr;
	private SaveManager _saveMgr;

	private bool _isStarted = false;
	private float _subtitleTimer = 0;

	[System.Serializable]
	public struct Subtitle {
		public string text;
		public float time;
	}
	[SerializeField]
	private List<Subtitle> _subtitles;
	private int _subtitleIndex = 0;

	private float _videoDuration;
	private float _videoTime = 0;

	// Use this for initialization
	void Start () {
		_sceneMgr = CustomSceneManager.GetInstance();
		_localMgr = LocalisationManager.GetInstance();
		_saveMgr = SaveManager.GetInstance();

		//_video.Play();
		//_videoDuration = _video.GetDuration();

		_localMgr.SetLanguage(_saveMgr.GetData("language"));

		if(videoObject != null) {
			_videoController = videoObject.GetComponent<MediaPlayerCtrl>();
		}
	}
	
	// Update is called once per frame
	void Update () {
		//_videoTime += Time.deltaTime;
		//if(_videoTime >= 97) {
		//	_sceneMgr.LoadScene("MenuScene");
		//}

		//if(_subtitles[_subtitleIndex].time < _videoTime && _subtitleIndex < _subtitles.Count - 1) {
		//	_movieView.ShowSubtitle(_subtitles[_subtitleIndex].text + ", " + _videoTime);
		//	_subtitleIndex++;
		//}

		if(videoObject != null) {
			//Video is Over
			if(_videoController.GetCurrentState() == MediaPlayerCtrl.MEDIAPLAYER_STATE.END) {
				_sceneMgr.LoadScene("MenuScene");
			}

			//Video is Started
			if(!_isStarted && _videoController.GetCurrentState() == MediaPlayerCtrl.MEDIAPLAYER_STATE.PLAYING) {
				OnVideoStarted();
			}
		}

		if(_isStarted) {
			_subtitleTimer += Time.fixedDeltaTime;
			if(_subtitles[_subtitleIndex].time < _videoController.GetSeekPosition() && _subtitleIndex < _subtitles.Count - 1) {
				if(_subtitles[_subtitleIndex].text == "") {
					_movieView.ShowSubtitle("");
				} else {
					_movieView.ShowSubtitle(_localMgr.GetPhrase("Intro Movie", "" + _subtitleIndex));
				}
				_subtitleIndex++;
			}
		}
	}

	//
	public void OnVideoStarted() {
		_isStarted = true;
	}
}
