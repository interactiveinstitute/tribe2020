using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeProfiler {

    long _first;
    long _latest;
    long _reference;
    string _name;

    static TimeProfiler _instance = new TimeProfiler();

    public TimeProfiler(string name = "Time profiler") {
        SetName(name);
        _first = System.DateTime.Now.Ticks;
        _latest = _first;
    }

    public static TimeProfiler GetInstance() {
        return _instance;
    }

    public void SetName(string name) {
        _name = name;
    }

    public long MillisecondsSinceCreated(bool log = false) {
        _latest = System.DateTime.Now.Ticks;
        long time = (_latest - _first) / System.TimeSpan.TicksPerMillisecond;
        if (log) {
            Debug.Log(_name + ", Time since created: " + time);
        }
        return time;
    }

    public long MillisecondsSinceLatestCheck(bool log = false) {
        long latestPrev = _latest;
        _latest = System.DateTime.Now.Ticks;
        long time = (_latest - latestPrev) / System.TimeSpan.TicksPerMillisecond;
        if (log) {
            Debug.Log(_name + ", Time since last check: " + time);
        }
        return time;
    }

    public long MillisecondsSinceReference(bool log = false) {
        _latest = System.DateTime.Now.Ticks;
        long time = (_latest - _reference) / System.TimeSpan.TicksPerMillisecond;
        if (log) {
            Debug.Log(_name + ", Time since reference: " + time);
        }
        return time;
    }

    public void SetReference() {
        _reference = System.DateTime.Now.Ticks;
    }
}
