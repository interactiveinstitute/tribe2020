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

	private string _filePath;
	private JSONNode _dataClone;

	//Sort use instead of constructor
	void Awake(){
		_instance = this;
		_filePath = Application.persistentDataPath + "/tribeSave.gd";
	}

	//Use this for initialization
	void Start(){
		_timeMgr = GameTime.GetInstance();
		_resourceMgr = ResourceManager.GetInstance();
	}
	
	//Update is called once per frame
	void Update(){
	}

	//
	public string GetData(string field) {
		string fieldData = "" + _dataClone["instances"][0][field].Value;
		return fieldData;
	}

	//
	public void SetData(string field, string value) {
		_dataClone["instances"][0][field].Value = value;
	}

	//
	public void Save(){
		//Regex pattern = new Regex("s/[{,\t,\r]/g");
		//string formattedData = _dataClone.ToString().Replace(",", ",\n").
		//	Replace("{", "{\n").Replace("}","\n}\n").Replace("[", "[\n").Replace("]", "\n]\n");

		SetData("lastTime", "" + _timeMgr.time);
		SetData("money", "" + _resourceMgr.cash);
		SetData("comfort", "" + _resourceMgr.comfort);

		File.WriteAllText(_filePath, _dataClone.ToString());
	}

	//
	public void Load() {
		_dataClone = ReadFileAsJSON();

		_timeMgr.SetTime(double.Parse(GetData("lastTime")));
		_resourceMgr.cash = int.Parse(GetData("money"));
		_resourceMgr.comfort = int.Parse(GetData("comfort"));
	}

	//
	public JSONNode ReadFileAsJSON() {
		if(!File.Exists(_filePath)) {
			Save();
		}

		string fileClone = "";

		StreamReader sr = File.OpenText(_filePath);
		string line = sr.ReadLine();
		while(line != null) {
			fileClone += line;

			line = sr.ReadLine();
		}
		sr.Close();

		JSONNode json = JSON.Parse(fileClone);
		return json;
	}
}
