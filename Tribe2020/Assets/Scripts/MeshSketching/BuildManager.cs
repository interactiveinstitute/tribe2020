using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BuildManager : MonoBehaviour{
//	private ShapeManager _shapeMgr;
//	private static BuildManager _instance;
	private static BuildManager _instance;

//	public InputManager _inputMgr;
	public GameObject NODE, EDGE, ROOM;
	public Transform graph;

	private int _recusionCount = 0;

	private BoundsOctree<GameObject> _nodeBounds;
//	, _edgeBounds;
//	private List<Node> _nodes;
//	private List<Edge> _edges;
//	private List<Room> _rooms;

//	public static BuildManager GetInstance(){
//		if(_instance == null) {
//			_instance = this;
//		}
//
//		return _instance;
//	}

	public static BuildManager GetInstance(){
		return _instance;
	}

	//
	void Awake(){
		Vector3 worldCenter = new Vector3(372.5f, 372.5f, 372.5f);
		_nodeBounds = new BoundsOctree<GameObject>(745, worldCenter, 1, 1.25f);

		_instance = this;
	}

	// Use this for initialization
	void Start(){
//		_shapeMgr = GameObject.FindWithTag("managers").GetComponent<ShapeManager>();


//		_edgeBounds = new BoundsOctree<GameObject>(745, worldCenter, 1, 1.25f);
//		_nodes = new List<Node>();
//		_edges = new List<Edge>();
//		_rooms = new List<Room>();
	}
	
	// Update is called once per frame
	void Update(){
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

		//Create four nodes with edges in between
		//Create volume mesh for them
	}

	//Add new node without edges to room graph
	public GameObject AddNode(Vector3 pos){
		GameObject newNodeObj = Instantiate(NODE, pos, Quaternion.identity) as GameObject;
		Node newNode = newNodeObj.GetComponent<Node>();
		newNode.Init();

//		_nodes.Add(newNode);
//		_nodeBounds.Add(newNodeObj, newNodeObj.GetComponent<Collider>().bounds);

		newNodeObj.transform.SetParent(graph);
		return newNodeObj;
	}

	//Add new node as neighbour with existing node to room graph
	public GameObject AddNode(Vector3 pos, Node curNode){
		GameObject newNodeObj = Instantiate(NODE, pos, Quaternion.identity) as GameObject;
		Node newNode = newNodeObj.GetComponent<Node>();
		newNode.Init();

//		_nodeBounds.Add(newNodeObj, newNodeObj.GetComponent<Collider>().bounds);

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

//		_edges.Add(newEdge);
//		_nodeBounds.Add(newEdgeObj, newEdgeObj.GetComponent<Collider>().bounds);

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
//		_rooms.Add(newRoom);

		foreach(Node n in nodes){
			n.AddRoom(newRoom);
		}

		newRoomObj.transform.SetParent(graph);
		return newRoomObj;
	}

	//
	public void ConnectNodes(Node node1, Node node2){
		Room room = node1.GetRoom();
		if(room.ContainsNode(node2)){;
			room.RemoveNode(node2);
		} else{
			Debug.Log("Connected node " + node1.ToString() + " and " + node2.ToString());
			node1.ConnectNode(node2);
			node2.ConnectNode(node1);
		}
	}

//	//Merge two nodes into one, updating connected edges and rooms
//	public void MergeNodes(Node node1, Node node2){
//		List<Node> neighbours = node2.GetNodes();
//		List<Node> exclude = node1.GetNodes();
//
//		List<Room> rooms = node1.GetRooms();d
//		rooms.AddRange(node2.GetRooms());
//
//		foreach(Node n in neighbours){
//			AddEdge(node1, n);
//		}
//
//		foreach(Room r in rooms){
//			r.UpdateNodes(node1, node2);
//		}
//
//		_nodeBounds.Remove(node2.gameObject);
////		_nodes.Remove(node2);
//		node2.Remove();
//
//		_recusionCount = 0;
////		SearchForRoom(node1, null, new List<Node>(), exclude);
////		SearchForRoom(node1, null, new List<Node>(), 0f);
//		SearchForRoom(node1);
//		Debug.Log("recursions: " + _recusionCount);
//	}

	//
	public void SplitEdge(Edge edge, Vector3 pos){
		GameObject newNodeObj = AddNode(pos);
		Node newNode = newNodeObj.GetComponent<Node>();

		AddEdge(newNode, edge.n1);
		AddEdge(newNode, edge.n2);

		Room room = edge.n1.GetRoom();
		room.AddNode(newNode, edge.n1, edge.n2);
		newNode.AddRoom(room);

		edge.Remove();
	}

	//
	public GameObject[] GetCollidingNode(Bounds b){
//		Bounds pb = _marker.GetComponent<Collider>().bounds;
		GameObject[] result = _nodeBounds.GetColliding(b);

		return result;
	}

	//
	public void UpdateCollision(GameObject node){
		node.GetComponent<Node>().Refresh();
//		_nodeBounds.Remove(node);
//		_nodeBounds.Add(node, node.GetComponent<Collider>().bounds);
//
//		List<Edge> edges = node.GetComponent<Node>().GetEdges();
//		foreach(Edge edge in edges) {
//			_nodeBounds.Remove(edge.gameObject);
//			_nodeBounds.Add(edge.gameObject, edge.gameObject.GetComponent<Collider>().bounds);
//		}


//		foreach(Edge edge in node.GetComponent<Node>().GetEdges()){
//			_nodeBounds.Remove(edge.gameObject);
//			_nodeBounds.Add(edge.gameObject, edge.gameObject.GetComponent<Collider>().bounds);
//		}
	}

//	void chordless_cycles(int* adjacency, int dim){
//		for(int i=0; i<dim-2; i++){
//			for(int j=i+1; j<dim-1; j++){
//				if(!adjacency[i+j*dim])
//					continue;
//				list<vector<int> > candidates;
//				for(int k=j+1; k<dim; k++){
//					if(!adjacency[i+k*dim])
//						continue;
//					if(adjacency[j+k*dim]){
//						cout << i+1 << " " << j+1 << " " << k+1 << endl;
//						continue;
//					}
//					vector<int> v;
//					v.resize(3);
//					v[0]=j;
//					v[1]=i;
//					v[2]=k;
//					candidates.push_back(v);
//				}
//				while(!candidates.empty()){
//					vector<int> v = candidates.front();
//					candidates.pop_front();
//					int k = v.back();
//					for(int m=i+1; m<dim; m++){
//						if(find(v.begin(), v.end(), m) != v.end())
//							continue;
//						if(!adjacency[m+k*dim])
//							continue;
//						bool chord = false;
//						int n;
//						for(n=1; n<v.size()-1; n++)
//							if(adjacency[m+v[n]*dim])
//								chord = true;
//						if(chord)
//							continue;
//						if(adjacency[m+j*dim]){
//							for(n=0; n<v.size(); n++)
//								cout<<v[n]+1<<" ";
//							cout<<m+1<<endl;
//							continue;
//						}
//						vector<int> w = v;
//						w.push_back(m);
//						candidates.push_back(w);
//					}
//				}
//			}
//		}
//	}

//	public void SearchForRoom(Node node, Node prev, List<Node> list, List<Node> exclude){
//		_recusionCount++;
//
//		//Inf recursion guard
//		if(_recusionCount > 200){
//			Debug.Log("force stopped");
//			return;
//		}
//
//		if(list.Contains(node)){
//			if(!ContainsRoom(list)){
//				AddRoom(list);
//				Debug.Log("Added new room");
//			} else{
//				Debug.Log("Found duplicate room");
//			}
//			return;
//		}
//
//		list.Add(node);
//
//		foreach(Edge edge in node.GetEdges()){
//			Node next = edge.GetOtherNode(node);
//			if(next != prev && !exclude.Contains(next)){
//				SearchForRoom(next, node, list, new List<Node>());
//			}
//		}
//
//		if(node.GetEdges().Count == 0) {
//			Debug.Log("no edges");
//		}
//	}

//	//
//	public void SearchForRoom0(Node node){
//		foreach(Edge edge in node.GetEdges()){
//			List<Node> list = new List<Node>();
//			list.Add(node);
//
//			Node next = edge.GetOtherNode(node);
//
//			float nextAngle = AngleFromNodeToNode(node, next);
//			float tmpAngle = (nextAngle - 0 + 360) % 360;
//			Debug.Log("Start, " + node.ToString() + "->" + next.ToString() + ": " + tmpAngle);
//
////			SearchForRoom(edge.GetOtherNode(node), node, list);
//			SearchForRoom(node, next, node, list, AngleFromNodeToNode(node, next));
////			SearchForRoom(next, node, list, 180);
////			SearchForRoom(edge.GetOtherNode(node), node, list, 0f);
//		}
//	}

	//
//	public void SearchForRoom2(Node start, Node node, Node prev, List<Node> list, float prevAngle){
//		Debug.Log(_recusionCount + " " +
//		          prev.ToString() + " -> " + node.ToString());
//		_recusionCount++;
//
//		//Inf recursion guard
//		if(_recusionCount > 200){
//			Debug.Log("Force stopped");
//			return;
//		}
//
//		if(start == node) {
//			if(PathContainsExistingRoom(list)){
//				Debug.Log("Room would contain smaller room");
//			} else if(ContainsRoom(list)){
//				Debug.Log("Duplicate room");
//			} else{
//				AddRoom(list);
//				Debug.Log("Added new room");
//			}
//			return;
//		}
//
//		if(list.Contains(node)) {
//			Debug.Log("Back on old path");
////			return;
//		}
//
//
////		if(list.Contains(node)){
////			if(!ContainsRoom(list)){
////				AddRoom(list);
////				Debug.Log("Added new room");
////			} else{
////				Debug.Log("Duplicate room");
////			}
////			return;
////		}
//
//		list.Add(node);
//
//		Node nextNode = null;
//		float maxAngle = -1 * Mathf.Infinity;
//		float nextAngle = 0;
//
//		foreach(Edge edge in node.GetEdges()){
//			Node tmpNode = edge.GetOtherNode(node);
////			float tmpAngle = AngularDifference(edge.GetRotationFrom(node), prevAngle);
//			if(tmpNode != prev){
////				nextNode = tmpNode;
//				nextAngle = AngleFromNodeToNode(node, tmpNode);
////				float tmpAngle = edge.GetRotationFrom(node, prevAngle);
//				float tmpAngle = (nextAngle - prevAngle + 360) % 360;
//
//				Debug.Log(node.ToString() + "->" + tmpNode.ToString() + ": " + tmpAngle);
//
//				if(tmpAngle >= maxAngle){
//					nextNode = tmpNode;
//					maxAngle = tmpAngle;
////					if(node.GetEdges().Count > 2)
////						Debug.Log("+new max " + tmpAngle + " -> " + nextNode.ToString());
//				}
//			}
//		}
//
//		if(nextNode != null){
////			if(maxAngle < 180){
////				Debug.Log("Counterclock-wise");
////				return;
////			}
//			SearchForRoom(start, nextNode, node, list, nextAngle);
//		}
//	}

//	public void SearchForRoom1(Node node, Node prev, List<Node> list){
//		Debug.Log(_recusionCount + " " +
//			prev.ToString() + " -> " + node.ToString());
////		Debug.Log(_recusionCount + " " +
////		          prev.ToString() + " -> " + node.ToString() +
////		          ", angle:" + "?" +
////		          ", list:" + list.Count +
////		          ", edges: " + node.GetEdges().Count);
//		_recusionCount++;
//		
//		//Inf recursion guard
//		if(_recusionCount > 200){
//			Debug.Log("force stopped");
//			return;
//		}
//		
//		if(list.Contains(node)){
//			if(!ContainsRoom(list)){
//				AddRoom(list);
//				Debug.Log("Added new room");
//			} else{
//				Debug.Log("Duplicate room");
//			}
//			return;
//		}
//		
//		list.Add(node);
//		
//		Node nextNode = null;
//		float maxAngle = 0;
//
//		foreach(Edge edge in node.GetEdges()){
//			Node tmpNode = edge.GetOtherNode(node);
//			//			float tmpAngle = AngularDifference(edge.GetRotationFrom(node), prevAngle);
//			if(tmpNode != prev){
//				//				nextNode = tmpNode;
//
//				float tmpAngle = edge.GetRotationFrom(node, prev);
//				
//				if(tmpAngle >= maxAngle){
//					nextNode = tmpNode;
//					maxAngle = tmpAngle;
//					if(node.GetEdges().Count > 2)
//						Debug.Log("+new max " + tmpAngle + " -> " + nextNode.ToString());
//				}
//			}
//		}
//		
//		if(nextNode != null){
//			Debug.Log(", angle:" + maxAngle);
//			SearchForRoom(nextNode, node, list);
//		}
//	}

//	//
//	public bool ContainsRoom(List<Node> nodes){
//		foreach(Room room in _rooms){
//			if(room.Equals(nodes)){
//				return true;
//			}
//		}
//		return false;
//	}
//
//	//
//	public bool PathContainsExistingRoom(List<Node> nodes){
//		foreach(Room room in _rooms){
//			if(PathContainsPath(nodes, room.GetNodes())){
//				return true;
//			}
//		}
//		return false;
//	}

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
}
