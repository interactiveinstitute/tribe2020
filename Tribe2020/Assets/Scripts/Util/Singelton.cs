using UnityEngine;
using System.Collections;

public class Singelton : MonoBehaviour {


	//Singleton functionality 
	private static Singelton _instance;

	void Awake () {
		_instance = this;
	}

	public static Singelton GetInstance () {
		return _instance;
	}


}
