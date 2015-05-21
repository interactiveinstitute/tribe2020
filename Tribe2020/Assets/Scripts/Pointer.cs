using UnityEngine;
using System.Collections;

public class Pointer : MonoBehaviour {
	public Collider groundPlane;
	public GameObject ground;
	public Transform markerObject;
	public GameObject block;
	public ArrayList blocks;

	// Use this for initialization
	void Start(){
		blocks = new ArrayList();
		ground = (GameObject)GameObject.Find ("ent_ground");
		groundPlane = ground.GetComponent<Collider> ();
	}

	void Update(){
		Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
		RaycastHit hit;

		//Position marker according to grid
		if(groundPlane.Raycast(ray, out hit, 10000.0f)){
			markerObject.position = ray.GetPoint(hit.distance);
			Vector3 pos = markerObject.position;
			pos.x = Mathf.Floor(pos.x / 5) * 5;
			pos.z = Mathf.Floor(pos.z / 5) * 5;
			markerObject.position = pos;
		}

		//If mouse click, add or delete block depending on if space vacant
		if(Input.GetMouseButtonDown(0)){
			GameObject colBlock = CollidesWithBlock();

			if(colBlock == null){
				GameObject newBlock =
					(GameObject)Instantiate(block, markerObject.transform.position, Quaternion.identity);
				blocks.Add(newBlock);
			} else{
				blocks.Remove(colBlock);
				Destroy(colBlock);
			}
		}
	}

	private GameObject CollidesWithBlock(){
		Collider col = markerObject.GetComponent<Collider> ();
		Bounds pb = col.bounds;
		foreach(GameObject b in blocks){
			Bounds bb = b.GetComponent<Collider>().bounds;
			if(pb.Intersects(bb)){
				return b;
			}
		}
		return null;
	}
}
