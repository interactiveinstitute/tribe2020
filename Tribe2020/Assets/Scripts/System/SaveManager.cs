using UnityEngine;
using System;
using System.IO;
using System.Text.RegularExpressions;
using SimpleJSON;
using System.Collections.Generic;

public class SaveManager : MonoBehaviour{
	//Singleton features
	private static SaveManager _instance;
	public static SaveManager GetInstance(){
		return _instance;
	}

	#region Fields
	public static int currentSlot = 0;

	private PilotController _controller;
	private GameTime _timeMgr;
	private ResourceManager _resourceMgr;
	private NarrationManager _narrationMgr;
    private PilotController _pilotController;

	public bool debug = false;

	/*Need reference to all avatars for saving and loading their state. Maybe we should have 
	the pilotcontroller hand over the references on init? Yeah let's try to achieve that.*/
    private List<BehaviourAI> _avatars = new List<BehaviourAI>();

    public Boolean enableSaveLoad;
	public string fileName;
	private string _filePath;
	private JSONNode _dataClone = new JSONNode();

	public List<GameObject> saveObjects;
	#endregion

	//Sort use instead of constructor
	void Awake(){
		_instance = this;
		_filePath = Application.persistentDataPath + "/" + fileName + ".gd";
		_dataClone = ReadFileAsJSON();
	}

	//Use this for initialization
	void Start(){
		_timeMgr = GameTime.GetInstance();
		_resourceMgr = ResourceManager.GetInstance();
		_narrationMgr = NarrationManager.GetInstance();

	}
	
	//Update is called once per frame
	void Update(){

	}

	//
	public JSONNode GetData(string key) {
		return GetData(currentSlot, key);
	}

	//
	public JSONNode GetData(int slot, string field) {
		if(_dataClone["instances"][slot] != null) {
			if(_dataClone["instances"][slot][field] != null) {
				return _dataClone["instances"][slot][field];
			} else {
				return null;
			}
		}
		return null;
	}

	//
	public JSONClass GetClass(string field) {
		return GetClass(currentSlot, field);
	}

	//
	public JSONClass GetClass(int slot, string field) {
		if(_dataClone["instances"][slot][field] != null) {
			return _dataClone["instances"][slot][field].AsObject;
		} else {
			return null;
		}
	}

	//
	public void SetData(string key, string value) {
		SetData(currentSlot, key, value);
	}

	//
	public void SetData(int slot, string field, string value) {
		_dataClone["instances"][slot].Add(field, value);
	}

	//
	public void SetData(string key, JSONClass value) {
		SetData(currentSlot, key, value);
	}

	//
	public void SetData(int slot, string field, JSONClass value) {
		_dataClone["instances"][slot].Add(field, value);
	}

	//
	public void SetData(string key, JSONArray value) {
		SetData(currentSlot, key, value);
	}

	//
	public void SetData(int slot, string key, JSONArray value) {
		_dataClone["instances"][slot].Add(key, value);
	}

	//
	public bool IsSlotVacant(int slot) {
		return GetData(slot, "curPilot") == null;
	}

	public void Delete(int slot) {
		//_dataClone["instances"][slot].Remove("curPilot");
		//_dataClone["instances"][slot].Remove("lastTime");
		_dataClone["instances"].Remove(slot);
		File.WriteAllText(_filePath, _dataClone.ToString());
	}

	//
	public string GetSlotData(int slot) {
		//_dataClone = ReadFileAsJSON();
		if(debug) { Debug.Log("GetSlotData: " + slot); }

		JSONNode fileData = GetData(slot, "curPilot");

		if(fileData == null) {
			return "New Game";
		}

		return fileData;
	}

	//
	public int GetLastSlot() {
		if(_dataClone["lastSlot"] != null) {
			return _dataClone["lastSlot"].AsInt;
		}
		return -1;
	}

	//
	public void ResetLastSlot() {
		_dataClone.Remove("lastSlot");
		File.WriteAllText(_filePath, _dataClone.ToString());
	}

	//
	public void Save(bool currentScene = true) {
		Save(currentSlot, currentScene);
	}

	//
	public void Save(int slot, bool currentScene = true) {
		File.WriteAllText(_filePath, _dataClone.ToString());
	}

	//
	public void Load() {
		Load(currentSlot);
	}

	//
	public void Load(int slot) {
        if(!enableSaveLoad) {
			if(debug) { Debug.Log("save/load disabled. Will not load game data."); }
            return;
        }

		if(debug) { Debug.Log("Load: " + currentSlot); }

		_dataClone = ReadFileAsJSON();
		_dataClone.Add("lastSlot", "" + slot);
	}

	//
	public JSONNode ReadFileAsJSON() {
		if(!File.Exists(_filePath)) {
			File.WriteAllText(_filePath, "{\"instances\":[]}");
		}

		string fileClone = File.ReadAllText(_filePath);
		JSONNode json = JSON.Parse(fileClone);
		return json;
	}
}
