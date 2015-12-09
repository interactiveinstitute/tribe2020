// Version 2.2
// ©2013 Reindeer Games
// All rights reserved
// Redistribution of source code without permission not allowed

using System.CodeDom;
using System.Linq;
using PrimitivesPro.Editor.MeshEditor;
using UnityEditor;
using UnityEngine;

namespace PrimitivesPro.Editor
{
    public class Toolbar : EditorWindow
    {
        public static Toolbar Instance;
        public MeshEditor.SelectionType MeshSelection { get; private set; }

        private bool showPrimitives = true;
        private bool enableMeshEditor = false;
        private GUIStyle buttonStyle;
        private GameObject selectedObject;

        private class ToolButton
        {
            public string icon;
            public System.Type commandType;
            public System.Enum commandEnum;
            public bool enabled = true;
        }

        private delegate void OnToolButton(ToolButton button);

        private readonly ToolButton[] primitivesIcons = new ToolButton[]
        {
            // 2d primitives
            new ToolButton{ commandType = typeof(CreateTriangle), icon = "icons/triangle", },
            new ToolButton{ commandType = typeof(CreatePlane), icon = "icons/plane", },
            new ToolButton{ commandType = typeof(CreateCircle), icon = "icons/circle", },
            new ToolButton{ commandType = typeof(CreateEllipse), icon = "icons/ellipse", },
            new ToolButton{ commandType = typeof(CreateRing), icon = "icons/ring", },

            // 3d primitives
            new ToolButton{ commandType = typeof(CreateBox), icon = "icons/box", },
            new ToolButton{ commandType = typeof(CreateSphere), icon = "icons/sphere", },
            new ToolButton{ commandType = typeof(CreateGeoSphere), icon = "icons/geosphere", },
            new ToolButton{ commandType = typeof(CreateEllipsoid), icon = "icons/ellipsoid", },
            new ToolButton{ commandType = typeof(CreateCapsule), icon = "icons/capsule", },
            new ToolButton{ commandType = typeof(CreateCylinder), icon = "icons/cylinder", },
            new ToolButton{ commandType = typeof(CreateCone), icon = "icons/cone", },
            new ToolButton{ commandType = typeof(CreateTube), icon = "icons/tube", },
            new ToolButton{ commandType = typeof(CreatePyramid), icon = "icons/pyramid", },
            new ToolButton{ commandType = typeof(CreateTorus), icon = "icons/torus", },

            // special primitives
            new ToolButton{ commandType = typeof(CreateArc), icon = "icons/arc", },
            new ToolButton{ commandType = typeof(CreateRoundedCube), icon = "icons/roundedBox", },
            new ToolButton{ commandType = typeof(CreateSphericalCone), icon = "icons/sphericalCone", },
            new ToolButton{ commandType = typeof(CreateSuperEllipsoid), icon = "icons/superellipsoid", },
            new ToolButton{ commandType = typeof(CreateTorusKnot), icon = "icons/torusKnot", },
        };

        private readonly ToolButton[] meshSelectionButtons = new ToolButton[]
        {
            new ToolButton { commandEnum = SelectionType.None,    icon = "icons/objectEdit"     },
            new ToolButton { commandEnum = SelectionType.Vertex,  icon = "icons/verticesEdit"   },
            new ToolButton { commandEnum = SelectionType.Polygon, icon = "icons/polygonEdit"    },
            new ToolButton { commandEnum = SelectionType.Edge,    icon = "icons/edgesEdit"      },
        };

        private readonly ToolButton[] systemButtons = new ToolButton[]
        {
            new ToolButton { commandEnum = SystemCommands.Save, icon = "icons/save" },
            new ToolButton { commandEnum = SystemCommands.Undo, icon = "icons/undo" },
            new ToolButton { commandEnum = SystemCommands.Redo, icon = "icons/redo" },
            new ToolButton { commandEnum = SystemCommands.Help, icon = "icons/help" },
            new ToolButton { commandEnum = SystemCommands.Grid, icon = "icons/grid" },
			new ToolButton { commandEnum = SystemCommands.Settings, icon = "icons/settings" },
        };

        private enum ExportOptions
        {
            UnityMesh,
            Prefab,
            OBJ
        }

        private enum GridOptions
        {
            Enable,
            Settings,
        }

        private enum SystemCommands
        {
            Save,
            Undo,
            Redo,
            Help,
            Grid,
			Settings,
        }

        [MenuItem("Window/PrimitivesPro")]
        public static void ShowWindow()
        {
            var window = EditorWindow.GetWindow(typeof(Toolbar));
            window.minSize = new Vector2(80, window.minSize.y);
        }

        void OnEnable()
        {
            MeshSelection = SelectionType.None;
            Instance = this;
        }

        private void OnGUI()
        {
            //
            // create primitives buttons
            //
            GUILayout.BeginVertical();
            if (GUILayout.Button("Primitives"))
            {
                showPrimitives = !showPrimitives;
            }
            GUILayout.EndVertical();

            var buttonsPerLine = (int)position.width / 45;
            buttonStyle = new GUIStyle("button")
            {
                fixedHeight = 40,
                fixedWidth = 40
            };

            if (showPrimitives)
            {
                EnableButtons(primitivesIcons, GUI.enabled);
                DisplayButtonSection(primitivesIcons, buttonsPerLine, buttonStyle, OnPrimitivesButton);
            }

            EditorGUILayout.Separator();

            //
            // mesh editor
            //
            var defaultColor = GUI.color;
            GUI.color = enableMeshEditor ? Color.green : Color.red;
            GUI.enabled = IsObjectSelected();

            if (GUILayout.Button("Mesh editor"))
            {
                enableMeshEditor = !enableMeshEditor;
                OnEnableMeshEditor();
            }

            GUI.color = defaultColor;
            GUI.enabled = IsObjectSelected() && enableMeshEditor;

            EnableButtons(systemButtons, GUI.enabled);

            if (IsObjectSelected() && enableMeshEditor)
            {
                var meshEdit = selectedObject.GetComponent<PrimitivesPro.MeshEditor.MeshEditorObject>();

                if (meshEdit && meshEdit.ppMesh != null)
                {
                    systemButtons[0].enabled = GUI.enabled;
                    systemButtons[1].enabled = meshEdit.ppMesh.CanUndo();
                    systemButtons[2].enabled = meshEdit.ppMesh.CanRedo();
                }
            }

            systemButtons[3].enabled = true;

            DisplayButtonSection(systemButtons, buttonsPerLine, buttonStyle, OnSystemButton, null);

            EnableButtons(meshSelectionButtons, GUI.enabled);
            DisplayButtonSection(meshSelectionButtons, buttonsPerLine, buttonStyle, OnMeshEditorButton, MeshSelection);
            EditorGUILayout.Separator();

            GUI.enabled = true;

            DisplayFeedback();
        }

        void EnableButtons(ToolButton[] buttons, bool enable)
        {
            foreach (var toolButton in buttons)
            {
                toolButton.enabled = enable;
            }
        }

        private void DisplayFeedback()
        {
            if (GUI.Button(new Rect(5, position.height-50, position.width-10, 30), "Feedback"))
            {
                FeedbackWindow.ShowWindow();
            }

            GUI.Label(new Rect(5, position.height - 20, position.width - 10, 30), "Version 2.1");
        }

        private void DisplayButtonSection(ToolButton[] btns, int buttonsPerLine, GUIStyle btnStyle, OnToolButton callback, System.Enum selection = null)
        {
            var buttons = btns.Count();
            var btn = 0;

            var guiEnabled = GUI.enabled;

            while (btn < buttons)
            {
                GUILayout.BeginHorizontal();

                var defaultColor = GUI.backgroundColor;

                for (var i = 0; i < buttonsPerLine; i++)
                {
                    var meshCommand = btns[btn];

                    if (selection != null && Equals(meshCommand.commandEnum, selection))
                    {
                        GUI.backgroundColor = Color.gray;
                    }

                    GUI.enabled = meshCommand.enabled;

                    if (GUILayout.Button(Resources.Load(meshCommand.icon) as Texture2D, btnStyle))
                    {
                        callback(meshCommand);
                    }

                    btn++;

                    GUI.backgroundColor = defaultColor;

                    if (btn >= buttons)
                        break;
                }

                GUILayout.EndHorizontal();
            }

            GUI.enabled = guiEnabled;
        }

        private void SaveCallback(object obj)
        {
            var exportOptions = (ExportOptions) obj;

            switch (exportOptions)
            {
                case ExportOptions.UnityMesh:
                    Utils.SaveMesh(selectedObject);
                    break;

                case ExportOptions.Prefab:
                    Utils.SavePrefab(selectedObject);
                    break;

                case ExportOptions.OBJ:
                    Utils.ExportToOBJ(selectedObject);
                    break;
            }
        }

        void OnMeshEditorButton(ToolButton cmd)
        {
            switch ((MeshEditor.SelectionType)cmd.commandEnum)
            {
                case SelectionType.Vertex:
                    MeshSelection = SelectionType.Vertex;
                    break;

                case SelectionType.Polygon:
                    MeshSelection = SelectionType.Polygon;
                    break;

                case SelectionType.Edge:
                    MeshSelection = SelectionType.Edge;
                    break;

                case SelectionType.None:
                    MeshSelection = SelectionType.None;
                    break;
            }
        }

        void OnPrimitivesButton(ToolButton btn)
        {
            var command = btn.commandType;
            var method = command.GetMethod("Create");

            method.Invoke(null, null);

            if (Selection.activeGameObject)
            {
                Utils.FocusSceneCamera(Selection.activeGameObject);
            }
        }

        void OnSystemButton(ToolButton btn)
        {
            switch ((SystemCommands) btn.commandEnum)
            {
                case SystemCommands.Save:
                {
                    var menu = new GenericMenu();

                    menu.AddItem(new GUIContent("Save as mesh"),   false, SaveCallback, ExportOptions.UnityMesh);
                    menu.AddItem(new GUIContent("Save as prefab"), false, SaveCallback, ExportOptions.Prefab);
                    menu.AddItem(new GUIContent("Export to OBJ"),  false, SaveCallback, ExportOptions.OBJ);
                    menu.ShowAsContext();
                }
                break;

                case SystemCommands.Undo:
                    OnUndo();
                    break;

                case SystemCommands.Redo:
                    OnRedo();
                    break;

                case SystemCommands.Help:
                    HelpWindow.ShowWindow();
                    break;

                case SystemCommands.Grid:
                {
					ShowGrid();
                }
                break;

				case SystemCommands.Settings:
				{
					ShowGridSettings();
				}
				break;
            }
        }

        private void OnUndo()
        {
            var meshEditor = MeshEditor.MeshEditor.Instance;

            if (meshEditor != null && meshEditor.IsValid(selectedObject))
            {
                meshEditor.OnUndo();
            }
        }

        private void OnRedo()
        {
            var meshEditor = MeshEditor.MeshEditor.Instance;

            if (meshEditor != null && meshEditor.IsValid(selectedObject))
            {
                meshEditor.OnRedo();
            }
        }

        private void ShowGrid()
        {
            var meshEditor = MeshEditor.MeshEditor.Instance;

            if (meshEditor != null && meshEditor.IsValid(selectedObject))
            {
                meshEditor.ShowGrid();
            }
        }

        private void ShowGridSettings()
        {
            var meshEditor = MeshEditor.MeshEditor.Instance;

            if (meshEditor != null && meshEditor.IsValid(selectedObject))
            {
                SettingsWindow.ShowWindow(meshEditor.GetSettings());
            }
        }

        private bool IsGridVisible()
        {
            var meshEditor = MeshEditor.MeshEditor.Instance;

            if (meshEditor != null && meshEditor.IsValid(selectedObject))
            {
                return meshEditor.IsGridVisible();
            }

            return false;
        }

        private void EnableMeshEditor(GameObject obj)
        {
            if (!obj.GetComponent<PrimitivesPro.MeshEditor.MeshEditorObject>())
            {
                var meshes = obj.GetComponent<MeshFilter>();

                if (meshes)
                {
                    obj.AddComponent<PrimitivesPro.MeshEditor.MeshEditorObject>();
                    enableMeshEditor = true;
//                    SettingsWindow.Refresh(MeshEditor.MeshEditor.Instance.GetSettings());
                }
            }
        }

        private bool IsEditable(GameObject obj)
        {
            return obj && obj.GetComponent<MeshFilter>() && obj.GetComponent<MeshFilter>().sharedMesh;
        }

        private void DisableMeshEditor(GameObject obj)
        {
            DestroyImmediate(obj.GetComponent<PrimitivesPro.MeshEditor.MeshEditorObject>());
            enableMeshEditor = false;
        }

        private bool IsObjectSelected()
        {
            return selectedObject;
        }

        void OnDestroy()
        {
            if (selectedObject)
            {
                DisableMeshEditor(selectedObject);
                selectedObject = null;
            }
        }

        void OnEnableMeshEditor()
        {
            if (enableMeshEditor)
            {
                EnableMeshEditor(selectedObject);
            }
            else
            {
                DisableMeshEditor(selectedObject);
            }
        }

        private void Update()
        {
            var active = Selection.activeGameObject;
            var objs = Selection.GetFiltered(typeof(GameObject), SelectionMode.ExcludePrefab);

            if (objs.Contains(active) || !active)
            {
                if (selectedObject != active)
                {
                    if (!IsEditable(active))
                    {
                        active = null;
                    }

                    selectedObject = active;

                    if (selectedObject)
                    {
                        enableMeshEditor = selectedObject.GetComponent<PrimitivesPro.MeshEditor.MeshEditorObject>();
                    }

                    Repaint();
                }
            }
        }
    }
}
