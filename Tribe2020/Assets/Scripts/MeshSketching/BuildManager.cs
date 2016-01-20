using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using SimpleJSON;

public class BuildManager : MonoBehaviour{
	//Singleton features
	private static BuildManager _instance;
	public static BuildManager GetInstance(){
		return _instance;
	}

//	public const string TOILET = "Toilet";
//	public const string CAMPFIRE = "Campfire";
//	public const string COFFEE = "Coffee";
//	public const string DOOR = "Door";
//	public const string WINDOW = "Window";
//	public const string STAIRS = "Stairs";

	public GameObject NODE, EDGE, ROOM;
	public List<GameObject> objects;
	private Dictionary<string, GameObject> _itemTable;

	public GameObject TOILET, CAMPFIRE, COFFEE, DOOR, WINDOW;
	public Transform graph;
	private int _recusionCount = 0;

	private BoundsOctree<GameObject> _nodeBounds;
	
	//Sort use instead of constructor
	void Awake(){
		Vector3 worldCenter = new Vector3(372.5f, 372.5f, 372.5f);
		_nodeBounds = new BoundsOctree<GameObject>(745, worldCenter, 1, 1.25f);

		_instance = this;
	}

	//Use this for initialization
	void Start(){
		_itemTable = new Dictionary<string, GameObject>();
		foreach(GameObject go in objects){
			_itemTable.Add(go.name, go);
		}
	}
	
	//Update is called once per frame
	void Update(){
	}

	//
	public void CreateRoom(string stringifiedRoom){
		var parse = JSON.Parse(stringifiedRoom);
		var ns = parse["nodes"];
		string type = parse["type"];

		List<Node> nodes = new List<Node>();
		GameObject firstNode = null;
		GameObject prevNode = null;
		
		for(int i = 0; i < ns.Count; i++){
			string nodeType = ns[i]["type"];
			Vector3 pos = new Vector3(
				ns[i]["pos"][0].AsFloat,
				ns[i]["pos"][1].AsFloat,
				ns[i]["pos"][2].AsFloat);

			GameObject curNode = null;
			if(nodeType == "Node"){
				curNode = AddNode(pos);
			} else{
				curNode = AddThing(pos, nodeType);
			}

			if(i == 0){
				firstNode = prevNode = curNode;
			} else{
				AddEdge(prevNode, curNode);
				prevNode = curNode;
			}
			nodes.Add(curNode.GetComponent<Node>());
		}
		AddEdge(prevNode, firstNode);
		AddRoom(nodes);
	}


	//
	public void CreateRoom(Vector3 pos, int size){
		GameObject ne = AddNode(pos + (Vector3.right + Vector3.forward) * 5 * size);
		GameObject nw = AddNode(pos + (Vector3.left + Vector3.forward) * 5 * size);
		GameObject se = AddNode(pos + (Vector3.right + Vector3.back) * 5 * size);
		GameObject sw = AddNode(pos + (Vector3.left + Vector3.back) * 5 * size);

		AddEdge(ne, nw);
		AddEdge(nw, sw);
		AddEdge(sw, se);
		AddEdge(se, ne);

		List<Node> nodes = new List<Node>();
		nodes.Add(ne.GetComponent<Node>());
		nodes.Add(nw.GetComponent<Node>());
		nodes.Add(sw.GetComponent<Node>());
		nodes.Add(se.GetComponent<Node>());
		AddRoom(nodes);
	}

	//For doors and windows
	public void CreateNodeObject(Vector3 pos, string objID){
	}

	//For objects not part of wall
	public void CreateBlockObject(Vector3 pos, string objID){
	}

	//Add new node without edges to room graph
	public GameObject AddNode(Vector3 pos){
		GameObject newNodeObj = Instantiate(NODE, pos, Quaternion.identity) as GameObject;
		Node newNode = newNodeObj.GetComponent<Node>();
		newNode.Init();

		newNodeObj.transform.SetParent(graph);
		return newNodeObj;
	}

	//Add new node as neighbour with existing node to room graph
	public GameObject AddNode(Vector3 pos, Node curNode){
		GameObject newNodeObj = Instantiate(NODE, pos, Quaternion.identity) as GameObject;
		Node newNode = newNodeObj.GetComponent<Node>();
		newNode.Init();

		AddEdge(curNode, newNode);

		newNodeObj.transform.SetParent(graph);
		return newNodeObj;
	}

	//
	public GameObject AddEdge(GameObject nodeObj1, GameObject nodeObj2){
		Node node1 = nodeObj1.GetComponent<Node>();
		Node node2 = nodeObj2.GetComponent<Node>();

		return AddEdge(node1, node2);
	}

	//Add new edge between two nodes to room graph
	public GameObject AddEdge(Node node1, Node node2){
		GameObject newEdgeObj = Instantiate(EDGE, Vector3.zero, Quaternion.identity) as GameObject;
		Edge newEdge = newEdgeObj.GetComponent<Edge>();
		newEdge.Init(node1, node2);

		node1.AddEdge(node2, newEdge);
		node2.AddEdge(node1, newEdge);

		newEdgeObj.transform.SetParent(graph);
		return newEdgeObj;
	}

	//Add new room surface defined by set of nodes to room graph
	public GameObject AddRoom(List<Node> nodes){
		GameObject newRoomObj = Instantiate(ROOM, Vector3.up, Quaternion.identity) as GameObject;
		Room newRoom = newRoomObj.GetComponent<Room>();
		newRoom.Init(nodes);

		foreach(Node n in nodes){
			n.AddRoom(newRoom);
		}

		newRoomObj.transform.SetParent(graph);
		return newRoomObj;
	}

	//Create new thing from string
	public GameObject AddThing(string thingParse){
		var parse = JSON.Parse(thingParse);
		
		string type = parse["type"];
		Vector3 pos = new Vector3(
			parse["pos"][0].AsFloat,
			parse["pos"][1].AsFloat,
			parse["pos"][2].AsFloat);
		
		return AddThing(pos, type);
	}

	//Create new thing for position and typ
	public GameObject AddThing(Vector3 pos, string type){
		GameObject thing = Instantiate(_itemTable[type], pos, Quaternion.identity) as GameObject;
		Node node = thing.GetComponent<Node>();
		node.Init();

		thing.transform.SetParent(graph);
		
		return thing;
	}

	//
	public void ConnectNodes(Node node1, Node node2){
		Room room = node1.GetRoom();
		if(room.ContainsNode(node2)){
			Debug.Log("Removed node " + node2.ToString());
			room.RemoveNode(node2);
		} else{
			node1.ConnectNode(node2);
			node2.ConnectNode(node1);
			Debug.Log("Connected node " + node1.ToString() + " and " + node2.ToString());
		}
	}

	//
	public void SplitEdge(Edge edge, Vector3 pos){
		GameObject newNodeObj = AddNode(pos);
		Node newNode = newNodeObj.GetComponent<Node>();

		SplitEdge(edge, pos, newNode);
	}

	//
	public void SplitEdge(Edge edge, Vector3 pos, Node node){
		AddEdge(node, edge.n1);
		AddEdge(node, edge.n2);
		
		Room room = edge.n1.GetRoom();
		room.AddNode(node, edge.n1, edge.n2);
		node.AddRoom(room);
		
		edge.Remove();
	}

	//
	public GameObject[] GetCollidingNode(Bounds b){
		GameObject[] result = _nodeBounds.GetColliding(b);

		return result;
	}

	//
	public void UpdateCollision(GameObject node){
		node.GetComponent<Node>().Refresh();
	}

	//
	public bool PathContainsPath(List<Node> big, List<Node> small){
		foreach(Node n in small) {
			if(!big.Contains(n)) {
				return false;
			}
		}
		return true;
	}

	//
	public float AngleFromNodeToNode(Node prev, Node next){
		Vector3 start = prev.gameObject.transform.position;
		Vector3 end = next.gameObject.transform.position;

		return Mathf.Atan2(end.z - start.z, end.x - start.x) * Mathf.Rad2Deg;
	}

	//
	public void RemoveBound(GameObject go){
		_nodeBounds.Remove(go);
	}

	//
	public void UpdateBound(GameObject go){
		_nodeBounds.Remove(go);
		_nodeBounds.Add(go, go.GetComponent<Collider>().bounds);
	}

	//
	public List<string> GetThings(){
		List<string> things = new List<string>();
		foreach(GameObject go in objects){
			things.Add(go.name);
		}
		return things;
	}
}
