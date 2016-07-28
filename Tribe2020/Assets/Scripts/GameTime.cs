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

		//See if day shift happened
		//if(TimestampToDateTime(time).Day != TimestampToDateTime(StartTime + Time.time).Day) {
		//	_controller.OnNextDay();
		//}

		//Calculate deltatime.
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

	//
	public double ScheduleToTimestamp(string hourMinute) {
		DateTime curTime = TimestampToDateTime(time);
		return ScheduleToTS(curTime.Year, curTime.Month, curTime.Day, hourMinute);
	}

	//
	public double ScheduleToTimestamp(int dOff, string hourMinute) {
		DateTime curTime = TimestampToDateTime(time);
		return ScheduleToTS(curTime.Year, curTime.Month, curTime.Day + dOff, hourMinute);
	}

	//
	public double ScheduleToTimestamp(int mOff, int dOff, string hourMinute) {
		DateTime curTime = TimestampToDateTime(time);
		return ScheduleToTS(curTime.Year, curTime.Month + mOff, curTime.Day + dOff, hourMinute);
	}

	//
	public double ScheduleToTS(int year, int month, int day, string hourMinute) {
		string[] timeParse = hourMinute.Split(':');
		int hour = int.Parse(timeParse[0]);
		int minute = int.Parse(timeParse[1]);

		//create Timespan by subtracting the value provided from
		//the Unix Epoch
		DateTime value = new DateTime(year, month, day, hour, minute, 0).ToLocalTime();
		TimeSpan span = (value - new DateTime(1970, 1, 1, 0, 0, 0, 0).ToLocalTime());

		//return the total seconds (which is a UNIX timestamp)
		return (double)span.TotalSeconds;
	}

	//
	public double ScheduleToTS(double timeOffset, int dayOffset, string hourMinute) {
		DateTime curTime = TimestampToDateTime(timeOffset);
		return ScheduleToTS(curTime.Year, curTime.Month, curTime.Day + dayOffset, hourMinute);
	}

	public DateTime TimestampToDateTime(double value)
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

	//
	public double GetTotalSeconds() {
		time = StartTime + Time.time;
		return (double)time;
	}

	//
	public string GetViewTime() {
		time = StartTime + Time.time;
		return TimestampToDateTime(time).ToString("HH:mm");
	}

	//
	public float GetMinutes() {
		return TimestampToDateTime(time).Minute + TimestampToDateTime(time).Hour * 60;
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

	//
	public int GetYear() {
		return GetDateTime().Year;
	}

	//
	public int GetMonth() {
		return GetDateTime().Month;
	}

	//
	public int GetDay() {
		return GetDateTime().Day;
	}
}

	