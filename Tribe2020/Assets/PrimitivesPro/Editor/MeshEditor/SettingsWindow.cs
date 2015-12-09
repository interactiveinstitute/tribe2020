// Version 2.2
// ©2013 Reindeer Games
// All rights reserved
// Redistribution of source code without permission not allowed

using UnityEditor;
using UnityEngine;

namespace PrimitivesPro.Editor
{
    class SettingsWindow : EditorWindow
    {
		private PrimitivesPro.Editor.MeshEditor.MeshEditorSettings settings;
        private static bool shown;

		public static void ShowWindow(PrimitivesPro.Editor.MeshEditor.MeshEditorSettings settings)
        {
            var window = EditorWindow.GetWindow(typeof (SettingsWindow)) as SettingsWindow;
			window.settings = settings;

		    if (!shown)
		    {
                window.minSize = new Vector2(400, 350);
                window.position = new Rect(200, 200, window.minSize.x, window.minSize.y);
		    }

            window.ShowUtility();
		    shown = true;
        }

        public static void Refresh(PrimitivesPro.Editor.MeshEditor.MeshEditorSettings settings)
        {
            if (shown)
            {
                ShowWindow(settings);
            }
        }

        private void OnDestroy()
        {
            shown = false;
        }

        private void OnGUI()
        {
            GUILayout.Space(20);

            var style = GUIStyle.none;
            style.alignment = TextAnchor.MiddleCenter;
            style.fontSize = 16;
            style.fontStyle = FontStyle.Bold;

            Utils.Separator("PrimitivesPro settings", 20, style);
            GUILayout.Space(40);

            style.fontSize = 12;
            style.fontStyle = FontStyle.Normal;
            style.alignment = TextAnchor.MiddleLeft;

            GUILayout.BeginHorizontal();
            GUILayout.Space(30);
			settings.Show = EditorGUILayout.Toggle("Show grid", settings.Show);
            GUILayout.EndHorizontal();

            GUI.enabled = settings.Show;

            GUILayout.Space(20);
            GUILayout.BeginHorizontal();
            GUILayout.Space(30);
			settings.GridSnap = EditorGUILayout.Toggle("Snap on grid", settings.GridSnap);
            GUILayout.EndHorizontal();

            GUILayout.Space(20);
            GUILayout.BeginHorizontal();
            GUILayout.Space(30);
			settings.Dim = EditorGUILayout.Slider("Grid Size", settings.Dim, 1.0f, 100.0f);
            GUILayout.EndHorizontal();

            GUILayout.Space(20);
            GUILayout.BeginHorizontal();
            GUILayout.Space(30);
			settings.Size = (int)EditorGUILayout.Slider("Grid Resolution", settings.Size, 1.0f, 100.0f);
            GUILayout.EndHorizontal();

            GUI.enabled = true;

			GUILayout.Space(20);
			GUILayout.BeginHorizontal();
			GUILayout.Space(30);
			settings.StickOverlappingPoints = EditorGUILayout.Toggle("Sticky overlapping points", settings.StickOverlappingPoints);
			GUILayout.EndHorizontal();

			GUILayout.Space(20);
			GUILayout.BeginHorizontal();
			GUILayout.Space(30);
			settings.VertexSnap = EditorGUILayout.Toggle("Vertex snapping", settings.VertexSnap);
			GUILayout.EndHorizontal();

            GUILayout.Space(40);
            GUILayout.BeginHorizontal();
            GUILayout.Space(100);
            GUILayout.Space(100);
            GUILayout.EndHorizontal();

            GUILayout.Space(10);

            GUILayout.BeginHorizontal();
            GUILayout.Space(100);
            GUILayout.Space(100);
            GUILayout.EndHorizontal();
        }
    }
}
