using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EnergyVisualiser : MonoBehaviour {
//	private List<UICell> _cells;
	private GridManager _gridMgr;
//	private CellPure[,] _cells;

	private Collider _groundPlane;

	// Use this for initialization
	void Start () {
		 GameObject CELL_FEEDBACK = GameObject.FindWithTag("cell_feedback");

//		_cells = new List<UICell> ();
//		_cells = new CellPure[13, 27];
		_gridMgr = GameObject.FindWithTag("grid_manager").GetComponent<GridManager>();

		_groundPlane = GameObject.Find("ent_ground").GetComponent<Collider>() as Collider;

		for(int z = 0; z < 27; z++){
			for(int x = 0; x < 13; x++){
				float xPos = (x + z - (z + 1) / 2) * 5;
				float zPos = (x - z + z / 2) * 5;
				Vector3 pos = new Vector3(xPos, 0, zPos);
				GameObject newCell =
//					Instantiate(CELL_FEEDBACK, pos, Quaternion.identity) as GameObject;
					Instantiate(CELL_FEEDBACK, pos, CELL_FEEDBACK.transform.rotation) as GameObject;
				newCell.transform.parent = transform;
				newCell.transform.position = pos;
			}
		}

//		foreach (Transform child in transform) {
////			UICell uiCell = child.GetComponent<UICell>();
////			_cells.Add(uiCell);
//
////			CellPure cellScript = child.GetComponent<CellPure>();
////			cellScript.Init();
//		}
	}
	
	// Update is called once per frame
	void Update () {
		Vector3 screenOrigo = PointOnGround (new Vector2 (0, Screen.height), _groundPlane);
		screenOrigo.x = Mathf.Floor (screenOrigo.x / 5) * 5 - 2.5f;
		screenOrigo.y = - 3.5f;
		screenOrigo.z = Mathf.Floor (screenOrigo.z / 5) * 5 + 5f;

		transform.position = screenOrigo;

		foreach (Transform child in transform) {
//			Mesh mesh = child.GetComponent<MeshFilter>().mesh;
//			Color[] colours = mesh.colors;
//			for(int i = 0; i < colours.Length; i++){
//				colours[i] = new Color(255, 0, 125);//the new colour you want to set it to.
//			}

			Vector3 checkPos = transform.position;
			checkPos.x = checkPos.x + child.transform.position.x;
			checkPos.y = 5f;
			checkPos.z = checkPos.z + child.transform.position.z;
			checkPos /= 5;

			float heat = _gridMgr.GetHeat(checkPos);

			CellFeedback cf = child.GetComponent<CellFeedback>();
			cf.SetColor(new Color(0.5f, heat / 255, 0.5f));

//			Vector3 tmpPos = child.transform.position;
//			tmpPos.y = heat / 50;
//
//			Material newMaterial = new Material(Shader.Find("Standard"));
//			newMaterial.color = new Color(0.5f, heat / 255, 0.5f);
//
//			MeshRenderer tmpRenderer = child.GetComponent<MeshRenderer>();
//
//			tmpRenderer.material = newMaterial ;
//			Debug.Log (newMaterial.color);
		}
	}

	private Vector3 PointOnGround(Vector2 screenCoord, Collider plane){
		Ray ray = Camera.main.ScreenPointToRay(screenCoord);
		RaycastHit hit;
		
		if(plane.Raycast(ray, out hit, 10000.0f)){
			return ray.GetPoint(hit.distance);
		}
		return new Vector3();
	}
}
