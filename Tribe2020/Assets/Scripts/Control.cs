using UnityEngine;
using System.Collections;

public class Control : MonoBehaviour {
	public Collider groundPlane;
	public GameObject ground;
	public GameObject block;
	public GameObject floor, campfire;
	public Transform markerObject;
	public GridManager gridMgr;
	public ArrayList blocks;

	public enum Block {Floor, Campfire};
	private Block _curBlock;

	// Use this for initialization
	void Start(){
		_curBlock = Block.Floor;

		floor = GameObject.Find ("ent_block");
		campfire = GameObject.Find ("ent_campfire");

		blocks = new ArrayList();
		ground = (GameObject)GameObject.Find ("ent_ground");
		groundPlane = ground.GetComponent<Collider> ();

		gridMgr = GameObject.Find("mgr_grid").GetComponent<GridManager>();
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
				gridMgr.AddCell(markerObject.position.x, markerObject.position.z);

				GameObject newBlock;
				if(_curBlock == Block.Floor){
					newBlock =(GameObject)Instantiate(
						floor, markerObject.transform.position, Quaternion.identity);
				} else{
					newBlock =(GameObject)Instantiate(
						campfire, markerObject.transform.position, Quaternion.identity);
				}

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

	public void OnFloorPressed(){
		_curBlock = Block.Floor;
	}

	public void OnCampFirePressed(){
		_curBlock = Block.Campfire;
	}
}
