using UnityEngine;
using System.Collections;

public class DebugManager : MonoBehaviour {

    //Add two fields here for the class that should have toggleable Debug. Also add a condition check in the function called Output, below. Then call DebugManager instead of Debug in the corresponding class
    static public bool staticBehaviourAI;
    public bool BehaviourAI = true;

    static public bool staticAvatarActivity;
    public bool AvatarActivity = true;

    static public bool staticAppliance;
    public bool Appliance = true;


    //TODO: Make a general function for both logerror and log

    // Use this for initialization
    void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
        staticBehaviourAI = BehaviourAI;
        staticAvatarActivity = AvatarActivity;
        staticAppliance = Appliance;
	}

    static public void Log(string message, System.Object caller)
    {
        Log(message, null, caller);
    }

    static public void Log(string message, Object context, System.Object caller)
    {
        Output(message, context, caller, false);
    }

    static public void LogError(string message, System.Object caller)
    {
        LogError(message, null, caller);
    }

    static public void LogError(string message, Object context, System.Object caller)
    {
        Output(message, context, caller, true);
    }

    static private void Output(string message, Object context, System.Object caller, bool isErrorLog)
    {
        bool print = true;

        ///-------------- Insert condition checks for different monobehaviours below!
        //Let's check for conditions when we shouldn't print
        if (caller is BehaviourAI && !staticBehaviourAI)
        {
            print = false;
        }else if(caller is AvatarActivity && !staticAvatarActivity)
        {
            print = false;
        }else if(caller is Appliance && !staticAppliance)
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
