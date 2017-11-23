using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class WeekdaysSelector{
	public bool Monday = false;
	public bool Tuesday = false;
	public bool Wenesday = false;
	public bool Thursday = false;
	public bool Friday = false;
	public bool Saturday = false;
	public bool Sunday = false;

	public bool IsChecked(DayOfWeek Day){
		if (Day == DayOfWeek.Monday)
			return Monday;

		if (Day == DayOfWeek.Tuesday)
			return Tuesday;

		if (Day == DayOfWeek.Wednesday)
			return Wenesday;

		if (Day == DayOfWeek.Thursday)
			return Thursday;

		if (Day == DayOfWeek.Friday)
			return Friday;

		if (Day == DayOfWeek.Saturday)
			return Saturday;

		if (Day == DayOfWeek.Sunday)
			return Sunday;

		return false;
	}
}

public class Schedule : DataSeries {

	[Header("Restart Scheduler")]
	[Space(10)]
	public string StartTime;
	public string StopTime;
	[SerializeField]
	double StartTimeEpoc = double.NaN;
	[SerializeField]
	double StopTimeEpoc = double.NaN;

	[Space(10)]



	public WeekdaysSelector Weekdays = new WeekdaysSelector();

	[Space(10)]
	public bool RedLetterDays = false;
	public bool ExludeRedLetterDays = false;

	DayOfWeek Day;
	double TimestampOfDay = double.NaN;




	// Use this for initialization
	void Start () {

		ParseTimes ();
		//Enabled = false;


		//print(SimulationTime.GetTimestampForDay(0));


		RegisterKeypoints ();
		base.Start ();


	}

	overide DataPoint GetDataAt(double ts) {
		DayOfWeek tsDay;
		double tsTimestampOfDay;

		tsDay = SimulationTime.GetDayOfWeek (ts);
		tsTimestampOfDay = SimulationTime.GetTimestampForDay (ts);



		DataPoint dp = new DataPoint();


	}



	void ParseTimes(){
		StartTimeEpoc = ParseTime (StartTime);
		StopTimeEpoc = ParseTime (StopTime);
	}

	double ParseTime(string Str){

		char separator=':';

		string[] parts = Str.Split(separator);


		if (parts.Length == 3)
			return double.Parse(parts[0]) * 3600 + double.Parse(parts[1]) * 60 + double.Parse(parts[2]);
		else if (parts.Length == 2)
			return double.Parse(parts[0]) * 3600 + double.Parse(parts[1]) * 60;
		else if (parts.Length == 1)
			return double.Parse(parts[0]);

		return double.Parse(Str);
	}



	public void UpdateTimePropterties(){
		Day = SimulationTime.GetDayOfWeek (0);
		TimestampOfDay = SimulationTime.GetTimestampForDay (0);
	}


}
