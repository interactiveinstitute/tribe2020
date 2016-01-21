using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Node : MonoBehaviour{
	private static BuildManager _buildMgr;

	public string type;

//	public static int count = 0;

	public GameObject NODE, EDGE;
	private Dictionary<Node, Edge> _edges;
	private Edge _edge1, _edge2;
	public float _connectionDistance;
	private List<Node> _connectedNodes;
	private List<Room> _rooms;
	private bool _isInit = false;
	private int _id = -1;

	//
	public void Init(){
		if(!_isInit) {
			_buildMgr = BuildManager.GetInstance();
			_buildMgr.UpdateBound(gameObject);
			
			_edges = new Dictionary<Node, Edge>();
			_connectedNodes = new List<Node>();
			_rooms = new List<Room>();
			_isInit = true;
		}
	}

	// Use this for initialization
	void Start(){
		Init();

		if(type == "") {
			type = "Node";
		}
	}
	
	// Update is called once per frame
	void Update(){
		if(!_isInit){
			return;
		}

		if(_edges.Count == 2){
			List<Edge> es = new List<Edge>();
			
			foreach(Edge e in _edges.Values){
				es.Add(e);
			}

			if(_edge1 == null){ _edge1 = es[1]; }
			if(_edge2 == null){ _edge2 = es[0]; }
			
			Vector3 start = es[0].gameObject.transform.position;
			Vector3 end = es[1].gameObject.transform.position;
			
			float rotation = Mathf.Atan2(end.z - start.z, end.x - start.x) * Mathf.Rad2Deg;
			transform.eulerAngles = new Vector3(0, 360 - rotation, 0);
		}
//		Refresh();
	}

	//
	public void Refresh(){
		if(!_isInit){
			return;
		}

		Refresh(true);
	}

	//
	public void Refresh(bool isMasterNode){
		foreach(Edge e in _edges.Values){
			e.Refresh(this);
		}

		if(isMasterNode) {
			foreach(Node n in _connectedNodes) {
				n.transform.position = transform.position;
				n.Refresh(false);
			}
		}

		foreach(Room r in _rooms){
			r.Refresh(this);
		}

		_buildMgr.UpdateBound(gameObject);
	}

	//Adds an edge to a new node if this node is not already connected
	public void AddEdge(Node node, Edge edge){
		if(!_edges.ContainsKey(node)){
			_edges.Add(node, edge);
		}
	}

	//
	public void ConnectNode(Node node){
		if(!_connectedNodes.Contains(node)){
			_connectedNodes.Add(node);
		}
	}

	//
	public void AddRoom(Room room){
		_rooms.Add(room);
	}

	//
	public List<Node> GetNodes(){
		List<Node> nodes = new List<Node>();

		foreach(Node node in _edges.Keys) {
			nodes.Add(node);
		}

		return nodes;
	}

	//
	public List<Edge> GetEdges(){
		List<Edge> edges = new List<Edge>();
		
		foreach(Edge edge in _edges.Values) {
			edges.Add(edge);
		}
		
		return edges;
	}

	//
	public Edge GetEdge(Node node){
		return _edges[node];
	}

	//
	public List<Room> GetRooms(){
		return _rooms;
	}

	//
	public Room GetRoom(){
		return _rooms[0];
	}

	//
	public Vector3 GetAnchorPoint(Edge edge){
		float rot = Mathf.Deg2Rad * (360 - transform.eulerAngles.y);
		if(edge == _edge1){
			float x = transform.position.x + Mathf.Cos(rot) * _connectionDistance;
			float z = transform.position.z + Mathf.Sin(rot) * _connectionDistance;
			return new Vector3(x, transform.position.y, z);
		} else if(edge == _edge2){
			float x = transform.position.x + Mathf.Cos(rot + Mathf.PI) * _connectionDistance;
			float z = transform.position.z + Mathf.Sin(rot + Mathf.PI) * _connectionDistance;
			return new Vector3(x, transform.position.y, z);
		}

		return gameObject.transform.position;
	}

	//
	public void Remove(){
		List<Edge> removeEdges = new List<Edge>();

		foreach(Edge edge in _edges.Values){
			removeEdges.Add(edge);
		}

		foreach(Edge edge in removeEdges){
			edge.Remove();
		}

		_buildMgr.RemoveBound(gameObject);
		Destroy(gameObject);
	}

	//
	public void RemoveEdgeTo(Node node){
		_edges.Remove(node);
	}

	//
	public bool IsNotConnected(){
		return _edges.Count == 0;
	}

	//
	public string ToString(){
		return "[" + transform.position.x + "," + transform.position.z + "]";
	}

	//
	public string Stringify(){
		Vector3 pos = transform.position;
		return "{\"type\":\"" + type +"\", \"pos\":[" + pos.x + "," + pos.y + "," + pos.z + "]}";
//		return "{\"pos\":[" + pos.x + "," + pos.y + "," + pos.z + "]}";
	}
}
