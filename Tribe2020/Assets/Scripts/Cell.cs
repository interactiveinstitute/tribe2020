using UnityEngine;
using System.Collections;

public class Cell : MonoBehaviour {
	public enum Block {Void, Empty, Floor, Campfire};
	public Block type;
	private Cell _nw, _n, _ne, _e, _se, _s, _sw, _w;
	private Cell[] _neighbours;
	public float energy;
	private bool _isInitialized;

	// Use this for initialization
	void Start () {
		type = Block.Void;
		_isInitialized = false;
	}
	
	// Update is called once per frame
	void Update () {
		if(_isInitialized){
			float total = 0;
			foreach(Cell c in _neighbours){
				total += c.energy;
			}

			foreach(Cell c in _neighbours){
				if(c.energy < this.energy){
					c.energy ++;
					this.energy --;
				} else if(c.energy > this.energy){
					c.energy --;
					this.energy ++;
				}
			}

//			float total = _nw.energy + _n.energy + _ne.energy + _e.energy + _se.energy +
//				_s.energy + _sw.energy + _w.energy;
		}
	}

	public void InitNeighbours(Cell nw, Cell n, Cell ne, Cell e, Cell se, Cell s, Cell sw, Cell w){
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

	public void SetType(Block type){
		this.type = type;
//		UpdateNeighbours(this);
	}

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
