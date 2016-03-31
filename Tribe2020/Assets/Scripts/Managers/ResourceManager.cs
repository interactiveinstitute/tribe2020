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

	private ViewManager _uiMgr;
	private GameTime _timeMgr;
	private List<Transform> _avatars;
	private List<Appliance> _appliances;

	public float cash;
	public float comfort;
	public int temperature;
	public int power;
	public int co2;

	public float cashProduction;
	public float comfortProduction;

	//Sort use instead of constructor
	void Awake(){
		_instance = this;
	}

	// Use this for initialization
	void Start(){
		_uiMgr = ViewManager.GetInstance();
		_timeMgr = GameTime.GetInstance();

		_avatars = new List<Transform>();
		_appliances = new List<Appliance>();

		RefreshProduction();
	}
	
	// Update is called once per frame
	void Update(){
		//cash += cashProduction * Time.deltaTime;
		//comfort += comfortProduction * Time.deltaTime;

		_uiMgr.cash.GetComponent<Text>().text = "" + (int)cash;
		_uiMgr.comfort.GetComponent<Text>().text = "" + (int)comfort;
	}

	//
	public void RefreshProduction(){
		_avatars.Clear();
		_appliances.Clear();
		cashProduction = 0;

		foreach(GameObject avatarObj in GameObject.FindGameObjectsWithTag("Avatar")){
			_avatars.Add(avatarObj.transform);
			cashProduction += 1;
			comfortProduction += 1;
		}

		foreach(GameObject applianceObj in GameObject.FindGameObjectsWithTag("Appliance")){
			foreach(Appliance appliance in applianceObj.GetComponents<Appliance>()){
				_appliances.Add(appliance);
				//if(action.performed){
				//	cashProduction += action.cashProduction;
				//}
			}
		}
	}

	//
	public void RefreshProductionForAppliance(GameObject go){
		
	}
}