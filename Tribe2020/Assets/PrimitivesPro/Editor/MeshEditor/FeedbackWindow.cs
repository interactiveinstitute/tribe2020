// Version 2.2
// ©2013 Reindeer Games
// All rights reserved
// Redistribution of source code without permission not allowed

using UnityEditor;
using UnityEngine;

namespace PrimitivesPro.Editor
{
    class FeedbackWindow : EditorWindow
    {
        public static void ShowWindow()
        {
            var window = EditorWindow.GetWindow(typeof(FeedbackWindow));

            window.minSize = new Vector2(640, 300);
            window.position = new Rect(200, 200, window.minSize.x, window.minSize.y);
            window.ShowUtility();
        }

        private void OnGUI()
        {
            GUILayout.Space(20);

            var style = GUIStyle.none;
            style.alignment = TextAnchor.MiddleCenter;
            style.fontSize = 16;
            style.fontStyle = FontStyle.Bold;

            Utils.Separator("PrimitivesPro Feedback", 20, style);
            GUILayout.Space(40);

            style.fontSize = 12;
            style.fontStyle = FontStyle.Normal;
            style.alignment = TextAnchor.MiddleLeft;

            GUILayout.Label("     If you have any questions, bug reports, suggestions, feature requests, send me a message!\n", style);

            GUILayout.Space(40);
            GUILayout.BeginHorizontal();
            GUILayout.Space(100);
            if (GUILayout.Button("Send e-mail to gamesreindeer@gmail.com", GUILayout.Height(30)))
            {
                SendEmail();
                this.Close();
            }
            GUILayout.Space(100);
            GUILayout.EndHorizontal();

            GUILayout.Space(10);

            GUILayout.BeginHorizontal();
            GUILayout.Space(100);
            if (GUILayout.Button("Visit Unity Forum", GUILayout.Height(30)))
            {
                VisitUnityForum();
                this.Close();
            }
            GUILayout.Space(100);
            GUILayout.EndHorizontal();
        }

        void SendEmail()
        {
            Application.OpenURL("mailto://gamesreindeer@gmail.com");
        }

        void VisitUnityForum()
        {
            Application.OpenURL("http://forum.unity3d.com/threads/primitivespro-released.173575/");
        }
    }
}
