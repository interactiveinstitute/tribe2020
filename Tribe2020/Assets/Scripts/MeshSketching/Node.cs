using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Node : MonoBehaviour{
	private static BuildManager _buildMgr;

//	public static int count = 0;

	public GameObject NODE, EDGE;
	private Dictionary<Node, Edge> _edges;
	private List<Node> _connectedNodes;
	private List<Room> _rooms;
	private bool _isInit = false;
	private int _id = -1;

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

//	public void Init(BuildManager buildMgr){
//		if(!_isInit) {
//			_buildMgr = buildMgr;
//
//			_edges = new Dictionary<Node, Edge>();
//			_connectedNodes = new List<Node>();
//			_rooms = new List<Room>();
//			_isInit = true;
//		}
//	}

	// Use this for initialization
	void Start(){
		Init();
	}
	
	// Update is called once per frame
	void Update(){
	
	}

	//
	public void Refresh(){
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

	//
	public void UpdateId(int removedID){
		if(_id > removedID) {
			_id--;
		}
	}

	//
	public int GetID(){
		return _id;
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
//		Init();

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

	public Room GetRoom(){
		return _rooms[0];
	}

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

	public void RemoveEdgeTo(Node node){
		_edges.Remove(node);
	}

//	public void RemoveEdge(Edge edge){
//		_edges.Remove(Node);
//	}

//	public void UpdateEdgeBounds(){
//	}

	public string ToString(){
		return "[" + transform.position.x / 5 + "," + transform.position.z / 5 + "]";
	}
}
