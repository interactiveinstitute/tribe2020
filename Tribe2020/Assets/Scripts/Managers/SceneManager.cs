using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class SceneManager : MonoBehaviour{
    //Singleton features
    private static SceneManager _instance;
    public static SceneManager GetInstance(){
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
                Application.LoadLevelAsync(nextScene);
                autoLoad = false;
            }
        }
    }

    public void LoadScene(string scene){
        nextScene = scene;
        Application.LoadLevel("LoadingScene");
    }
}