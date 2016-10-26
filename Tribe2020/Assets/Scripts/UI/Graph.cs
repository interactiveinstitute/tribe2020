using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Graph : TimeDataObject {

	[Header("Parameters")]
	public double StartTime;
	public double StopTime;
	public double Max,Min;
	public bool Relative=false;


	[Header("Source")]
	public TimeSeries Source;
	public  int ValueIndex = 0;

	[Header("Design")]
	public Color color = Color.red;
	public Material _material;
	public bool Staircase; 	

	double Period;
	float graphWidth;
	float graphHeight;
	float scalefactorX;
	float scalefactorY;
	double Now;

	public List<DataPoint> Data;

	GameTime _timeMgr = GameTime.GetInstance();

	// Use this for initialization
	void Start () {
		_timeMgr = GameTime.GetInstance();
	}
	
	// Update is called once per frame
	void Update () {
		if (Relative)
			Plot ();
	}

	public void InitParams(){

		if (_timeMgr == null)
			_timeMgr = GameTime.GetInstance();

		Now = _timeMgr.time;

		//Get period and width
		Period = StopTime - StartTime;
		graphWidth = GetComponent<RectTransform>().rect.width;
		scalefactorX = graphWidth / (float)Period;

		graphHeight = GetComponent<RectTransform>().rect.height;

		scalefactorY = graphHeight / (float)Max;


		Data = Source.GetPeriod (GetStartTime (), GetStopTime ());

			
	}

	public float TimeToCoordinate(double TimeStamp) {

		double start;

		start = GetStartTime ();

		return (float)(TimeStamp - start) * scalefactorX;
	}

	public double GetStartTime(){
		double start;

		if (Relative)
			start = Now + StartTime;
		else
			start = StartTime;

		return start;
	}

	public double GetStopTime(){
		double stop;

		if (Relative)
			stop = Now + StopTime;
		else
			stop = StopTime;

		return stop;
	}



	public float ValueToCoordinate(double value) {
		return (float)(value-Min) * scalefactorY;
	}

	public List<Vector2> Draw(List<DataPoint> values,bool staircase)  {

		float x=0, y,px,py;
		List<Vector2> Verts = new List<Vector2>();

		if (values.Count == 0)
			return Verts;

		px = TimeToCoordinate (values[0].Timestamp);
		py = ValueToCoordinate (values[0].Values [ValueIndex]);

		Verts.Add(new Vector2(px,0));
		Verts.Add(new Vector2(px,py));

		//newVerts.Add(new Vector2(0, 0));
		for (int i=1; i<Data.Count;i++) {

			x = TimeToCoordinate (values[i].Timestamp);
			y = ValueToCoordinate (values[i].Values [ValueIndex]);

			if (staircase)
				Verts.Add(new Vector2(x,py));

			Verts.Add(new Vector2(x,y));
			//newVerts.Add(new Vector2(i * stepWidth, values[i] / maxValue * graphHeight));

			px = x;
			py = y;
		}

		Verts.Add(new Vector2(x,0));

		return Verts;
	}

	public void Plot() {

		float alt = 1;


		InitParams ();
		//float stepWidth = graphWidth / values.Count;

		 

//		float minValue = 0;
//		float maxValue = 0;
//		foreach(float value in values) {
//			minValue = Mathf.Min(minValue, value);
//			maxValue = Mathf.Max(maxValue, value);
//		}
		//minValue -= 0.1f;


		//if (Data.Count > 0 )
		List<Vector2> newVerts;

		newVerts = Draw (Data,Staircase);

		//newVerts.Add(new Vector2(graphWidth, 0));

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
