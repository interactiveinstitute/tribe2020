using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using SimpleJSON;

public class ResourceManager : MonoBehaviour {
	//Singleton features
	private static ResourceManager _instance;
	public static ResourceManager GetInstance(){
		return _instance;
	}

	private PilotView _uiMgr;
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
		_uiMgr = PilotView.GetInstance();
		_timeMgr = GameTime.GetInstance();

		_avatars = new List<Transform>();
		_appliances = new List<Appliance>();

		RefreshProduction();
	}
	
	// Update is called once per frame
	void Update(){
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

    public void AddComfort(int value) {
        comfort += value;
    }

    public void AddComfort(Gem gem) {
        comfort += gem.value;
    }

    //
    public void RefreshProductionForAppliance(GameObject go){
		
	}

	//
	public JSONClass SerializeAsJSON() {
		JSONClass json = new JSONClass();

		json.Add("money", cash.ToString());
		json.Add("comfort", comfort.ToString());

		return json;
	}

	//
	public void DeserializeFromJSON(JSONClass json) {
		if(json != null) {
			cash = json["money"].AsInt;
			comfort = json["comfort"].AsInt;
		}
	}
}