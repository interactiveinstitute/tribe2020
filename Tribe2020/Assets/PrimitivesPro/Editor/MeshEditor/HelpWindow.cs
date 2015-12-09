// Version 2.2
// ©2013 Reindeer Games
// All rights reserved
// Redistribution of source code without permission not allowed

using UnityEditor;
using UnityEngine;

namespace PrimitivesPro.Editor
{
    class HelpWindow : EditorWindow
    {
        public static void ShowWindow()
        {
            var window = EditorWindow.GetWindow(typeof(HelpWindow));

            window.minSize = new Vector2(576, 600);
            window.position = new Rect(200, 200, window.minSize.x, window.minSize.y);
            window.maxSize = window.minSize;
            window.ShowUtility();
        }

        private void OnGUI()
        {
            if (GUILayout.Button(Resources.Load<Texture2D>("helpscreen")))
            {
                Close();
            }
        }
    }
}
