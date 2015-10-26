using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using UnityEngine.UI;

public class SaveManager : MonoBehaviour{
	public GameObject WALL;
	private MeshManager _meshMgr;

	private string _path;
	public List<SerialObject> savedGames = new List<SerialObject>();
//	public string dummyText = "dummy";

	public Transform map;

	private Text _saveText;

	// Use this for initialization
	void Start () {
		GameObject saveTextGO = GameObject.FindWithTag("save_text") as GameObject;
		_saveText = saveTextGO.GetComponentsInChildren<Text>()[1];
//		_marker2 = GameObject.FindWithTag("ui_pointer2") as GameObject;
//		_selectArea = GameObject.FindWithTag("ui_selection") as GameObject;

		_meshMgr = GameObject.FindWithTag("managers").GetComponent<MeshManager>();

		_path = Application.persistentDataPath  + "/savedGames.gd";
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void DummySave(){
		Save(map.gameObject);
//		Save(_saveText.text);
	}

	public void Save(GameObject data){
		List<SerialObject> meshList = new List<SerialObject>();
		foreach(Transform transform in map.GetComponentsInChildren<Transform>()){
			meshList.Add(new SerialObject(transform.gameObject));
		}
		savedGames = meshList;
		BinaryFormatter bf = new BinaryFormatter();
		FileStream file = File.Create(_path);
		bf.Serialize(file, savedGames);
		file.Close();
		Debug.Log("saved: " + data);
	}

	public void Load(){
		if(File.Exists(_path)) {
			BinaryFormatter bf = new BinaryFormatter();
			FileStream file = File.Open(_path, FileMode.Open);
			savedGames = (List<SerialObject>)bf.Deserialize(file);
			file.Close();

//			_saveText.text = savedGames[0].ToString();
//			return savedGames[0].data;
		}
		Debug.Log("loaded: " + savedGames.Count);

		foreach(SerialObject so in savedGames) {
			InsantiateSerialObject(so);
		}

//		return "";
	}

	public void InsantiateSerialObject(SerialObject so){
		_meshMgr.AddMesh(so.ToGameObject());


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
