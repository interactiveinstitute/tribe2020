using UnityEngine;
using System.Collections;

public class Cell : MonoBehaviour {
	private GridManager.Block _type;
	private Cell _nw, _n, _ne, _e, _se, _s, _sw, _w;
	private Cell[] _neighbours;
	public float energy;
	private bool _isInitialized;
	private GameObject _block, _ui;
	private GameObject _energyUI;

	// Use this for initialization
	void Start(){
		GameObject uiCell = GameObject.Find ("UICell");
//		_energyUI = Instantiate(uiCell, transform.position, Quaternion.identity) as GameObject;
//		_block = transform.Find("ent_block").gameObject;
//		_ui = transform.Find("ui_cell").gameObject;
//
//		SetType (GridManager.Block.Empty);
	}
	
	// Update is called once per frame
	void Update(){
//		if(_isInitialized){
//			float total = 0;
//			foreach(Cell c in _neighbours){
//				total += c.energy;
//			}
//
//			foreach(Cell c in _neighbours){
//				if(c.energy < this.energy){
//					c.energy ++;
//					this.energy --;
//				} else if(c.energy > this.energy){
//					c.energy --;
//					this.energy ++;
//				}
//			}
//
////			float total = _nw.energy + _n.energy + _ne.energy + _e.energy + _se.energy +
////				_s.energy + _sw.energy + _w.energy;
//		}
	}

	public void Init(){
		_block = transform.Find("ent_block").gameObject;
		_ui = transform.Find("ui_cell").gameObject;
		SetType (GridManager.Block.Empty);
	}

	public void InitNeighbours(
		Cell nw, Cell n, Cell ne, Cell e, Cell se, Cell s, Cell sw, Cell w){
		_nw = nw;
		_n  = n;
		_ne = ne;
		_e  = e;
		_se = se;
		_s  = s;
		_sw = sw;
		_w  = w;
		_neighbours = new Cell[8]{nw, n, ne, e, se, s, sw, w};
		_isInitialized = true;
	}

	public void SetType(GridManager.Block type){
		_type = type;

//		Debug.Log ("Cell, SetType: now " + _type);

		switch(_type){
		case GridManager.Block.Empty:
			_block.GetComponent<MeshRenderer>().enabled = false;
//			Debug.Log("set to empty");
			break;
		case GridManager.Block.Floor:
			_block.GetComponent<MeshRenderer>().enabled = true;
//			Debug.Log("set to floor");
			break;
		default:
			_block.GetComponent<MeshRenderer>().enabled = false;
//			Debug.Log("set to void");
			break;
		}
//		UpdateNeighbours(this);
	}

	public GridManager.Block GetType(){
//		print ("from cell " + this.type);
//		Debug.Log ("Cell, GetType() : "+_type);
		return _type;
	}

//	public void SetBlock(GameObject block){
//		this.block = block;
//	}

//	public void Reset(){
//		Destroy(block);
//		type = GridManager.Block.Empty;
//	}

//	public void UpdateNeighbours(Cell orig){
//		if(nw != orig){ nw.UpdateNeighbours(this); }
//		if(n != orig) { n.UpdateNeighbours(this); }
//		if(ne != orig){ ne.UpdateNeighbours(this); }
//		if(e != orig) { e.UpdateNeighbours(this); }
//		if(se != orig){ se.UpdateNeighbours(this); }
//		if(s != orig) { s.UpdateNeighbours(this); }
//		if(sw != orig){ sw.UpdateNeighbours(this); }
//		if(w != orig) { w.UpdateNeighbours(this); }
//	}
}
