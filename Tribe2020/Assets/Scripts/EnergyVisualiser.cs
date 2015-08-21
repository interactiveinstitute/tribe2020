using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EnergyVisualiser : MonoBehaviour {
//	private List<UICell> _cells;
	private GridManager _gridMgr;
//	private CellPure[,] _cells;

	private Collider _groundPlane;

	// Use this for initialization
	void Start(){
		 GameObject CELL_FEEDBACK = GameObject.FindWithTag("cell_feedback");

		_gridMgr = GameObject.FindWithTag("grid_manager").GetComponent<GridManager>();

		_groundPlane = GameObject.FindWithTag("ent_ground").GetComponent<Collider>();

		for(int z = 0; z < 27; z++){
			for(int x = 0; x < 13; x++){
				float xPos = (x + z - (z + 1) / 2) * 5 - 2.5f;
				float zPos = (x - z + z / 2) * 5 + 2.5f;
				Vector3 pos = new Vector3(xPos, 0f, zPos);
				GameObject newCell =
					Instantiate(CELL_FEEDBACK, pos, CELL_FEEDBACK.transform.rotation) as GameObject;
				newCell.transform.parent = transform;
				newCell.transform.position = pos;
			}
		}
		Vector3 startPos = new Vector3 (0f, 0.1f, 0f);
		transform.position = startPos;
	}
	
	// Update is called once per frame
	void Update(){
//		Vector3 screenOrigo = PointOnGround (new Vector2 (0, Screen.height), _groundPlane);
//		screenOrigo.x = Mathf.Floor (screenOrigo.x / 5) * 5 - 2.5f;
//		screenOrigo.y = - 3.5f;
//		screenOrigo.z = Mathf.Floor (screenOrigo.z / 5) * 5 + 5f;

		Ray ray = Camera.main.ScreenPointToRay(new Vector2(0, Screen.height));
		Vector3 point = ray.origin + (ray.direction * 166f);
		point.x = Mathf.Floor(point.x / 5) * 5 + 2.5f;
		point.y = transform.position.y;
		point.z = Mathf.Floor(point.z / 5) * 5 - 2.5f;

		transform.position = point;

//		Debug.Log (point);

		foreach(Transform child in transform){
			Vector3 checkPos = new Vector3();
			checkPos.x = /*point.x + */child.transform.position.x;
			checkPos.y = 5f;
			checkPos.z = /*point.z + */child.transform.position.z;
			checkPos /= 5;

			float heat = _gridMgr.GetHeat(checkPos);

			CellFeedback cf = child.GetComponent<CellFeedback>();
			cf.SetColor(new Color(0.5f, heat / 255, 0.5f));
		}
	}

	private Vector3 PointOnGround(Vector2 screenCoord, Collider plane){
		Ray ray = Camera.main.ScreenPointToRay(screenCoord);
		Vector3 point = ray.origin + (ray.direction * 90f);

		return point;

		RaycastHit hit;
		
		if(plane.Raycast(ray, out hit, 10000.0f)){
			return ray.GetPoint(hit.distance);
		}
		return new Vector3();
	}

	public void SetVisible(bool visible){
		foreach (Transform child in transform) {
			child.GetComponent<Renderer>().enabled = visible;
		}
	}

	public void SetFloor(float newY){
		Vector3 newPos = new Vector3(transform.position.x, newY, transform.position.z);
		transform.position = newPos;
	}
}
