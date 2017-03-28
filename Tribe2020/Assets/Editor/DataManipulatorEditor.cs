using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(DataManipulator))]
public class DataManipulatorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        DataManipulator myScript = (DataManipulator)target;


        if (GUILayout.Button("Add", GUILayout.Width(100)))
        {
            //myScript.Test();

            Manipulation  manipulation = new Manipulation();
            myScript.Manipulations.Add(manipulation);

        }

        if (GUILayout.Button("Activate selected", GUILayout.Width(100)))
        {
            //myScript.Test();
            myScript.Activate();

        }

        if (GUILayout.Button("Deactivate selected", GUILayout.Width(100)))
        {
            //myScript.Test();
            myScript.Deactivate();

        }

    }
}