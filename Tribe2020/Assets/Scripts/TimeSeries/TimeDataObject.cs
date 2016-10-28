using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using System;

public class TimeDataObject : MonoBehaviour {


	[System.Serializable]
	public class Connection{
		
		public string Topic;
		public TimeDataObject Target;
		[HideInInspector] public TimeDataObject Source;


//		public bool UseTopicFilter = false;

//		public bool UseTimeFilter = false;
//		public double FromTime = double.NaN,ToTime=double.NaN;
//		public bool RelativeTime=false;


	}


	[Header("Time data object properties")]
	public string Name;
	[Space(10)]
	public string[] Units;
	public string[] Columns; 
	[Space(10)]
	public List<Connection> Targets = new List<Connection>();
	private List<Connection> Sources = new List<Connection>();

	public void Register(Connection Sub){

		if (Sub.Target == null && Sub.Source == null)
			return;

		if (Sub.Target == null)
			Sub.Target = this;

		if (Sub.Source == null)
			Sub.Source = this;

		if (Sub.Target == this) {
			Sub.Source.Register (Sub);
			if (!Sources.Contains(Sub))
				Sources.Add(Sub);
		}
		else if (Sub.Source == this) {
			//Sub.Target.Register (Sub);
			if (!Targets.Contains(Sub))
				Targets.Add(Sub);
		}
			

	}

	public void Awake() {
		RegisterAll ();
	}

	public void RegisterAll(){
		foreach (Connection Sub in Sources) {
			Register (Sub);
		}
	}


	virtual public void TimeDataUpdate(Connection Con,DataPoint data) {
		print("Unhandled data!");
	}

	virtual public void TimeDataUpdate(Connection Con, DataPoint[] data) {
		print("Unhandled data!");
	}

	virtual public void JsonUpdate(Connection Con, JSONObject json) {
		//Convert to DataPoint or DataPoints and do TimeDataUpdate
		print("Unhandled data!");
	}


	virtual public void UpdateAllTargets(DataPoint Data) {
		foreach (Connection conn in Targets) {
			if (conn != null) {
				conn.Target.TimeDataUpdate (conn, Data);
			}
		}

	}

}
