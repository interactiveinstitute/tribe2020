using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MeshManager : MonoBehaviour{
	public GameObject WALL, CAMPFIRE, COFFEE_MACHINE, TOILET;
	private List<GameObject> _meshes;
	
	// Use this for initialization
	void Start(){
		WALL = GameObject.Find("Block Wall");
		CAMPFIRE = GameObject.Find("Block Campfire");
		COFFEE_MACHINE = GameObject.Find ("Block Coffee Machine");
		TOILET = GameObject.Find ("Block Toilet");
		_meshes = new List<GameObject>();
	}
	
	// Update is called once per frame
	void Update(){
	}

	public void AddMesh(Vector3 start, Vector3 end, SimulationManager.Block type){
		Vector3 pos = start + (end - start) / 2;
		GameObject newMesh =
			Instantiate(TypeToObject(type), pos, Quaternion.identity) as GameObject;

//		float scaleX = Mathf.Max(start.x, end.x) - Mathf.Min(start.x, end.x) + 2;
//		float scaleZ = Mathf.Max(start.z, end.z) - Mathf.Min(start.z, end.z) + 2;
//
//		newMesh.transform.localScale = new Vector3(scaleX, 10, scaleZ);
		newMesh.transform.localScale = TypeToScale(type, start.x, end.x, start.z, end.z);
		_meshes.Add(newMesh);
	}

	public GameObject CollidesWithBlock(GameObject otherObj){
//		Collider col = otherObj.GetComponent<Collider>().bounds;
		Bounds pb = otherObj.GetComponent<Collider>().bounds;
		foreach(GameObject b in _meshes){
			Bounds bb = b.GetComponent<Collider>().bounds;
			if(pb.Intersects(bb)){
				return b;
			}
		}
		return null;
	}

	public void DestroyMesh(GameObject mesh){
		_meshes.Remove (mesh);
		Destroy (mesh);
		//		Vector3 pos = obj.transform.position / 5;
		//		
		//		cells[(int)pos.x, (int)pos.y, (int)pos.z].GetComponent<Cell>().Reset();
		//	}
	}

	private GameObject TypeToObject(SimulationManager.Block type){
		switch(type){
		case SimulationManager.Block.Floor:
			return WALL;
		case SimulationManager.Block.Campfire:
			return CAMPFIRE;
		case SimulationManager.Block.Coffee:
			return COFFEE_MACHINE;
		case SimulationManager.Block.Toilet:
			return TOILET;
		default:
			return WALL;
		}
	}

	private Vector3 TypeToScale(
		SimulationManager.Block type, float x1, float x2, float z1, float z2){
		Vector3 scale = new Vector3();
		switch(type){
		case SimulationManager.Block.Wall:
			scale.x = Mathf.Max(x1, x2) - Mathf.Min(x1, x2) + 2;
			scale.y = 10;
			scale.z = Mathf.Max(z1, z2) - Mathf.Min(z1, z2) + 2;
			break;
		case SimulationManager.Block.Campfire:
		case SimulationManager.Block.Coffee:
		case SimulationManager.Block.Toilet:
		default:
			scale.x = Mathf.Max(x1, x2) - Mathf.Min(x1, x2) + 5;
			scale.y = 5;
			scale.z = Mathf.Max(z1, z2) - Mathf.Min(z1, z2) + 5;
			break;
		}

		return scale;
	}
}
