using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class UIManager1 : MonoBehaviour{
	//Singleton features
	private static UIManager1 _instance;
	public static UIManager1 GetInstance(){
		return _instance;
	}

	private InputManager _ioMgr;
	private BuildManager _buildMgr;

	public Text selectInfo;
	public Text debug1, debug2, debug3, debug4, debugY;

	//Sort use instead of constructor
	void Awake(){
		_instance = this;
	}

	//Use this for initialization
	void Start(){
		_ioMgr = InputManager.GetInstance();
		_buildMgr = BuildManager.GetInstance();

		//Selection interface
		selectInfo = GameObject.FindWithTag("selection_text").GetComponent<Text>();

		//Debug interface
		debug1 = GameObject.FindWithTag("debug_1").GetComponent<Text>();
		debug2 = GameObject.FindWithTag("debug_2").GetComponent<Text>();
		debug3 = GameObject.FindWithTag("debug_3").GetComponent<Text>();
		debug4 = GameObject.FindWithTag("debug_4").GetComponent<Text>();
		debugY = GameObject.FindWithTag("debug_y").GetComponent<Text>();

		//Generate item list
		GameObject LIST_ITEM = Resources.Load("UI/List Item") as GameObject;
		GameObject thingList = GameObject.FindWithTag("thing_list") as GameObject;

		List<string> things = _buildMgr.GetThings();
		List<GameObject> listItems = new List<GameObject>();
		for(int i = 0; i < things.Count; i++){
			string key = things[i];
			listItems.Add(Instantiate(LIST_ITEM, Vector2.zero, Quaternion.identity) as GameObject);
			listItems[i].GetComponentsInChildren<Text>()[0].text = things[i];
			listItems[i].GetComponent<Button>().onClick.AddListener(() => _ioMgr.OnItemPush(key));
		}
		AttachList(thingList, listItems);
	}
	
	//Update is called once per frame
	void Update(){
	}

	//Help method for filling a gui list with list elements
	private void AttachList(GameObject list, List<GameObject> elements){
		Transform listTransform = list.transform;
		ClearChildren(listTransform);
		
		foreach(GameObject element in elements){
			element.transform.SetParent(listTransform, false);
		}
	}

	//Help method for clearing a game object of children
	private void ClearChildren(Transform transform){
		List<GameObject> children = new List<GameObject>();
		foreach (Transform child in transform){
			children.Add (child.gameObject);
		}
		children.ForEach(child => Destroy(child));
	}

	//
	public string GetFileName(){
		GameObject fileGUI = GameObject.FindWithTag("save_text");
		Text saveText = fileGUI.GetComponentsInChildren<Text>()[1];
		Debug.Log("GetFileName: " + saveText.text);
		return saveText.text;
	}
}
