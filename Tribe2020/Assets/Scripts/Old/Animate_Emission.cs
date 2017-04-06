using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Animate_Emission : MonoBehaviour {

	public Color colorStart = Color.red;
	public Color colorEnd = Color.green;
	public float duration = 1.0F;
	private Renderer rend;
	//private Material[] myMaterials;
	void Start() {

		rend = GetComponent<Renderer>();

	}

	void Update() {
		foreach (Material mat in rend.materials) {

					// mat.color = Color.red;
					float lerp = Mathf.PingPong(Time.time, duration) / duration;
					mat.SetColor("_EmissionColor", Color.Lerp(colorStart, colorEnd, lerp));
					}			

	}

}

