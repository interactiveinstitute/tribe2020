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
	public static int currentSlot;

	public bool overrideSlot = false;
    [SerializeField]
    private int saveSlot = -1;

    public bool debug = false;

	public string fileName;
	[TextArea(3, 10)]
	public string defaultContent;
	private string _filePath = "";
	private JSONNode _dataClone = new JSONNode();
	#endregion

	//Sort use instead of constructor
	void Awake() {
		_instance = this;
		_filePath = Application.persistentDataPath + "/" + fileName;
		//_dataClone = ReadFileAsJSON();
		Load();
	}

	//Use this for initialization
	void Start() {

	}

    //Callback when interacting with component in editor
    void OnValidate() {
		if(overrideSlot) {
			currentSlot = saveSlot;
		}
    }
	
	//Update is called once per frame
	void Update() {
	}

	//
	public void CheckVersion() {
		if(GetData("version") != Application.version) {
			Debug.Log("wrong version");
		} else {
			Debug.Log("right version");
		}
	}

	public JSONNode GetData(string key) {
		return _dataClone[key];
	}

	//
	public JSONNode GetCurrentSlotData(string key) {
		return GetData(currentSlot, key);
	}

	//
	public JSONNode GetData(int slot, string key) {
		if(_dataClone["slots"][slot] != null) {
			if(_dataClone["slots"][slot][key] != null) {
				return _dataClone["slots"][slot][key];
			} else {
				return null;
			}
		}
		return null;
	}

	//
	public JSONClass GetCurrentSlotClass(string field) {
		return GetClass(currentSlot, field);
	}

	//
	public JSONClass GetClass(int slot, string field) {
		if(_dataClone["slots"][slot][field] != null) {
			return _dataClone["slots"][slot][field].AsObject;
		} else {
			return null;
		}
	}

	//
	public JSONClass GetClass(string field) {
		if(_dataClone[field] != null) {
			return _dataClone[field].AsObject;
		} else {
			return null;
		}
	}

	public void SetData(string key, string value) {
		_dataClone.Add(key, value);
	}

	//
	public void SetCurrentSlotData(string key, string value) {
		SetData(currentSlot, key, value);
	}

	//
	public void SetData(int slot, string field, string value) {
		_dataClone["slots"][slot].Add(field, value);
	}

	//
	public void SetCurrentSlotClass(string key, JSONClass value) {
		SetClass(currentSlot, key, value);
	}

	//
	public void SetClass(int slot, string field, JSONClass value) {
		_dataClone["slots"][slot].Add(field, value);
	}

	//
	public void SetClass(string field, JSONClass value) {
		_dataClone.Add(field, value);
	}

	//
	public void RemoveClass(string field) {
		_dataClone.Remove(field);
	}

	//
	public void SetCurrentSlotArray(string key, JSONArray value) {
		SetArray(currentSlot, key, value);
	}

	//
	public void SetArray(int slot, string key, JSONArray value) {
		_dataClone["slots"][slot].Add(key, value);
	}

	//
	public bool IsSlotVacant(int slot) {
		return GetData(slot, "curPilot") == null;
	}

	//
	public void ClearFile() {
		File.WriteAllText(Application.persistentDataPath + "/" + fileName, defaultContent);
		Load();

		Debug.Log(name + ": " + Application.persistentDataPath + "/" + fileName + " was cleared");
	}

	//
	public void Delete(int slot) {
		_dataClone["slots"][slot] = new JSONClass();
		File.WriteAllText(_filePath, _dataClone.ToString());
	}

	//
	public string GetSlotData(int slot) {
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
		SetData("lastSlot", "-1");
		File.WriteAllText(_filePath, _dataClone.ToString());
	}

	//
	public void SaveCurrentSlot(bool currentScene = true) {
		Save();
	}

	//Overwrite save file with current runtime serialization
	public void Save() {
		//if(currentSlot != -1) {
			File.WriteAllText(_filePath, _dataClone.ToString());
		//}
	}

	//Load data from file into runtime serialization
	public void Load() {
		if(debug) { Debug.Log("Load: " + currentSlot); }

		_dataClone = ReadFileAsJSON();
		if(currentSlot == -1) {
			currentSlot = saveSlot;
		}
	}

	//
	public void LoadCurrentSlot() {
		Load(currentSlot);
	}

	//
	public void Load(int slot) {
		if(debug) { Debug.Log("Load: " + currentSlot); }

		_dataClone = ReadFileAsJSON();
		currentSlot = slot;
		SetData("lastSlot", slot.ToString());
	}

	//
	public JSONNode ReadFileAsJSON() {
		if(_filePath == "") { return null; }
		if(!File.Exists(_filePath)) { InitFile(); }

		string fileClone = File.ReadAllText(_filePath);
		if(fileClone == "") {
			InitFile();
			fileClone = File.ReadAllText(_filePath);
		}

		JSONNode json = JSON.Parse(fileClone);
		if(json["slots"] == null) {
			InitFile();
			json = JSON.Parse(fileClone);
		}

		return json;
	}

	//
	public void InitFile() {
		File.WriteAllText(_filePath, defaultContent);
	}
}
