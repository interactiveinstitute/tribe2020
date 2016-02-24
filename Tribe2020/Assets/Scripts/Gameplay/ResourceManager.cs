using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class ResourceManager : MonoBehaviour {
	//Singleton features
	private static ResourceManager _instance;
	public static ResourceManager GetInstance(){
		return _instance;
	}

	private UIManager _uiMgr;
	private GameTime _timeMgr;
	private List<Transform> _avatars;
	private List<Action> _actions;

	public float cash;
	public int comfort;
	public int temperature;
	public int power;
	public int co2;

	public float cashProduction;

	//Sort use instead of constructor
	void Awake(){
		_instance = this;
	}

	// Use this for initialization
	void Start(){
		_uiMgr = UIManager.GetInstance();
		_timeMgr = GameTime.GetInstance();

		_avatars = new List<Transform>();
		_actions = new List<Action>();

		RefreshProduction();
	}
	
	// Update is called once per frame
	void Update(){
		cash += _avatars.Count * Time.deltaTime;

		_uiMgr.cash.GetComponent<Text>().text = "" + (int)cash;
	}

	//
	public void RefreshProduction(){
		_avatars.Clear();
		_actions.Clear();
		cashProduction = 0;

		foreach(GameObject avatarObj in GameObject.FindGameObjectsWithTag("Avatar")){
			_avatars.Add(avatarObj.transform);
			cashProduction += 1;
		}

		foreach(GameObject appliance in GameObject.FindGameObjectsWithTag("Appliance")){
			foreach(Action action in appliance.GetComponents<Action>()){
				_actions.Add(action);
				if(action.performed){
					cashProduction += action.cashProduction;
				}
			}
		}
	}

	//
	public void RefreshProductionForAppliance(GameObject go){
		
	}
}