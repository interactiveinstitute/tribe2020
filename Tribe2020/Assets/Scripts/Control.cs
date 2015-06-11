using UnityEngine;
using System.Collections;

public class Control : MonoBehaviour {
	public Collider groundPlane;
	public GameObject ground;
	public Transform markerObject;
	public GridManager gridMgr;

	private GridManager.Block _curBlock;
	private int _curLevel;

	// Use this for initialization
	void Start(){
		_curBlock = GridManager.Block.Floor;
		_curLevel = 0;

		ground = (GameObject)GameObject.Find("ent_ground");
		groundPlane = ground.GetComponent<Collider>();

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
			pos.y = _curLevel * 5;
			pos.z = Mathf.Floor(pos.z / 5) * 5;
			markerObject.position = pos;
		}

		//If mouse click, add or delete block depending on if space vacant
		if(Input.GetMouseButtonDown(0)){
			Vector3 pos = markerObject.transform.position / 5;

			GameObject colBlock = gridMgr.GetBlock((int)pos.x, (int)pos.y, (int)pos.z);

			if(colBlock == null){
				gridMgr.AddBlock((int)pos.x, (int)pos.y, (int)pos.z, _curBlock);
			} else{
				gridMgr.RemoveBlock(colBlock);
			}
		}
	}

	public void OnFloorPressed(){
		_curBlock = GridManager.Block.Floor;
	}

	public void OnCampFirePressed(){
		_curBlock = GridManager.Block.Campfire;
	}

	public void OnUpPressed(){
		_curLevel++;
	}

	public void OnDownPressed(){
		_curLevel--;
	}
}
