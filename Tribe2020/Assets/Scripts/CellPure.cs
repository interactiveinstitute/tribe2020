using UnityEngine;
using System.Collections;

public class CellPure {
	private GridManager.Block _type;

	public CellPure(GridManager.Block type){
		_type = type;
	}

	// Use this for initialization
	public void Start () {
	
	}
	
	// Update is called once per frame
	public void Update () {
	
	}

	public void SetType(GridManager.Block type){
		_type = type;
	}

	public GridManager.Block GetType(){
		return _type;
	}
}
