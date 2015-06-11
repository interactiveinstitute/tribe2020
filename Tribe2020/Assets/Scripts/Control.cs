using UnityEngine;
using System.Collections;

public class Control : MonoBehaviour {
	public Collider groundPlane;
	public GameObject ground;
	public Transform markerObject;
	public GridManager gridMgr;
	public GameObject cameraHolder;

	private GridManager.Block _curBlock;
	private int _curLevel;

	// Use this for initialization
	void Start(){
		_curBlock = GridManager.Block.Floor;
		_curLevel = 0;

		ground = (GameObject)GameObject.Find("ent_ground");
		groundPlane = ground.GetComponent<Collider>();

		gridMgr = GameObject.Find("mgr_grid").GetComponent<GridManager>();

		cameraHolder = GameObject.Find("camera_holder");
	}

	void Update(){
		Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
		RaycastHit hit;

		Vector3 camPos = cameraHolder.transform.position;
		Transform camTransform = cameraHolder.transform;
		float speed = 100;
		float tSpeed = 0.1F;

#if UNITY_ANDROID
		if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Moved) {
			// Get movement of the finger since last frame
			Vector2 touchDeltaPosition = Input.GetTouch(0).deltaPosition;
			// Move object across XY plane
			camTransform.Translate(-touchDeltaPosition.x * tSpeed, -touchDeltaPosition.y * tSpeed, 0);
		}
#endif
#if UNITY_EDITOR || UNITY_WEBPLAYER
		if(Input.mousePosition.x > Screen.width - 100){
			camPos.x += 100 * Time.deltaTime;
		}else if (Input.mousePosition.x < 100){
			camPos.x -= 100 * Time.deltaTime;
		}
		
		if(Input.mousePosition.y > Screen.height - 100){
			camPos.z += 100 * Time.deltaTime;
		}else if (Input.mousePosition.y < 100){
			camPos.z -= 100 * Time.deltaTime;
		}
		
		cameraHolder.transform.position = camPos;
#endif

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
		if(Input.GetMouseButtonDown(0)&&
		   Input.mousePosition.x < Screen.width - 200){
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
