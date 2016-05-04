using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Text.RegularExpressions;
using SimpleJSON;
using UnityEngine.UI;

public class SaveManager : MonoBehaviour{
	//Singleton features
	private static SaveManager _instance;
	public static SaveManager GetInstance(){
		return _instance;
	}

	private string _filePath;
	private JSONNode _dataClone;

	//Sort use instead of constructor
	void Awake(){
		_instance = this;

		_filePath = Application.persistentDataPath + "/tribeSave.gd";
		_dataClone = Load();
	}

	//Use this for initialization
	void Start(){
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

		File.WriteAllText(_filePath, _dataClone.ToString());
	}

	//
	public JSONNode Load() {
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

		JSONNode json = JSON.Parse(fileClone);
		return json;
	}
}
