using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

[CustomEditor(typeof(AvatarActivity))]
public class AvatarActivityEditor : Editor {

    private ReorderableList reorderableSessions;

    private AvatarActivity activity
    {
        get
        {
            return target as AvatarActivity;
        }
    }

    private void OnEnable()
    {
        //reorderableSessions = new ReorderableList(activity.sessions, typeof(AvatarActivity.Session), true, true, true, true);

        // This could be used aswell, but I only advise this your class inherrits from UnityEngine.Object or has a CustomPropertyDrawer
        // Since you'll find your item using: serializedObject.FindProperty("list").GetArrayElementAtIndex(index).objectReferenceValue
        // which is a UnityEngine.Object
        reorderableSessions = new ReorderableList(serializedObject, serializedObject.FindProperty("sessions"), true, true, true, true);

        // Add listeners to draw events
        reorderableSessions.drawHeaderCallback += DrawHeader;
        reorderableSessions.drawElementCallback += DrawElement;

        reorderableSessions.elementHeightCallback += ElementHeight;

        reorderableSessions.onAddCallback += AddItem;
        reorderableSessions.onRemoveCallback += RemoveItem;
    }

    private void OnDisable()
    {
        // Make sure we don't get memory leaks etc.
        reorderableSessions.drawHeaderCallback -= DrawHeader;
        reorderableSessions.drawElementCallback -= DrawElement;

        reorderableSessions.onAddCallback -= AddItem;
        reorderableSessions.onRemoveCallback -= RemoveItem;
    }

    /// <summary>
    /// Draws the header of the list
    /// </summary>
    /// <param name="rect"></param>
    private void DrawHeader(Rect rect)
    {
        GUI.Label(rect, "Sessions");
    }

    /// <summary>
    /// Draws one element of the list (Session)
    /// </summary>
    /// <param name="rect"></param>
    /// <param name="index"></param>
    /// <param name="active"></param>
    /// <param name="focused"></param>
    private void DrawElement(Rect rect, int index, bool active, bool focused)
    {
        EditorGUI.PropertyField(new Rect(rect.x + 18, rect.y, rect.width - 18, rect.height), serializedObject.FindProperty("sessions").GetArrayElementAtIndex(index), true); 
    }

    private float ElementHeight(int index)
    {
        SerializedProperty element = reorderableSessions.serializedProperty.GetArrayElementAtIndex(index);
        return EditorGUI.GetPropertyHeight(element);
    }

    private void AddItem(ReorderableList list)
    {
        activity.sessions.Add(new AvatarActivity.Session());
    }

    private void RemoveItem(ReorderableList list)
    {
        activity.sessions.RemoveAt(list.index);
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update(); // Needed for free good editor functionality

        reorderableSessions.DoLayoutList();
        Editor.DrawPropertiesExcluding(base.serializedObject, "sessions"); //Draw default excluding sessions

        serializedObject.ApplyModifiedProperties(); // Needed for zero-hassle good editor functionality
    }
}