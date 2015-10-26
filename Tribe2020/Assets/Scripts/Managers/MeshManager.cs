using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MeshManager : MonoBehaviour{
	public GameObject WALL, CAMPFIRE, COFFEE_MACHINE, TOILET;
	private enum ALIGNMENT { X = 0, Z = 1, SAME = 3 };
//	private List<GameObject> _meshes;
	public Transform map;

	private BoundsOctree<GameObject> _meshes2;
	private List<GameObject> _removeMeshes;

	private int _recursionCount = 0;
	
	// Use this for initialization
	void Start(){
//		WALL = GameObject.Find("Block Wall");
//		CAMPFIRE = GameObject.Find("Block Campfire");
//		COFFEE_MACHINE = GameObject.Find ("Block Coffee Machine");
//		TOILET = GameObject.Find ("Block Toilet");
//		_meshes = new List<GameObject>();
		Vector3 center = new Vector3(372.5f, 372.5f, 372.5f);
		_meshes2 = new BoundsOctree<GameObject>(745, center, 1, 1.25f);
		_removeMeshes = new List<GameObject>();
	}
	
	// Update is called once per frame
	void Update(){
	}

	//
	public void CreateRoom(Vector3 start, Vector3 end, ESManager.Block type){
		AddMesh(start, new Vector3(start.x, start.y, end.z), type);
		AddMesh(start, new Vector3(end.x, start.y, start.z), type);
		AddMesh(end, new Vector3(start.x, start.y, end.z), type);
		AddMesh(end, new Vector3(end.x, start.y, start.z), type);
	}

	//
	public void AddMesh(Vector3 pos, ESManager.Block type){
		GameObject obj = TypeToObject(type);
		GameObject newMesh = Instantiate(obj, pos, Quaternion.identity) as GameObject;
		newMesh.transform.SetParent(map);
	}

	//
	public void AddMesh(GameObject go){
		GameObject newMesh = Instantiate(WALL, Vector3.zero, Quaternion.identity) as GameObject;
//		GameObject stencil = so.ToGameObject();

//		GameObject newMesh =
//			Instantiate(WALL, go.transform.position, go.transform.rotation) as GameObject;

		newMesh.transform.position = go.transform.position;
		newMesh.transform.rotation = go.transform.rotation;
		newMesh.transform.localScale = go.transform.localScale;
//		newMesh.tag = so.GetTag();
//		go.transform.SetParent(map);
//		GameObject newMesh =
//			Instantiate(go, go.transform.position, go.transform.rotation) as GameObject;
		newMesh.transform.SetParent(map);
	}

	//
	public void AddMesh(Vector3 start, Vector3 end, ESManager.Block type){
		FlushMeshes();

		if(_recursionCount > 6) {
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

		_recursionCount = 0;
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

			AddMesh(ends1[0], ends2[0], ESManager.Block.Wall);
			AddMesh(ends2[0], ends1[1], ESManager.Block.Wall);
			AddMesh(ends1[1], ends2[1], ESManager.Block.Wall);
		} else {
			// Crossing
			Vector3 crossing = GetCrossing(m1, m2);
			Vector3[] ends1 = GetEnds(m1);
			Vector3[] ends2 = GetEnds(m2);

			_removeMeshes.Add(m1);
			_removeMeshes.Add(m2);

			AddMesh(crossing, ends1[0], type);
			AddMesh(crossing, ends1[1], type);
			AddMesh(crossing, ends2[0], type);
			AddMesh(crossing, ends2[1], type);
		}
	}

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
			return CAMPFIRE;
		case ESManager.Block.Coffee:
			return COFFEE_MACHINE;
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
