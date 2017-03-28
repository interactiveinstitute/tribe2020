using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(Manipulation))]
public class ManipulationEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        //Manipulation myScript = (Manipulation)target;


        if (GUILayout.Button("Activate", GUILayout.Width(100)))
        {
            //myScript.Test();

            //Manipulation manipulation = new Manipulation();
            //myScript.Manipulations.Add(manipulation);

        }

    }
}
