using UnityEngine;
using System.Collections;

public class DebugManager : MonoBehaviour {


    static public bool staticBehaviourAI = true;
    public bool BehaviourAI;


    //TODO: Make a general function for both logerror and log

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
        staticBehaviourAI = BehaviourAI;
	}

    static public void Log(string message, MonoBehaviour caller)
    {
        Log(message, null, caller);
    }

    static public void Log(string message, Object context, MonoBehaviour caller)
    {
        Output(message, context, caller, false);
    }

    static public void LogError(string message, MonoBehaviour caller)
    {
        LogError(message, null, caller);
    }

    static public void LogError(string message, Object context, MonoBehaviour caller)
    {
        Output(message, context, caller, true);
    }

    static private void Output(string message, Object context, MonoBehaviour caller, bool isErrorLog)
    {
        bool print = true;

        ///-------------- Insert condition checks for different monobehaviours below!
        //Let's check for conditions when we shouldn't print
        if (caller is BehaviourAI && !staticBehaviourAI)
        {
            print = false;
        }



        //Ok. Ledz print that shit!
        if (print)
        {
            if (context != null)
            {
                if (isErrorLog)
                {
                    Debug.LogError(message, context);
                }else
                {
                    Debug.Log(message, context);
                }
            }
            else
            {
                if (isErrorLog)
                {
                    Debug.LogError(message);
                }else
                {
                    Debug.Log(message);
                }
            }
        }
    }

}
