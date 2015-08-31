using UnityEngine;
using System.Collections;

public class EnergyVisualiserCell : MonoBehaviour {
	private Material _material;
	private MeshRenderer _renderer;

	// Use this for initialization
	void Start () {
		_renderer = GetComponent<MeshRenderer>();
		_material = new Material(Shader.Find("Standard"));
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void SetColor(Color newColor){
		_material.color = newColor;
		_renderer.material = _material;
	}
}
