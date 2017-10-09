using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;



public class GameTime : MonoBehaviour {
	private static GameTime _instance;
	public static GameTime GetInstance() {
		return _instance;
	}

	public enum TimeContext { None, GameTime, RealWorldTime };

	[Serializable]
	public class KeyAction:Event
	{
		public SimulationObject target;
	}

	public class CompareKeyAction : IComparer<KeyAction>
	{
		static IComparer<KeyAction> comparer = new CompareKeyAction();

		public int Compare(KeyAction x, KeyAction y)
		{
			if (x == y)    return 0;
			if (x == null) return -1;
			if (y == null) return 1;
			if (x.Timestamp > y.Timestamp)
				return 1;
			if (x.Timestamp < y.Timestamp)
				return -1;

			return 0;
		}
	}

   
    public double RealWorldTime;
    [Space(10)]
    [HideInInspector]
    public float simulationDeltaTime;
    [Space(10)]
    [Header("Game Time")]
    public double StartTime;
	public bool StartInRealtime;
	public bool StopAtRealtime;
	public double offset;
	public double time = Double.NaN;
	public double VisualTime;
	public string CurrentDate;
    public double _skipToOffset = -1;

    DateTime dateTimeCurrent;
    DateTime dateTimeLastUpdate;

    double lastupdate=0;
	public List<KeyAction> KeyActions = new List<KeyAction>();

	[Range(0.0f, 100.0f)]
	public float VisualTimeScale = 1.0f;

	[Range(0.0f, 10000.0f)]
	public float SimulationTimeScaleFactor = 1.0f;

	public bool LockScales;



	[Space(10)]
	public List<double> Hollidays = new List<double>();
	[Space(10)]

	private float prevVisualTimeScale,prevSimulationTimeScale;



	void Awake () {
		
		if (StartInRealtime) {
			TimeSpan t = DateTime.UtcNow - new DateTime(1970, 1, 1);
			RealWorldTime = t.TotalSeconds;

			StartTime = RealWorldTime;


		}

		time = StartTime + offset;
		_instance = this;

		prevVisualTimeScale = VisualTimeScale;
		prevSimulationTimeScale = SimulationTimeScaleFactor;
		lastupdate = Time.time;
	}

	// Use this for initialization
	void Start () {



        CurrentDate = TimestampToDateTime(time).ToString("yyyy-MM-dd HH:mm:ss");

	}

    //Add a reference to an object that implements simulationObject. The UpdateSim function of the passed object will be called when provided timestamp is passed. If provided timestamp is in history, the updatesim will get called immmeditely (kind of).
	public bool AddKeypoint(double TimeStamp,SimulationObject target)
	{
        //If trying to add a key action that should already have ben run, then run it immediately instead and return
        //if (TimeStamp < time) {
        //    target.UpdateSim(TimeStamp);
        //    return false;
        //}

        KeyAction keypoint = new KeyAction ();

		keypoint.Timestamp = TimeStamp;
		keypoint.target = target;
		
		KeyActions.Add (keypoint);
		KeyActions.Sort (new CompareKeyAction() );

		return true;
	}
	
	// Update is called once per frame
	void Update () {

		TimeSpan t = DateTime.UtcNow - new DateTime(1970, 1, 1);
		RealWorldTime = t.TotalSeconds;

		//Delete expired keypoints.

		//Did scales change? 
		if (prevVisualTimeScale != VisualTimeScale && LockScales)
			SimulationTimeScaleFactor = VisualTimeScale;
		else if (prevSimulationTimeScale != SimulationTimeScaleFactor && LockScales)
			VisualTimeScale = SimulationTimeScaleFactor;

		prevVisualTimeScale = VisualTimeScale;
		prevSimulationTimeScale = SimulationTimeScaleFactor;


		//Calculate difference between simulation and visual timescales and apply to offset.
		double now = Time.time;
		VisualTime = now;
		double delta = now - lastupdate;



		offset = offset + (delta/VisualTimeScale * (SimulationTimeScaleFactor - VisualTimeScale));

		double new_time = StartTime + offset + Time.time;

		if (StopAtRealtime && new_time > RealWorldTime) {
			new_time = RealWorldTime;
			offset = 0;
			Time.timeScale = 1;
		} else {
			Time.timeScale = VisualTimeScale;
		}

        //Do all key actions requiered until the new time
        DoKeyActions(new_time);

        simulationDeltaTime = (float) (new_time - time);
		time = new_time;

        dateTimeLastUpdate = dateTimeCurrent;
        dateTimeCurrent = TimestampToDateTime(time);
        CurrentDate = dateTimeCurrent.ToString("yyyy-MM-dd HH:mm:ss");

		lastupdate = now;

        
        
	}

	public void JumpToRealtime(){
		

		TimeSpan t = DateTime.UtcNow - new DateTime(1970, 1, 1);
		RealWorldTime = t.TotalSeconds;

		offset = RealWorldTime - StartTime - Time.time;
		time = RealWorldTime;
	}

	public string GetDay(int i){
		DateTime date = TimestampToDateTime (time + (86400 * i));
		return date.DayOfWeek.ToString();
	}

	public bool IsRedLetterDay(double timestamp)
	{
		DateTime date = TimestampToDateTime (timestamp);

		if (date.DayOfWeek == DayOfWeek.Sunday)
			return true;

		foreach (double ts in Hollidays) {
			if (TimestampToDateTime (ts).Date == date.Date)
				return true;
		}

		return false;
	}

	public bool IsRedLetterDay()
	{
		return IsRedLetterDay (time);
	}

	private void DoKeyActions(double newtime) { 

		KeyAction ka = null;

        //TimeProfiler tp = new TimeProfiler("Do key actions", true);

        while (KeyActions.Count > 0 ) {
            //All remaning are in the future (assuming that the list is sorted). 
            if (KeyActions[0].Timestamp > newtime) {
                break;
            }

            //tp.IncreaseCounter(true);
 
			ka = KeyActions[0];

            //Set gameTime to the time for the key action. In case game time are referenced somewhere when executing UpdateSim.
            time = ka.Timestamp;

            //Execute the event. 
            ka.target.UpdateSim(time);

            //Remove
            KeyActions.Remove (ka);

		}
        //tp.MillisecondsSinceCreated(true);

        return;
	}

	private double DateTimeToTimestamp(DateTime value)
	{
		//create Timespan by subtracting the value provided from
		//the Unix Epoch
		TimeSpan span = (value - new DateTime(1970, 1, 1, 0, 0, 0, 0).ToLocalTime());

		//return the total seconds (which is a UNIX timestamp)
		return (double)span.TotalSeconds;
	}

	////Convert timestring from schedule to timestamp
	//public double ScheduleToTimestamp(string hourMinute) {
	//	DateTime curTime = TimestampToDateTime(time);
	//	return ScheduleToTS(curTime.Year, curTime.Month, curTime.Day, hourMinute);
	//}

	////
	//public double ScheduleToTimestamp(int dOff, string hourMinute) {
	//	DateTime curTime = TimestampToDateTime(time);
	//	return ScheduleToTS(curTime.Year, curTime.Month, curTime.Day + dOff, hourMinute);
	//}

	////
	//public double ScheduleToTimestamp(int mOff, int dOff, string hourMinute) {
	//	DateTime curTime = TimestampToDateTime(time);
	//	return ScheduleToTS(curTime.Year, curTime.Month + mOff, curTime.Day + dOff, hourMinute);
	//}

	//Returns a timestamp derived from year, month, day and an hourminute string
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

	//Returns a timestamp derived from an hour-minute string and a day offset
    //The returned timestamp is based on the hour-minute from the day of the referenceStamp
    //offset by dayoffset.
	public double ScheduleToTS(double referenceStamp, int dayOffset, string hourMinute) {
		DateTime curTime = TimestampToDateTime(referenceStamp); //Ok we want a stamp from this day (+- dayoffset) corresponding to the hour:minute string
		curTime = curTime.AddDays(dayOffset);
		return ScheduleToTS(curTime.Year, curTime.Month, curTime.Day, hourMinute);
	}

	public DateTime TimestampToDateTime(double value)
	{
		//create Timespan by subtracting the value provided from
		//the Unix Epoch
		DateTime date = ( new DateTime(1970, 1, 1, 0, 0, 0, 0) + new TimeSpan(0,0,(int)value));
		//return the total seconds (which is a UNIX timestamp)
		return date;
	}

	public double GetFirstTimeOfDay(double ts){
		DateTime day = TimestampToDateTime(ts).Date;
		return DateTimeToTimestamp (day);
	}

	public double GetFirstTimeOfDay(){
		return GetFirstTimeOfDay (time);
	}

	public double GetFirstTimeOfDay(int i){
		return GetFirstTimeOfDay (time + (3600.0*24 * i));
	
	}

	public DateTime GetDateTime()
	{
		return TimestampToDateTime(time);
	}

	//
	public double GetTotalSeconds() {

		return (double)time;
	}

	//
	public string GetViewTime() {

		return TimestampToDateTime(time).ToString("HH:mm");
	}

	//
	public float GetMinutes() {
		return TimestampToDateTime(time).Minute + TimestampToDateTime(time).Hour * 60;
	}

	public void Offset(float delta)
	{
		offset = offset + delta;
	}

	public void SetStartTime(double NewTime) {
		offset = NewTime;
	}

	public void SetTime(double NewTime) {
		offset = NewTime - Time.time;
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

	//
	public string GetTimeWithFormat(string format) {
		return TimestampToDateTime(time).ToString(format);
	}

    public int GetNewMonths() {
        if(dateTimeCurrent.Year == 1 || dateTimeLastUpdate.Year == 1) {
            return 0;
        }
        return (dateTimeCurrent.Year - dateTimeLastUpdate.Year) * 12 + (dateTimeCurrent.Month - dateTimeLastUpdate.Month);
    }

    public bool IsWeekend() {
		DateTime date = TimestampToDateTime (time);

		if (date.DayOfWeek == DayOfWeek.Sunday || date.DayOfWeek == DayOfWeek.Saturday)
			return true;

		foreach (double ts in Hollidays) {
			if (TimestampToDateTime (time).Date == date.Date)
				return true;
		}

		return false;
    }

    public bool IsWeekendTomorrow() {
		DateTime date = TimestampToDateTime (time + 8640);

		if (date.DayOfWeek == DayOfWeek.Sunday || date.DayOfWeek == DayOfWeek.Saturday)
			return true;

		foreach (double ts in Hollidays) {
			if (TimestampToDateTime (time + 8640).Date == date.Date)
				return true;
		}

		return false;
    }
}

