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

    //Sort use instead of constructor
    void Awake(){
        _instance = this;
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
            float progress = Application.GetStreamProgressForLevel(nextScene);

            loadingBar.localScale = progress * Vector2.right + loadingBar.localScale.y * Vector2.up;

            if(progress == 1){
				
                SceneManager.LoadSceneAsync(nextScene);
                autoLoad = false;
            }
        }
    }

    public void LoadScene(string scene){
        nextScene = scene;
		SceneManager.LoadScene("LoadingScene");
    }
}