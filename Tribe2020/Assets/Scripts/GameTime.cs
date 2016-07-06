using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;



public class GameTime : MonoBehaviour {
	private static GameTime _instance;
	public static GameTime GetInstance() {
		return _instance;
	}

	private PilotController _controller;

	public double StartTime;
	public double time;
	public string CurrentDate;
	public List<double> Keypoints = new List<double>();

	[Range(0.0f, 100.0f)]
	public float TimeScale = 1.0f;

	void Awake () {
		time = StartTime;
		_instance = this;
	}

	// Use this for initialization
	void Start () {
		_controller = PilotController.GetInstance();

		time = StartTime;
	}

	public bool AddKeypoint(double keypoint)
	{
		if (Keypoints.Contains(keypoint))
			return false;
		
		Keypoints.Add (keypoint);
		Keypoints.Sort ();

		return true;
	}
	
	// Update is called once per frame
	void Update () {

		//Delete expired keypoints.

		//Calculate deltatime.
		if(TimestampToDateTime(time).Day != TimestampToDateTime(StartTime + Time.time).Day) {
			_controller.OnNextDay();
		}

		time = StartTime + Time.time;
		CurrentDate = TimestampToDateTime(time).ToString("yyyy-MM-dd HH:mm:ss");
		Time.timeScale = TimeScale;
	}

	private double DateTimeToTimestamp(DateTime value)
	{
		//create Timespan by subtracting the value provided from
		//the Unix Epoch
		TimeSpan span = (value - new DateTime(1970, 1, 1, 0, 0, 0, 0).ToLocalTime());

		//return the total seconds (which is a UNIX timestamp)
		return (double)span.TotalSeconds;
	}

	private DateTime TimestampToDateTime(double value)
	{
		//create Timespan by subtracting the value provided from
		//the Unix Epoch
		DateTime date = ( new DateTime(1970, 1, 1, 0, 0, 0, 0) + new TimeSpan(0,0,(int)value));

		//return the total seconds (which is a UNIX timestamp)
		return date;
	}

	public DateTime GetDateTime()
	{
		return TimestampToDateTime(time);
	}

	public void Offset(float delta)
	{
		StartTime = StartTime + delta;
	}

	public void SetStartTime(double NewTime) {
		StartTime = NewTime;
	}

	public void SetTime(double NewTime) {
		StartTime = NewTime - Time.time;
	}
}

	