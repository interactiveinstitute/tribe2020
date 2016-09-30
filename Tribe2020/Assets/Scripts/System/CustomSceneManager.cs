using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class CustomSceneManager : MonoBehaviour{
    //Singleton features
    private static CustomSceneManager _instance;
    public static CustomSceneManager GetInstance(){
        return _instance;
    }

    public RectTransform loadingBar;
    public static string nextScene = "";
    public string defaultNextScene;
    public bool autoLoad;

	private float _progress = 0;

    //Sort use instead of constructor
    void Awake(){
        _instance = this;

		Screen.sleepTimeout = (int)SleepTimeout.NeverSleep;
	}

    // Use this for initialization
    void Start(){
        if (nextScene == ""){
            nextScene = defaultNextScene;
        }
    }

    // Update is called once per frame
    void Update(){
        if(autoLoad){
			SceneManager.LoadSceneAsync(nextScene);
			autoLoad = false;

			//float progress = Application.GetStreamProgressForLevel(nextScene);

			

			//if(_progress >= 1) {

			//	SceneManager.LoadSceneAsync(nextScene);
			//	autoLoad = false;
			//}
		}
		//_progress += Mathf.Min(1, Time.deltaTime * 0.5f);
		//loadingBar.localScale = new Vector3(_progress, 1, 1);
	}

    public void LoadScene(string scene){
        nextScene = scene;
		SceneManager.LoadScene("LoadingScene");
    }
}