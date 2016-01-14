using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Room : MonoBehaviour {
	private static BuildManager _buildMgr;

	private Material _material;
	private List<Color> _colors;

	private List<Node> _nodes;
	private bool _isInit = false;

	//
	public void Init(){
		Init(new List<Node>());
	}

	//
	public void Init(List<Node> nodes){
		if(!_isInit) {
			_buildMgr = BuildManager.GetInstance();

			_nodes = nodes;

			_material = Resources.Load("BaseMaterial") as Material;
			_colors = new List<Color>();
			_colors.Add(Color.black);
			_colors.Add(Color.blue);
			_colors.Add(Color.cyan);
			_colors.Add(Color.green);
			_colors.Add(Color.magenta);
			_colors.Add(Color.red);
			_colors.Add(Color.white);
			_colors.Add(Color.yellow);

			CreateShape();

			_isInit = true;
		}
	}

	//Use this for initialization
	void Start(){
	}
	
	//Update is called once per frame
	void Update(){
	
	}

	//
	public void Refresh(Node n){
		UpdateMesh();
	}

	//
	public bool Equals(Room other){
		return Equals(other.GetNodes());
	}

	//
	public bool Equals(List<Node> otherList){
		if(_nodes.Count != otherList.Count){
			return false;
		}
		
		foreach(Node node in _nodes){
			if(!otherList.Contains(node)){
				return false;
			}
		}
		
		return true;
	}

	//
	public List<Node> GetNodes(){
		return _nodes;
	}

	//
	public Vector3 GetPosition(){
		float minX = Mathf.Infinity;
		float minZ = Mathf.Infinity;
		float maxX = -Mathf.Infinity;
		float maxZ = -Mathf.Infinity;

		foreach(Node node in _nodes){
			minX = Mathf.Min(minX, node.transform.position.x);
			minZ = Mathf.Min(minZ, node.transform.position.z);
			maxX = Mathf.Max(maxX, node.transform.position.x);
			maxZ = Mathf.Max(maxZ, node.transform.position.z);
		}

		Vector3 center = Vector3.zero;
		center.x = minX + (maxX - minX) / 2;
		center.z = minZ + (maxZ - minZ) / 2;

		center.y = 1;

		return center;
	}

	//Setup object with needed components and run a first mesh creation
	public void CreateShape(){
		// Set up game object with mesh;
		MeshRenderer render = gameObject.AddComponent(typeof(MeshRenderer)) as MeshRenderer;
		MeshFilter filter = gameObject.AddComponent(typeof(MeshFilter)) as MeshFilter;
		MeshCollider collider = gameObject.AddComponent(typeof(MeshCollider)) as MeshCollider;

		//Pick random color
		render.material = _material;
		render.material.shader = Shader.Find("Specular");
		render.material.SetColor("_Color", _colors[Random.Range(0, _colors.Count)]);

		//Correct rotation from weird Unity 2D to 3D coordinates
		gameObject.transform.eulerAngles = new Vector3(90, 0, 0);

		//Draw mesh
		UpdateMesh();
	}

	//Update mesh coordinates in case nodes have changed
	public void UpdateMesh(){
		float minX = Mathf.Infinity;
		float minY = Mathf.Infinity;
		
		List<Vector2> vertices2D = new List<Vector2>();

		float alt = -1 * _nodes[0].transform.position.y + 2.5f;
		foreach(Node node in _nodes){
			vertices2D.Add(new Vector2(node.transform.position.x, node.transform.position.z));
			minX = Mathf.Min(minX, node.transform.position.x);
			minY = Mathf.Min(minY, node.transform.position.z);
		}
		
		for(int i = 0; i < vertices2D.Count; i++){
			vertices2D[i].Set(vertices2D[i].x - minX, vertices2D[i].y - minY);
		}
		
		// Use the triangulator to get indices for creating triangles
		Triangulator tr = new Triangulator(vertices2D.ToArray());
		int[] indices = tr.Triangulate();
		
		// Create the Vector3 vertices
		Vector3[] vertices = new Vector3[vertices2D.Count];
		for(int i = 0; i < vertices.Length; i++){
			vertices[i] = new Vector3(vertices2D[i].x, vertices2D[i].y, alt); 
		}
		
		// Create the mesh
		Mesh msh = new Mesh();
		msh.vertices = vertices;
		msh.triangles = indices;
		msh.RecalculateNormals();
		msh.RecalculateBounds();
		
		// Set up game object with mesh;
		MeshFilter filter = gameObject.GetComponent<MeshFilter>();
		filter.mesh = msh;

		//Update collider with new mesh
		MeshCollider collider = gameObject.GetComponent<MeshCollider>();
		collider.sharedMesh = msh;
	}

	//
	public void AddNode(Node newNode, Node after, Node before){
		if(!_nodes.Contains(newNode)){
			int indexAfter = _nodes.IndexOf(after);
			int indexBefore = _nodes.IndexOf(before);

			int index = Mathf.Max(indexAfter, indexBefore);
			if(index == _nodes.Count - 1 && Mathf.Min(indexAfter, indexBefore) == 0){
				index = 0;
			}
			_nodes.Insert(index, newNode);
		}

		if(_isInit){
			UpdateMesh();
		}
	}

	//
	public void RemoveNode(Node node){
		if(_nodes.Count > 3){
			Node after = node.GetNodes()[0];
			Node before = node.GetNodes()[1];
			_buildMgr.AddEdge(after, before);

			_nodes.Remove(node);
			node.Remove();
		} else{
			Remove();
		}

		UpdateMesh();
	}

	//
	public void UpdateNodes(Node newNode, Node removedNode){
		if(!_nodes.Contains(newNode)){
			_nodes.Add(newNode);
		}
		_nodes.Remove(removedNode);

		if(_isInit){
			UpdateMesh();
		}
	}

	//
	public bool ContainsNode(Node node){
		return _nodes.Contains(node);
	}

	//
	public void Remove(){
		foreach(Node node in _nodes){
			node.Remove();
		}

		Destroy(gameObject);
	}

	//
	public string Stringify(){
		string nodes = "";
		foreach(Node n in _nodes) {
			nodes += n.Stringify() + ",";
		}
		nodes = nodes.Substring(0, nodes.Length - 2);
		return "{\"type\":\"room\", \"nodes\":[" + nodes + "]}";
	}
}
