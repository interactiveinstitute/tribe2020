using UnityEngine;
using System.Collections;

public class CellPure {
	private GridManager.Block _type;
	public float _heat;

	public CellPure(GridManager.Block type){
		_type = type;
		_heat = Random.value * 255;
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

	public float Heat
	{
		get { return _heat;}
		set { _heat = value;}
	}
}
