using UnityEngine;

public class MovieController : Controller {
	private CustomSceneManager _sceneMgr;

	public GameObject videoObject;
	private MediaPlayerCtrl _videoController;

	// Use this for initialization
	void Start () {
		_sceneMgr = CustomSceneManager.GetInstance();

		if(videoObject != null) {
			_videoController = videoObject.GetComponent<MediaPlayerCtrl>();
		}
	}
	
	// Update is called once per frame
	void Update () {
		if(videoObject != null) {

			//Video is Over
			if(_videoController.GetCurrentState() == MediaPlayerCtrl.MEDIAPLAYER_STATE.END) {
				_sceneMgr.LoadScene("MenuScene");
			}
		}
	}
}
