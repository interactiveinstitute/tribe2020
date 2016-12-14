using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class OutdoorTemperature : DataSeriesBuffer {

	//Singleton functionality 
	private static OutdoorTemperature _instance;

	void Awake () {
		_instance = this;
	}

	public static OutdoorTemperature GetInstance () {
		return _instance;
	}

}