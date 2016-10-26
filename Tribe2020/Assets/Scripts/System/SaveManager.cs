using UnityEngine;
using System;
using System.IO;
using System.Text.RegularExpressions;
using SimpleJSON;

public class SaveManager : MonoBehaviour{
	//Singleton features
	private static SaveManager _instance;
	public static SaveManager GetInstance(){
		return _instance;
	}

	public static int currentSlot = 0;

	private GameTime _timeMgr;
	private ResourceManager _resourceMgr;
	private NarrationManager _narrationMgr;

	public string fileName;
	private string _filePath;
	private JSONNode _dataClone = new JSONNode();

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
	public JSONClass GetClass(int slot, string field) {
		if(_dataClone["instances"][slot][field] != null) {
			return _dataClone["instances"][slot][field].AsObject;
		} else {
			return null;
		}
	}

	//
	public void SetData(int slot, string field, string value) {
		_dataClone["instances"][slot].Add(field, value);
	}

	//
	public void SetData(int slot, string field, JSONClass value) {
		_dataClone["instances"][slot].Add(field, value);
	}

	//
	public bool IsSlotVacant(int slot) {
		return GetData(slot, "pilot") == null;
	}

	public void Delete(int slot) {
		_dataClone["instances"][slot].Remove("pilot");
		_dataClone["instances"][slot].Remove("lastTime");
		File.WriteAllText(_filePath, _dataClone.ToString());
	}

	//
	public string GetSlotData(int slot) {
		//_dataClone = ReadFileAsJSON();
		//Debug.Log("GetSlotData: " + slot);

		JSONNode fileData = GetData(slot, "pilot");

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
	public void InitSlot(int slot, string pilot) {
		//SetData(slot, "pilot", pilot);

		//SetData(slot, "lastTime", "1452691843.939");
		//SetData(slot, "money", "500");
		//SetData(slot, "comfort", "500");

		//SetData(slot, "questState", _narrationMgr.Encode());

		//File.WriteAllText(_filePath, _dataClone.ToString());
	}

	//
	public void Save(int slot, bool currentScene = true) {
		if(currentScene) {
			SetData(slot, "pilot", Application.loadedLevelName);
		}

		SetData(slot, "lastTime", _timeMgr.time.ToString());
		SetData(slot, "money", _resourceMgr.cash.ToString());
		SetData(slot, "comfort", _resourceMgr.comfort.ToString());

		SetData(slot, "questState", _narrationMgr.Encode());

		File.WriteAllText(_filePath, _dataClone.ToString());
	}

	//
	public void Load(int slot) {
		//Debug.Log("Load: " + currentSlot);
		_dataClone = ReadFileAsJSON();

		_dataClone.Add("lastSlot", "" + slot);

		if(GetData(slot, "lastTime") != null) {
			_timeMgr.SetTime(GetData(slot, "lastTime").AsDouble);
			_resourceMgr.cash = GetData(slot, "money").AsInt;
			_resourceMgr.comfort = GetData(slot, "comfort").AsInt;

			_narrationMgr.Decode(GetClass(slot, "questState"));
		} else {
			_narrationMgr.SetStartState();
		}
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
