using UnityEngine;
using System.Collections.Generic;

public class Shape : MonoBehaviour {
	public Color color = Color.red;
	public Material _material;
	public bool continuousPush = false;

	[Header("Source")]
	public List<float> values;
	public TimeSeries timeSeries;

	//Use this for initialization
	void Start() {
	}

	//Update is called once per frame
	void Update() {
		if(continuousPush && timeSeries != null) {
			Debug.Log(timeSeries.GetCurrentValue());
			PushValue((float)timeSeries.GetCurrentValue());
		}

		UpdateMesh();
	}

	//
	public void PushValue(float value) {
		values.RemoveAt(values.Count - 1);
		if(!float.IsNaN(value)) {
			values.Insert(0, value);
		} else {
			values.Insert(0, 0);
		}
	}

	//Update mesh coordinates in case nodes have changed
	public void UpdateMesh() {
		float alt = 1;

		float xMult = GetComponent<RectTransform>().rect.width / 10;
		float yMult = GetComponent<RectTransform>().rect.height / 10;

		float graphWidth = GetComponent<RectTransform>().rect.width;
		float graphHeight = GetComponent<RectTransform>().rect.height;
		float stepWidth = graphWidth / values.Count;

		float minValue = 0;
		float maxValue = 0;
		foreach(float value in values) {
			minValue = Mathf.Min(minValue, value);
			maxValue = Mathf.Max(maxValue, value);
		}
		minValue -= 0.1f;

		List<Vector2> newVerts = new List<Vector2>();
		newVerts.Add(new Vector2(0, minValue));
		for(int i = 0; i < values.Count; i++) {
			newVerts.Add(new Vector2(i * stepWidth, values[i] / maxValue * graphHeight));
		}
		newVerts.Add(new Vector2(graphWidth, minValue));

		//Use the triangulator to get indices for creating triangles
		Triangulator tr = new Triangulator(newVerts.ToArray());
		int[] indices = tr.Triangulate();

		//Create the Vector3 vertices
		Vector3[] vertices3d = new Vector3[newVerts.Count];
		for(int i = 0; i < vertices3d.Length; i++) {
			vertices3d[i] = new Vector3(newVerts[i].x, newVerts[i].y, -1 * alt);
		}

		//Create the mesh
		Mesh msh = new Mesh();
		msh.vertices = vertices3d;
		msh.triangles = indices;
		msh.RecalculateNormals();
		msh.RecalculateBounds();

		//_material = Resources.Load("BaseMaterial") as Material;
		Material pMat = new Material(_material);
		pMat.SetColor("_TintColor", color);
		pMat.SetColor("_Color", color);

		CanvasRenderer pCanvasRenderer = GetComponent<CanvasRenderer>();
		pCanvasRenderer.SetMaterial(pMat, null);

		RectTransform pRectTransform = GetComponent<RectTransform>();
		if(pRectTransform == null) {
			Vector3 pos = transform.localPosition;
			Quaternion rot = transform.localRotation;
			Vector3 scale = transform.localScale;

			pRectTransform = gameObject.AddComponent<RectTransform>();
			pRectTransform.localPosition = pos;
			pRectTransform.localRotation = rot;
			pRectTransform.localScale = scale;
		}

		//pCanvasRenderer.Clear();
		pCanvasRenderer.SetMesh(msh);
	}
}
