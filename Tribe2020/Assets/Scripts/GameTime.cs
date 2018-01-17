using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using SimpleJSON;



public class GameTime : SimulationObject {
	private static GameTime _instance;
	public static GameTime GetInstance() {
		return _instance;
	}

	public enum TimeContext { None, GameTime, RealWorldTime };

	[Serializable]
	public class KeyAction:DataEvent
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


	[HideInInspector]
    public double RealWorldTime;
    [Space(10)]
    [HideInInspector]
    public float simulationDeltaTime;
    [Space(10)]
    
	[Header("Clock")]
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
	public List<SimulationObject> SimulationObjects = new List<SimulationObject>();
	[SerializeField]
	private SimulationObject closestPrev = null;
	[SerializeField]
	private SimulationObject closestNext = null;

	[Range(0.0f, 100.0f)]
	public float VisualTimeScale = 1.0f;

	[Range(0.0f, 10000.0f)]
	public float SimulationTimeScaleFactor = 1.0f;

	public bool LockScales;



	[Space(10)]
	public List<double> Hollidays = new List<double>();
	public bool RedLetterDay = false;
	[Space(10)]

	private float prevVisualTimeScale,prevSimulationTimeScale;
	private float normalVisualTimeScale,normalSimulationTimeScale;
	private double target = double.NaN;
	public bool speeding = false;

	public void Awake () {
		base.Awake ();
		RegisterKeypoints ();

		if (StartInRealtime) {
			TimeSpan t = DateTime.UtcNow - new DateTime(1970, 1, 1);
			RealWorldTime = t.TotalSeconds;

			StartTime = RealWorldTime;


		}

		time = StartTime + offset;

		//First instance.
		if (_instance == null)
			_instance = this;

		prevVisualTimeScale = VisualTimeScale;
		prevSimulationTimeScale = SimulationTimeScaleFactor;
		lastupdate = Time.time;
	}

	// Use this for initialization
	public void Start () {

        CurrentDate = TimestampToDateTime(time).ToString("yyyy-MM-dd HH:mm:ss");

	}

    //Add a reference to an object that implements simulationObject. The UpdateSim function of the passed object will be called when provided timestamp is passed. If provided timestamp is in history, the updatesim will get called immmeditely (kind of).
	public bool AddKeypoint(double TimeStamp,SimulationObject target)
	{

	//	print ("DEPRICTED");
		return false;

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

		RedLetterDay = IsRedLetterDay (new_time);

		if (StopAtRealtime && new_time > RealWorldTime) {
			new_time = RealWorldTime;
			offset = 0;
			Time.timeScale = 1;
		} else {
			Time.timeScale = VisualTimeScale;
		}

        //Do all key actions requiered until the new time
        DoKeyActions(new_time);
		UpdateSimulationObjects (time,new_time,0,false);

        simulationDeltaTime = (float) (new_time - time);
		time = new_time;

        dateTimeLastUpdate = dateTimeCurrent;
        dateTimeCurrent = TimestampToDateTime(time);
        CurrentDate = dateTimeCurrent.ToString("yyyy-MM-dd HH:mm:ss");

		lastupdate = now;

        
        
	}

	override public bool UpdateSim(double time) {
		
		if (!double.IsNaN(target)){
			print ("Returning to normal timescale");
			print (normalVisualTimeScale);
			VisualTimeScale = normalVisualTimeScale;
			prevVisualTimeScale = normalVisualTimeScale;
			SimulationTimeScaleFactor = normalSimulationTimeScale;
			prevSimulationTimeScale = normalSimulationTimeScale;

			SetNext(double.PositiveInfinity);

			target = double.NaN;
			speeding = false;

			return true;
		
		}

		return false;
	}

	//Sets a temporary speed that is mainained until a certain point in time is reached. 
	public void SpeedTo(double ts, float VisualSpeedFactor, float SimulationSpeedFactor) {


		if (double.IsNaN (target)) {
			normalVisualTimeScale = VisualTimeScale;
			normalSimulationTimeScale = SimulationTimeScaleFactor;

		}

		speeding = true;

		target = ts;
		VisualTimeScale = VisualTimeScale * VisualSpeedFactor;
		SimulationTimeScaleFactor = SimulationTimeScaleFactor * SimulationSpeedFactor;
		prevVisualTimeScale = VisualTimeScale;
		prevSimulationTimeScale = SimulationTimeScaleFactor;

		SetNext (ts);
	
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

	public DayOfWeek GetDayOfWeek(int i){
		DateTime date = TimestampToDateTime (time + (86400 * i));
		return date.DayOfWeek;
	}

	public DayOfWeek GetDayOfWeek(double ts){
		DateTime date = TimestampToDateTime (ts);
		return date.DayOfWeek;
	}
	public double GetTimestampForDay(double ts){
		DateTime date = TimestampToDateTime (ts);
		TimeSpan span = (date.Date - new DateTime(1970, 1, 1, 0, 0, 0, 0).ToLocalTime());

		//return the total seconds (which is a UNIX timestamp)
		return (double)span.TotalSeconds;
	}

	public double GetTimestampForDay(int i){
		DateTime date = TimestampToDateTime (time + (86400 * i));
		TimeSpan span = (date.Date - new DateTime(1970, 1, 1, 0, 0, 0, 0).ToLocalTime());

		//return the total seconds (which is a UNIX timestamp)
		return (double)span.TotalSeconds;
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


	// NEW Interface or callback on specific times. 
	public bool register(SimulationObject Obj){

		if (SimulationObjects.Contains(Obj))
			return false;

		SimulationObjects.Add (Obj);

		UpdatePrev (Obj);
		UpdateNext (Obj);

		return true;
	}

	private double UpdateSimulationObjects(double oldtime, double newtime,double maxtime ,bool skiptto){

		bool forward = ((newtime - oldtime) > 0);
			

		if (skiptto) {
			//TODO
			return newtime;
		}
			
		if (forward) { 
			while (closestNext.NeedUpdate (newtime)) {
				time = closestNext.GetNext ();
				closestNext.UpdateSim (time);
				closestNext = FindNextClosest (time);
			}
			closestPrev = FindPrevClosest (time);

		} else {
			while (closestPrev.NeedUpdate (newtime)) {
				time = closestPrev.GetPrev ();
				closestPrev.UpdateSim (time);
				closestPrev = FindPrevClosest (time);
			}

			closestNext = FindNextClosest (time);
		}
			

		return newtime;
	}

	public void FindClosest (double ts){
		closestNext = FindNextClosest (time);
		closestPrev = FindPrevClosest (time);
	}

	public SimulationObject FindNextClosest (double ts){
		double Current, ClosestTs = double.PositiveInfinity;
		SimulationObject Closest = null;

		foreach (SimulationObject Obj in SimulationObjects) {
		
			Current = Obj.GetNext ();

			if (Current < ClosestTs && Current >= ts) {
				ClosestTs = Obj.GetNext ();
				Closest = Obj;
			}
		}

		return Closest;
	}

	public SimulationObject FindPrevClosest (double ts){
		double Current, ClosestTs = double.NegativeInfinity;
		SimulationObject Closest = null;

		foreach (SimulationObject Obj in SimulationObjects) {

			Current = Obj.GetPrev ();

			if (Current > ClosestTs && Current <= ts) {
				ClosestTs = Obj.GetPrev ();
				Closest = Obj;
			}
		}

		return Closest;
	}

	public bool UpdatePrev(SimulationObject obj){

		//In the furture 
		if (obj.GetPrev () > time)
			return false;

		if (closestPrev == null || obj.GetPrev () > closestPrev.GetPrev ())
			closestPrev = obj;

		return true;
	}

	public bool UpdateNext(SimulationObject obj){

		//In the past 
		if (obj.GetNext () < time)
			return false;

		if (closestNext == null || obj.GetNext () < closestNext.GetNext ())
			closestNext = obj;

		return true;
	}

	private void DoKeyActions(double newtime) { 

		KeyAction ka = null;
		double oldtime,delta;

        //TimeProfiler tp = new TimeProfiler("Do key actions", true);

        while (KeyActions.Count > 0 ) {
            //All remaning are in the future (assuming that the list is sorted). 
            if (KeyActions[0].Timestamp > newtime) {
                break;
            }

            //tp.IncreaseCounter(true);
 
			ka = KeyActions[0];

            //Set gameTime to the time for the key action. In case game time are referenced somewhere when executing UpdateSim.
			oldtime = time;
            time = ka.Timestamp;

			delta = time - oldtime;

			if (delta < 0)
				print ("!!!!!!!!!!!!! Game time delta:" + delta);

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

	public JSONClass EncodeToJSON() {
		JSONClass json = new JSONClass();
		json.Add("lastTime", offset.ToString());
		return json;
	}

	public void DecodeFromJSON(JSONClass json) {
		offset = (json["lastTime"].AsDouble);
		time = StartTime + offset;
	}

}

