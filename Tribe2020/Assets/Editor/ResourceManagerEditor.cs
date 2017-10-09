using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(ResourceManager))]
public class ResourceManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

		ResourceManager myScript = (ResourceManager)target;


        if (GUILayout.Button("Test button", GUILayout.Width(100)))
        {
            myScript.Test();

        }

    }
}
