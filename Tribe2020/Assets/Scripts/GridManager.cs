using UnityEngine;
using System.Collections;

public class GridManager : MonoBehaviour {
	public const int cols = 20, rows = 20;
	public Cell[,] cells;

	// Use this for initialization
	void Start(){
		cells = new Cell[cols, rows];
		for(int x = 0; x < cols; x++){
			for(int y = 0; y < rows; y++){
				cells[x, y] = new Cell();
				if((x > 0 && y > 0) || (x < cols && y < rows)){
					cells[x, y].SetType(Cell.Block.Empty);
				}
			}
		}
	}
	
	// Update is called once per frame
	void Update(){
		
	}

	public void AddCell(float x, float y){
		if(x > 0 && x < cols && y > 0 && y < rows){
			cells[(int)x, (int)y].SetType(Cell.Block.Floor);
		}
	}

//	private GameObject CollidesWithBlock(Bounds b){
//		foreach(GameObject c in cells){
//			Bounds cb = c.GetComponent<Collider>().bounds;
//			if(b.Intersects(cb)){
//				return c;
//			}
//		}
//		return null;
//	}
}
