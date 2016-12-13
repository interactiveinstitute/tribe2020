using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(Clothing))]
public class ClothingEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        Clothing clothing = (Clothing)target;


        if (GUILayout.Button("Randomize clothes", GUILayout.Width(150)))
        {
            clothing.RandomizeClothes();
        }
    }
}