using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(AvatarModel))]
public class ClothingEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        AvatarModel clothing = (AvatarModel)target;

        //Set dirty, in order to save new data on game start
        EditorUtility.SetDirty(clothing);

        if (GUILayout.Button("Randomize clothes", GUILayout.Width(150)))
        {
            clothing.RandomizeClothes();
        }
    }
}