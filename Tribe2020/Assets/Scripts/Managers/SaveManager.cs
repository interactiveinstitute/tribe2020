using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using SimpleJSON;
using UnityEngine.UI;

public class SaveManager : MonoBehaviour{
	//Singleton features
	private static SaveManager _instance;
	public static SaveManager GetInstance(){
		return _instance;
	}

	private BuildManager _buildMgr;
	private GameObject _graph;

	private string _path;
	public List<SerialObject> savedGames = new List<SerialObject>();

	public Transform map;

	private Text _saveText;

	//Sort use instead of constructor
	void Awake(){
		_instance = this;
	}

	//Use this for initialization
	void Start(){
		_buildMgr = BuildManager.GetInstance();

		_graph = GameObject.FindGameObjectWithTag("graph") as GameObject;

		_path = Application.persistentDataPath  + "/savedGames.gd";
	}
	
	//Update is called once per frame
	void Update(){
	}

	public void Save(string fileName){
		string path = Application.persistentDataPath + "/" + fileName + ".gd"; 
		StreamWriter file = File.CreateText(path);

		foreach(Transform t in _graph.transform){
			if(t.GetComponent<Room>() != null){
				string room = t.GetComponent<Room>().Stringify();
				file.WriteLine(room);
			}
		}

		foreach(Transform t in _graph.transform){
			if(t.GetComponent<Thing>() != null){
				string thing = t.GetComponent<Thing>().Stringify();
				Debug.Log(thing);
				file.WriteLine(thing);
			}
		}
		file.Close();
	}

	public void Load(string fileName){
		_buildMgr.Clear();

		string path = Application.persistentDataPath + "/" + fileName + ".gd"; 
		if(!File.Exists(path)) {
			return;
		}

		StreamReader sr = File.OpenText(path);
		string line = sr.ReadLine();
		while(line != null){
			var parse = JSON.Parse(line);
			string type = parse["type"].Value;

			if(type == "room"){
				_buildMgr.CreateRoom(line);
			} else{
				_buildMgr.AddThing(line);
			}

			line = sr.ReadLine();
		}
	}
}
