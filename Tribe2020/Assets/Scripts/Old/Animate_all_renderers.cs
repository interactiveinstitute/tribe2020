using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Animate_all_renderers : MonoBehaviour {

	public float colorStart = -2f;
	public float colorEnd = 0.5f;
	public float duration = 1.0F;
	public Renderer[] AllRenderers;
	private Renderer rend;
	//private Material[] myMaterials;
	void Start() {
		AllRenderers = (Renderer[])Object.FindObjectsOfTypeAll(typeof(Renderer));
		rend = GetComponent<Renderer>();
	}



	void Update() {
		
		foreach (Renderer rend in AllRenderers) {
			foreach (Material mat in rend.materials) {

				// mat.color = Color.red;
				float lerp = Mathf.PingPong (Time.time, duration) / duration;
				mat.SetFloat ("_Metallic", Mathf.Lerp (colorStart, colorEnd, lerp));
			}			

		}
	}
}
/*
Renderer[] arrend = (Renderer[])Resources.FindObjectsOfTypeAll(typeof(Renderer));
foreach (Renderer rend in arrend)
{
	foreach (Material mat in rend.sharedMaterials)
	{
		if (!armat.Contains(mat))
		{
			armat.Add(mat);
		}
	}
}
*/