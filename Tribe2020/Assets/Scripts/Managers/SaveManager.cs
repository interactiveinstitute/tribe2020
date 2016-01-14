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

	public GameObject WALL;
//	private MeshManager _meshMgr;

	private string _path;
	public List<SerialObject> savedGames = new List<SerialObject>();
//	public string dummyText = "dummy";

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

	public void DummySave(){
//		Save(map.gameObject);
//		Save(_saveText.text);
	}

	public void Save(){
		StreamWriter file = File.CreateText(_path);

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

	public void Load(){
		if(!File.Exists(_path)) {
			return;
		}

		StreamReader sr = File.OpenText(_path);
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

//		if(File.Exists(_path)) {
//			BinaryFormatter bf = new BinaryFormatter();
//			FileStream file = File.Open(_path, FileMode.Open);
//			savedGames = (List<SerialObject>)bf.Deserialize(file);
//			file.Close();
//
////			_saveText.text = savedGames[0].ToString();
////			return savedGames[0].data;
//		}
//		Debug.Log("loaded: " + savedGames.Count);
//
//		foreach(SerialObject so in savedGames) {
//			InsantiateSerialObject(so);
//		}
//
////		return "";
	}

	public void InsantiateSerialObject(SerialObject so){
//		_meshMgr.AddMesh(so.ToGameObject());


//		GameObject go =  Instantiate(WALL, Vector3.zero, Quaternion.identity) as GameObject;
//		GameObject stencil = so.ToGameObject();

//		go.transform.position = stencil.transform.position;
//		go.transform.rotation = stencil.transform.rotation;
//		go.transform.localScale = stencil.transform.localScale;
//		go.tag = so.GetTag();
//		go.transform.SetParent(map);
//
//		return go;
	}
}
