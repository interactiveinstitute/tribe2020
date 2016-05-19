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

	private GameTime _timeMgr;
	private ResourceManager _resourceMgr;
	private QuestManager _questMgr;

	public string fileName;
	private string _filePath;
	private JSONNode _dataClone = new JSONNode();

	//Sort use instead of constructor
	void Awake(){
		_instance = this;
		_filePath = Application.persistentDataPath + "/" + fileName + ".gd";
	}

	//Use this for initialization
	void Start(){
		_timeMgr = GameTime.GetInstance();
		_resourceMgr = ResourceManager.GetInstance();
		_questMgr = QuestManager.GetInstance();
	}
	
	//Update is called once per frame
	void Update(){

	}

	//
	public JSONNode GetData(int instance, string field) {
		if(_dataClone["instances"][instance][field] != null) {
			return _dataClone["instances"][0][field];
		} else {
			return null;
		}
	}

	//
	public JSONClass GetClass(int instance, string field) {
		if(_dataClone["instances"][instance][field] != null) {
			return _dataClone["instances"][0][field].AsObject;
		} else {
			return null;
		}
	}

	//
	public void SetData(int instance, string field, string value) {
		_dataClone["instances"][instance].Add(field, value);
	}

	//
	public void SetData(int instance, string field, JSONClass value) {
		_dataClone["instances"][instance].Add(field, value);
	}

	//
	public void Save() {
		SetData(0, "lastTime", _timeMgr.time.ToString());
		SetData(0, "money", _resourceMgr.cash.ToString());
		SetData(0, "comfort", _resourceMgr.comfort.ToString());

		SetData(0, "questState", _questMgr.Encode());

		File.WriteAllText(_filePath, _dataClone.ToString());
	}

	//
	public void Load() {
		_dataClone = ReadFileAsJSON();

		if(GetData(0, "lastTime") != null) {
			_timeMgr.SetTime(GetData(0, "lastTime").AsDouble);
			_resourceMgr.cash = GetData(0, "money").AsInt;
			_resourceMgr.comfort = GetData(0, "comfort").AsInt;

			_questMgr.Decode(GetClass(0, "questState"));
		} else {
			_questMgr.SetStartState();
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
