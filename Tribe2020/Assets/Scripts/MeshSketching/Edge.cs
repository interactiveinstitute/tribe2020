using UnityEngine;
using System.Collections;

public class Edge : MonoBehaviour{
	private static BuildManager _buildMgr;

	public Node n1, n2;
	private bool _isInit = false;

	private float _prevRot = 0;

	public void Init(Node node1, Node node2){
		if(!_isInit){
			_buildMgr = BuildManager.GetInstance();
			_buildMgr.UpdateBound(gameObject);

			n1 = node1;
			n2 = node2;
			_isInit = true;

			Refresh(n1);
		}
	}

	// Use this for initialization
	void Start(){
	}
	
	// Update is called once per frame
	void Update(){
	}

	public void Refresh(Node n){
		if(_isInit){
			Vector3 start = n1.GetAnchorPoint(this);
			Vector3 end = n2.GetAnchorPoint(this);
			Vector3 center = start + (end - start) / 2;
			
			float distance = Vector3.Distance(start, end);
			
			float rotation = Mathf.Atan2(end.z - start.z, end.x - start.x) * Mathf.Rad2Deg;
			
			transform.position = center;
			transform.localScale = new Vector3(distance, 1, 1);
			transform.eulerAngles = new Vector3(0, 360 - rotation, 0);
			
			if(_prevRot != rotation){
				_prevRot = rotation;
			}

			_buildMgr.UpdateBound(gameObject);
		}
	}	

	public Node GetOtherNode(Node node){
		if(node == n1){
			return n2;
		} else if(node == n2){
			return n1;
		}

		return null;
	}

	//TODO
	public float GetRotationFrom(Node node, float prevAngle){
		Node other = GetOtherNode(node);

		Vector3 start = node.gameObject.transform.position;
		Vector3 end = other.gameObject.transform.position;

		float rotation = Mathf.Atan2(end.z - start.z, end.x - start.x) * Mathf.Rad2Deg;
		float angle = (rotation - prevAngle + 360) % 360;
		return angle;
	}

	//
	public void Remove(){
		n1.RemoveEdgeTo(n2);
		n2.RemoveEdgeTo(n1);

		_buildMgr.RemoveBound(gameObject);

		Destroy(gameObject);
	}

	//
	public string Stringify(){
		return "";
	}
}
