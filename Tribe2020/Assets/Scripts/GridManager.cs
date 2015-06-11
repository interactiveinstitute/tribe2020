using UnityEngine;
using System.Collections;

public class GridManager : MonoBehaviour {
	public const int xMax = 40, yMax = 40, zMax = 40;
	public const int offset = -20;
	public Cell[,,] cells;
	public GameObject floor, campfire;
	public ParticleSystem pm;
	private ParticleSystem.Particle[] _ps;

	public enum Block {Floor, Campfire};

	// Use this for initialization
	void Start(){
		floor = GameObject.Find ("ent_block");
		campfire = GameObject.Find ("ent_campfire");

		_ps = new ParticleSystem.Particle[this.pm.particleCount];
		pm.GetParticles(_ps);

		cells = new Cell[xMax, yMax, zMax];
		for(int x = 0; x < xMax; x++){
			for(int y = 0; y < yMax; y++){
				for(int z = 0; z < zMax; z++){
					cells[x, y, z] = new Cell();
					if(x > 0 && y > 0 && z > 0 && x < xMax-1 && y < yMax-1 && z < zMax-1){
						cells[x, y, z].SetType(Cell.Block.Empty);
					} else{
						cells[x, y, z].SetType(Cell.Block.Void);
					}
				}
			}
		}
	}
	
	// Update is called once per frame
	void Update(){
		
	}

	public void AddBlock(int x, int y, int z, Block type){
		if(x > offset && x < xMax-offset &&
		   y > offset && y < yMax-offset &&
		   z > offset && z < zMax-offset){
			Vector3 pos = new Vector3(x * 5, y * 5, z * 5);
			GameObject blockObj = TypeToObject(type);
			GameObject newBlock = (GameObject)Instantiate(
				blockObj, pos, Quaternion.identity);
			cells[x, y, z].SetBlock(newBlock);
		}
	}

	public void RemoveBlock(GameObject obj){
		Vector3 pos = obj.transform.position / 5;

		cells[(int)pos.x, (int)pos.y, (int)pos.z].Reset();
	}

//	public GameObject CollidesWithBlock(int x, int y, int z, GameObject otherObj){
//		Collider col = otherObj.GetComponent<Collider>();
//		Bounds pb = col.bounds;
//		foreach(GameObject b in blocks){
//			Bounds bb = b.GetComponent<Collider>().bounds;
//			if(pb.Intersects(bb)){
//				return b;
//			}
//		}
//	}

	public GameObject GetBlock(int x, int y, int z){
		return cells[x, y, z].block;
	}

	public void ResetParticles(){
		pm.SetParticles(_ps, _ps.Length);
	}

	private GameObject TypeToObject(Block type){
		switch (type) {
		case Block.Floor:
			return GameObject.Find ("ent_block");
		case Block.Campfire:
			return GameObject.Find ("ent_campfire");
		default:
			return null;
		}
	}
}
