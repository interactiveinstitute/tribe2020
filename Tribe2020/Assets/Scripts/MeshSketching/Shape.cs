using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using PrimitivesPro.GameObjects;
using PrimitivesPro.Primitives;

public class Shape : MonoBehaviour{
	private GameObject WALL;

	public const int RECTANGLE = 0, CIRCLE = 1;
	public float _radius = 3;
	private int _segments = 20;
	private float _segLength = 10;
	private List<GameObject> _meshes = new List<GameObject>();
//	private int _shape = Shape.RECTANGLE;

	private float _oldRad = 0;

//	private float[] shapeParamsStart;
//	private float[] shapeParamsMax;
	private PrimitivesPro.GameObjects.BaseObject _shape;

//	private Box _boxShape;
//	private Tube _tubeShape;
//
//	private BoxPrimitive _box;
//	private TubePrimitive _tube;

	// Use this for initialization
	void Start(){
		WALL = Resources.Load("Wall Segment") as GameObject;

//		_boxShape = gameObject.GetComponent<Box>();
//		_tubeShape = gameObject.GetComponent<Tube>();
//
//		_box = gameObject.GetComponent<BoxPrimitive>();
//		_tube = gameObject.GetComponent<TubePrimitive>();
	}

	// Update is called once per frame
	void Update(){
//		if(_shape == CIRCLE && _radius != _oldRad){
//			UpdateShape();
//			_oldRad = _radius;
//		}
	}

	public void UpdateShape(){
//		ClearMesh();
//		
//		_segments = (int) Mathf.Max(10, Mathf.Ceil(_radius));
//		_segLength = Mathf.PI * 2f * _radius / _segments + 0.5f;
//		
//		Debug.Log(_segments);
//		
//		for(int i = 0; i < _segments; i++){
//			_meshes.Add(GameObject.CreatePrimitive(PrimitiveType.Cube));
//			float angle = 2f * Mathf.PI / _segments * i;
//			float xPos = Mathf.Cos(angle);
//			float zPos = Mathf.Sin(angle);
//			_meshes[i].transform.localScale = new Vector3(2f, 10f, _segLength);
//			_meshes[i].transform.position =
//				transform.position + new Vector3(xPos * _radius, 0f, zPos * _radius);
//			//			float rotation = Vector3.Angle(transform.position, _meshes[i].transform.position);
//			_meshes[i].transform.Rotate(0f, 360f - 360f / _segments * i, 0f);
//			_meshes[i].transform.parent = transform;
//		}
	}

	public void SetRectangle(){
//		_shape = Box.Create(
//			1f, 1f, 1f, 1, 1, 1, false, null, PivotPosition.Botttom);
//		shapeParamsMax = new float[] {2.5f, 2.5f, 2.5f, 1, 1, 1};
//		shapeParamsStart = new float[] {1f, 1f, 1f, 1, 1, 1};
//		MeshFilter meshFilter = gameObject.GetComponent<MeshFilter>();
//		meshFilter.mesh = PrimitivesPro.Primitives.BoxPrimitive.
//		_boxShape.enabled = true;
//		_boxShape.SetHeight(100);

		float width = 1f;
		float height = 1f;
		float depth = 1f;
		int widthSegments = 1;
		int heightSegments = 1;
		int depthSegments = 1;
		bool cubeMap = false;
		float[] edgeOffset = null;
		PivotPosition pivot = PivotPosition.Botttom;

//		var planeObject = new GameObject("BoxPro");
		
//		planeObject.AddComponent<MeshFilter>();
		var renderer = gameObject.AddComponent<MeshRenderer>();
		
		renderer.sharedMaterial = new Material(Shader.Find("Diffuse"));
		
		if (edgeOffset == null || edgeOffset.Length != 4)
		{
			edgeOffset = new float[4];
		}
		
		var cube = gameObject.AddComponent<Box>();
		cube.GenerateGeometry(width, height, depth, widthSegments, heightSegments, depthSegments, cubeMap, edgeOffset, pivot);
		
//		return cube;
	}

	public void SetTube(){
		bool flatNormals = false;

//		_shape = Tube.Create(
//			0.8f, 1f, 1f, 3, 1, 0.0f, false,
//			flatNormals ? NormalsType.Face : NormalsType.Vertex, PivotPosition.Botttom);

//		float radius0 = 0.8f;
//		float radius1 = 1f;
//		int torusSegments = 1f;
//		int coneSegments = 3;
//		float slice = 1;
//		NormalsType normalsType = NormalsType.Face;
//		PivotPosition pivotPosition = PivotPosition.Botttom

		float radius0 = 0.8f;
		float radius1 = 1f;
		float height = 1f;
		int sides = 20;
		int heightSegments = 1;
		float slice = 0.0f;
		bool radialMapping = false;
		NormalsType normalsType = flatNormals ? NormalsType.Face : NormalsType.Vertex;
		PivotPosition pivotPosition = PivotPosition.Botttom;


//		var cylinderObject = new GameObject("TubePro");
		
//		cylinderObject.AddComponent<MeshFilter>();
		var renderer = gameObject.AddComponent<MeshRenderer>();
		
		renderer.sharedMaterial = new Material(Shader.Find("Diffuse"));
		
		var tube = gameObject.AddComponent<Tube>();
		tube.GenerateGeometry(radius0, radius1, height, sides, heightSegments, slice, radialMapping, normalsType, pivotPosition);
		
//		return tube;
	}

//	public void SetRectangle(Vector3 start, Vector3 end){
//		ClearMesh();
//
////		GameObject obj = TypeToObject(type);
//		Vector3 center = start + (end - start) / 2;
//		Vector3 scale = new Vector3();
//		scale.x = Mathf.Max(start.x, end.x) - Mathf.Min(start.x, end.x) + 2;
//		scale.y = 10;
//		scale.z = Mathf.Max(start.z, end.z) - Mathf.Min(start.z, end.z) + 2;
//
//		GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
////		Instantiate(WALL, Vector3.zero, Quaternion.identity) as GameObject;
//		wall.transform.SetParent(transform);
//
//		transform.position = center;
//		transform.localScale = scale;
//
//		_meshes.Add(wall);
//
////		transform.localScale = TypeToScale(WALL, start.x, end.x, start.z, end.z);
//	}

//	public void SetRectangle(Vector3 center, int direction, int length, ESManager.Block type){
//		ClearMesh();
//
//		transform.position = center;
//	}

//	public void SetCircle(Vector3 center, float radius){
//		ClearMesh();
//
//		transform.position = center;
//		_radius = radius;
//
//		UpdateShape();
//	}

//	public void SetShape(int shape){
//		_shape = shape;
//	}

//	public void ClearMesh(){
//		foreach(GameObject go in _meshes) {
//			Destroy(go);
//		}
//		_meshes.Clear();
//	}

//	private GameObject TypeToObject(ESManager.Block type){
//		switch(type){
//		case ESManager.Block.Floor:
//			return WALL;
////		case ESManager.Block.Campfire:
////			return CAMPFIRE;
////		case ESManager.Block.Coffee:
////			return COFFEE_MACHINE;
////		case ESManager.Block.Toilet:
////			return TOILET;
//		default:
//			return WALL;
//		}
//	}

	private Vector3 TypeToScale(ESManager.Block type, float x1, float x2, float z1, float z2){
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
}
