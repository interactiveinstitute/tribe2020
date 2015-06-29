using UnityEngine;
using UnityEditor;
using System.Collections;

public class Shor : MonoBehaviour {
	static class UsefulShortcuts{
		[MenuItem ("Tools/Clear Console %#c")] // CMD + SHIFT + C
		static void ClearConsole () {
			// This simply does "LogEntries.Clear()" the long way:
			var logEntries =
				System.Type.GetType("UnityEditorInternal.LogEntries,UnityEditor.dll");
			var clearMethod = logEntries.GetMethod(
				"Clear",
				System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
			clearMethod.Invoke(null,null);
		}
	}
	
	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update () {
	}
}