﻿using UnityEngine;
using System.Collections;
using System;




public class GameTime : MonoBehaviour {

	public double StartTime = 1452691843.939;
	public double CurrentTime;
	public string CurrentDate;

	// Use this for initialization
	void Start () {
		CurrentTime = StartTime;
	}
	
	// Update is called once per frame
	void Update () {
		CurrentTime = StartTime + Time.time;
		CurrentDate = TimestampToDateTime(CurrentTime).ToString("yyyy-MM-dd HH:mm:ss");
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
}

	