using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using System;
using PrimitivesPro.MeshCutting;
using Plane = PrimitivesPro.Utils.Plane;

public class MeshManager : MonoBehaviour{
	private static MeshManager _instance;

	private GameObject SHAPE, WALL, FIRE, COFFEE, TOILET;
	private enum ALIGNMENT { X = 0, Z = 1, SAME = 3 };

//	private List<GameObject> _meshes;
	public Transform map;

	private BoundsOctree<GameObject> _meshes2;
	private BoundsOctree<GameObject> _meshes3;
	private List<GameObject> _removeMeshes;

	private int _recursionCount = 0;

	//PrimitivesPRO
	public GameObject[] OriginalObjects = null;
	
	private GameObject OriginalObject;
	private int objectIdx;
//	private GameObject cut0, cut1;
//	private Plane plane;
//	private float GenerationTimeMS;
	
	private int success = 0;
	private float minTime = float.MaxValue;
	private float maxTime = float.MinValue;
	private float sumTime = 0.0f;
	private int sumSteps;
	private float cutTimeout;
	private bool triangulate = true;
	private Vector3[] tweenAmount = new Vector3[2];
	private float targetTweenTimeout;
	
	private MeshCutter cutter;

	GameObject cube;
	bool done = false;

	public static MeshManager GetInstance(){
		if(_instance != null) {
			return _instance;
		}

		return null;
	}
	
	// Use this for initialization
	void Start(){
		_instance = this;

		SHAPE = Resources.Load("Shape") as GameObject;
		WALL = Resources.Load("Wall Segment") as GameObject;
		FIRE = Resources.Load("Block Campfire") as GameObject;
		COFFEE = Resources.Load("Block Coffee Machine") as GameObject;
		TOILET = Resources.Load("Block Toilet") as GameObject;
//		WALL = GameObject.Find("Block Wall");
//		CAMPFIRE = GameObject.Find("Block Campfire");
//		COFFEE_MACHINE = GameObject.Find ("Block Coffee Machine");
//		TOILET = GameObject.Find ("Block Toilet");
//		_meshes = new List<GameObject>();
		Vector3 center = new Vector3(372.5f, 372.5f, 372.5f);
		_meshes2 = new BoundsOctree<GameObject>(745, center, 1, 1.25f);
		_removeMeshes = new List<GameObject>();

		cutter = new MeshCutter();

//		GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
//		GameObject cube = GameObject.FindGameObjectWithTag("boxPro") as GameObject;
//		Plane plane = new Plane(new Vector3(1, 0, 0), Vector3.zero);

//		cube = Instantiate(SHAPE, Vector3.zero, Quaternion.identity) as GameObject;

//		Cut(cube, Vector3.right);
	}
	
	// Update is called once per frame
	void Update(){

//		if(!done) {
//			Shape shape = cube.GetComponent<Shape>();
//			shape.SetTube();
//		
//			Cut(cube, Vector3.forward);
//			done = true;
//		}
	}

	//
	public void CreateRoom(Vector3 start, Vector3 end){
//		AddWall(start, new Vector3(start.x, start.y, end.z));
//		AddWall(start, new Vector3(end.x, start.y, start.z));
//		AddWall(end, new Vector3(start.x, start.y, end.z));
//		AddWall(end, new Vector3(end.x, start.y, start.z));

//		AddMesh(start, new Vector3(start.x, start.y, end.z), type);
//		AddMesh(start, new Vector3(end.x, start.y, start.z), type);
//		AddMesh(end, new Vector3(start.x, start.y, end.z), type);
//		AddMesh(end, new Vector3(end.x, start.y, start.z), type);
	}

//	public void CreateCircleRoom(Vector3 center, float radius){
//		GameObject wall = Instantiate(WALL, Vector3.zero, Quaternion.identity) as GameObject;
//		Shape wallMesh = wall.GetComponent<Shape>();
//		wallMesh.SetCircle(center, radius);
//
//		wall.transform.SetParent(map);
//	}

//	public void AddWall(Vector3 start, Vector3 end){
//		GameObject wall = Instantiate(WALL, Vector3.zero, Quaternion.identity) as GameObject;
//		Shape wallMesh = wall.GetComponent<Shape>();
//		wallMesh.SetRectangle(start, end);
//
//		AddMesh(wall);
////		RecursionAddMesh(wall);
//
////		wall.transform.SetParent(map);
//	}

	//
	public void AddMesh(Vector3 pos, ESManager.Block type){
		GameObject obj = TypeToObject(type);
		GameObject newMesh = Instantiate(obj, pos, Quaternion.identity) as GameObject;
		newMesh.transform.SetParent(map);
	}

	//
	public void AddMesh(GameObject mesh){
//		GameObject newMesh = Instantiate(WALL, Vector3.zero, Quaternion.identity) as GameObject;
//
//		newMesh.transform.position = go.transform.position;
//		newMesh.transform.rotation = go.transform.rotation;
//		newMesh.transform.localScale = go.transform.localScale;
//
//		newMesh.transform.SetParent(map);
		_recursionCount = 0;

		RecursionAddMesh(mesh);
	}

	public void AddMesh(Vector3 start, Vector3 end, ESManager.Block type){
		_recursionCount = 0;
		RecursionAddMesh(start, end, type);
	}

	//
	public void RecursionAddMesh(Vector3 start, Vector3 end, ESManager.Block type){
		FlushMeshes();

		if(_recursionCount > 30){
			return;
		} else {
			_recursionCount++;
		}

		GameObject obj = TypeToObject(type);
		Vector3 pos = start + (end - start) / 2;
		GameObject newMesh = Instantiate(obj, pos, Quaternion.identity) as GameObject;
		newMesh.transform.SetParent(map);
		
		newMesh.transform.localScale = TypeToScale(type, start.x, end.x, start.z, end.z);
		Bounds newBounds = newMesh.GetComponent<Collider>().bounds;

		GameObject[] collidingBoxes = _meshes2.GetColliding(newBounds);
		
		if(collidingBoxes.Length == 0){
			_meshes2.Add(newMesh, newBounds);
		} else {
			foreach(GameObject go in collidingBoxes){
				CutIntersections(newMesh, go, type);
			}
		}
	}

	public void RecursionAddMesh(GameObject mesh){
		FlushMeshes();
		
		if(_recursionCount > 30){
			return;
		} else {
			_recursionCount++;
		}
		
//		GameObject obj = TypeToObject(type);
//		Vector3 pos = start + (end - start) / 2;
//		GameObject newMesh = Instantiate(obj, pos, Quaternion.identity) as GameObject;
		mesh.transform.SetParent(map);
		
//		newMesh.transform.localScale = TypeToScale(type, start.x, end.x, start.z, end.z);
		Bounds newBounds = mesh.GetComponentInChildren<Collider>().bounds;
		
		GameObject[] collidingBoxes = _meshes2.GetColliding(newBounds);
		
		if(collidingBoxes.Length == 0){
			_meshes2.Add(mesh, newBounds);
		} else {
			foreach(GameObject go in collidingBoxes){
//				CutIntersections(mesh, go);
			}
		}
	}

	//
	private void CutIntersections(GameObject m1, GameObject m2, ESManager.Block type){
		if(m1.transform.localScale == m2.transform.localScale) {
			// Same
			DestroyMesh(m1);
		} else if(GetAlignment(m1) == GetAlignment(m2)) {
			// Parallel
			Vector3[] ends1 = GetEnds(m1);
			Vector3[] ends2 = GetEnds(m2);

			_removeMeshes.Add(m1);
			_removeMeshes.Add(m2);

			RecursionAddMesh(ends1[0], ends2[0], ESManager.Block.Wall);
			RecursionAddMesh(ends2[0], ends1[1], ESManager.Block.Wall);
			RecursionAddMesh(ends1[1], ends2[1], ESManager.Block.Wall);
		} else {
			// Crossing
			Vector3 crossing = GetCrossing(m1, m2);
			Vector3[] ends1 = GetEnds(m1);
			Vector3[] ends2 = GetEnds(m2);

			_removeMeshes.Add(m1);
			_removeMeshes.Add(m2);

			RecursionAddMesh(crossing, ends1[0], type);
			RecursionAddMesh(crossing, ends1[1], type);
			RecursionAddMesh(crossing, ends2[0], type);
			RecursionAddMesh(crossing, ends2[1], type);
		}
	}

	private void AddWall(GameObject mesh){
		
	}

	private void AddCircleWall(GameObject mesh){
		
	}

	private void Cut(GameObject mesh, Vector3 planeOrientation){
		GameObject cut0, cut1;
		Plane plane = new Plane(planeOrientation, Vector3.zero);
		
		try{
			ContourData contour;

			Vector4 dcs = MeshCutter.defaultCrossSection;
			float time = cutter.Cut(mesh, plane, true, true, dcs, out cut0, out cut1, out contour);
			
			success++;
			
			if(time > maxTime){
				maxTime = time;
			}
			
			if(time < minTime){
				minTime = time;
			}
			
			sumTime += time;
			sumSteps++;
		} catch(Exception){
			Debug.Log("Cutter exception!");
			return;
		}
		
		if(cut0 != null){
			tweenAmount[0] = plane.Normal*0.02f;
			tweenAmount[1] = plane.Normal*-0.02f;
			targetTweenTimeout = 0.5f;
			cutTimeout = 0.5f;
		}

		cut0.AddComponent<Shape>();
		cut0.AddComponent<NavMeshObstacle>();

		cut1.AddComponent<Shape>();
		cut1.AddComponent<NavMeshObstacle>();
	}

	//
//	private void CutIntersections(GameObject m1, GameObject m2){
//		if(m1.transform.localScale == m2.transform.localScale) {
//			// Same
//			DestroyMesh(m1);
//		} else if(GetAlignment(m1) == GetAlignment(m2)) {
//			// Parallel
//			Vector3[] ends1 = GetEnds(m1);
//			Vector3[] ends2 = GetEnds(m2);
//			
//			_removeMeshes.Add(m1);
//			_removeMeshes.Add(m2);
//			
//			RecursionAddMesh(ends1[0], ends2[0], ESManager.Block.Wall);
//			RecursionAddMesh(ends2[0], ends1[1], ESManager.Block.Wall);
//			RecursionAddMesh(ends1[1], ends2[1], ESManager.Block.Wall);
//		} else {
//			// Crossing
//			Vector3 crossing = GetCrossing(m1, m2);
//			Vector3[] ends1 = GetEnds(m1);
//			Vector3[] ends2 = GetEnds(m2);
//			
//			_removeMeshes.Add(m1);
//			_removeMeshes.Add(m2);
//			
//			RecursionAddMesh(crossing, ends1[0]);
//			RecursionAddMesh(crossing, ends1[1]);
//			RecursionAddMesh(crossing, ends2[0]);
//			RecursionAddMesh(crossing, ends2[1]);
//		}
//	}

	//
	private ALIGNMENT GetAlignment(GameObject go){
		Vector3 scale = go.transform.localScale;

		if(scale.x > scale.z){
			return ALIGNMENT.X;
		} else if(scale.z > scale.x){
			return ALIGNMENT.Z;
		}

		return ALIGNMENT.SAME;
	}

	//
	private Vector3[] GetEnds(GameObject mesh){
		Vector3 end1 = mesh.transform.position;
		Vector3 end2 = mesh.transform.position;
		ALIGNMENT align = GetAlignment(mesh);
		float size = 0f;

		if(align == ALIGNMENT.X){
			size = mesh.transform.localScale.x;
			end1 = end1 + Vector3.right * (size / 2 - 1);
			end2 = end2 + Vector3.left * (size / 2 - 1);
		} else if(align == ALIGNMENT.Z){
			size = mesh.transform.localScale.z;
			end1 = end1 + Vector3.forward * (size / 2 - 1);
			end2 = end2 + Vector3.back * (size / 2 - 1);
		}

		Vector3[] poles = {end1, end2};
		return poles;
	}

	//
	private Vector3 GetCrossing(GameObject m1, GameObject m2){
		float cx = 0;
		float cz = 0;

		if(GetAlignment(m1) == ALIGNMENT.Z) {
			cx = m1.transform.position.x;
		} else {
			cx = m2.transform.position.x;
		}
		
		if(GetAlignment(m1) == ALIGNMENT.X){
			cz = m1.transform.position.z;
		} else {
			cz = m2.transform.position.z;
		}

		return new Vector3(cx, m1.transform.position.y, cz);
	}

	//
	public GameObject CollidesWithBlock(GameObject otherObj){
//		Collider col = otherObj.GetComponent<Collider>().bounds;
		Bounds pb = otherObj.GetComponent<Collider>().bounds;

		GameObject[] result2 = _meshes2.GetColliding(pb);

		if(result2.Length > 0)
			return result2[0];

//		Debug.Log(result2.Length);

//		foreach(GameObject b in _meshes){
//			Bounds bb = b.GetComponent<Collider>().bounds;
//			if(pb.Intersects(bb)){
//				return b;
//			}
//		}
		return null;
	}

	//
	public void FlushMeshes(){
		foreach(GameObject go in _removeMeshes){
			Debug.Log("remove: " + go);
			DestroyMesh(go);
		}
		_removeMeshes.Clear();
	}

	//
	public void DestroyMesh(GameObject mesh){
		_meshes2.Remove(mesh);
		Destroy(mesh);
		//		Vector3 pos = obj.transform.position / 5;
		//		
		//		cells[(int)pos.x, (int)pos.y, (int)pos.z].GetComponent<Cell>().Reset();
		//	}
	}

	//
	private GameObject TypeToObject(ESManager.Block type){
		switch(type){
		case ESManager.Block.Floor:
			return WALL;
		case ESManager.Block.Campfire:
			return FIRE;
		case ESManager.Block.Coffee:
			return COFFEE;
		case ESManager.Block.Toilet:
			return TOILET;
		default:
			return WALL;
		}
	}

	//
	private Vector3 TypeToScale(
		ESManager.Block type, float x1, float x2, float z1, float z2){
		Vector3 scale = new Vector3();
		switch(type){
		case ESManager.Block.Wall:
			scale.x = Mathf.Max(x1, x2) - Mathf.Min(x1, x2) + 2;
			scale.y = 10;
			scale.z = Mathf.Max(z1, z2) - Mathf.Min(z1, z2) + 2;
			break;
		case ESManager.Block.Campfire:
		case ESManager.Block.Coffee:
		case ESManager.Block.Toilet:
		default:
			scale.x = Mathf.Max(x1, x2) - Mathf.Min(x1, x2) + 5;
			scale.y = 5;
			scale.z = Mathf.Max(z1, z2) - Mathf.Min(z1, z2) + 5;
			break;
		}

		return scale;
	}

//	public void CreateShape(int[] coords, Vector2 pos, string tag){
//		GameObject newShape = Instantiate(SHAPE, pos, Quaternion.identity) as GameObject;
//		
//		float minX = Mathf.Infinity;
//		float minY = Mathf.Infinity;
//		
//		List<Vector2> vertices2D = new List<Vector2>();
//		
//		for(int i = 0; i < coords.Length; i += 2){
//			vertices2D.Add(new Vector2(coords[i], coords[i + 1]));
//			minX = Mathf.Min(minX, coords[i]);
//			minY = Mathf.Min(minY, coords[i + 1]);
//		}
//		
//		foreach(Vector2 v in vertices2D){
//			v.Set(v.x - minX, v.y - minY);
//		}
//		
//		// Use the triangulator to get indices for creating triangles
//		Triangulator tr = new Triangulator(vertices2D.ToArray());
//		int[] indices = tr.Triangulate();
//		
//		// Create the Vector3 vertices
//		Vector3[] vertices = new Vector3[vertices2D.Count];
//		for(int i = 0; i < vertices.Length; i++){
//			vertices[i] = new Vector3(vertices2D[i].x, vertices2D[i].y, 0);
//		}
//		
//		// Create the mesh
//		Mesh msh = new Mesh();
//		msh.vertices = vertices;
//		msh.triangles = indices;
//		msh.RecalculateNormals();
//		msh.RecalculateBounds();
//		
//		// Set up game object with mesh;
//		newShape.AddComponent(typeof(MeshRenderer));
//		MeshFilter filter = newShape.AddComponent(typeof(MeshFilter)) as MeshFilter;
//		filter.mesh = msh;
//		
//		MeshCollider collider = newShape.AddComponent(typeof(MeshCollider)) as MeshCollider;
//		collider.sharedMesh = msh;
//		
//		newShape.transform.localScale = new Vector3(0.001f, -0.001f, 1f);
//		
////		Shape shape = newShape.GetComponent<Shape>();
////		shape.pushTag = tag;
//	}
}
