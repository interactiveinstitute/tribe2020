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

	public bool debug = false;

	public string fileName;
	private string _filePath;
	private JSONNode _dataClone = new JSONNode();
	#endregion

	//Sort use instead of constructor
	void Awake(){
		_instance = this;
		_filePath = Application.persistentDataPath + "/" + fileName;
		//_dataClone = ReadFileAsJSON();
		Load();
	}

	//Use this for initialization
	void Start(){
	}
	
	//Update is called once per frame
	void Update(){
	}

	//
	public JSONNode GetData(string key) {
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
	public JSONClass GetClass(string field) {
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
	public void SetData(string key, string value) {
		SetData(currentSlot, key, value);
	}

	//
	public void SetData(int slot, string field, string value) {
		_dataClone["slots"][slot].Add(field, value);
	}

	//
	public void SetData(string key, JSONClass value) {
		SetData(currentSlot, key, value);
	}

	//
	public void SetData(int slot, string field, JSONClass value) {
		_dataClone["slots"][slot].Add(field, value);
	}

	//
	public void SetData(string key, JSONArray value) {
		SetData(currentSlot, key, value);
	}

	//
	public void SetData(int slot, string key, JSONArray value) {
		_dataClone["slots"][slot].Add(key, value);
	}

	//
	public bool IsSlotVacant(int slot) {
		return GetData(slot, "curPilot") == null;
	}

	//
	public void ClearFile() {
		File.WriteAllText(Application.persistentDataPath + "/" + fileName, "");
	}

	//
	public void Delete(int slot) {
		_dataClone["slots"].Remove(slot);
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
		if(debug) { Debug.Log("Load: " + currentSlot); }

		_dataClone = ReadFileAsJSON();
		_dataClone.Add("lastSlot", "" + slot);
	}

	//
	public JSONNode ReadFileAsJSON() {
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
		File.WriteAllText(_filePath, "{\"slots\":[]}");
	}
}
