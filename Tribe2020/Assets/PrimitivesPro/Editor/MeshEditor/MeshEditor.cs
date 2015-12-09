// Version 2.2
// ©2013 Reindeer Games
// All rights reserved
// Redistribution of source code without permission not allowed

using System;
using System.Collections.Generic;
using PrimitivesPro.MeshEditor;
using UnityEditor;
using UnityEngine;

namespace PrimitivesPro.Editor.MeshEditor
{
    public enum SelectionType
    {
        None,
        Vertex,
        Polygon,
        Edge,
    }

    [CustomEditor(typeof(PrimitivesPro.MeshEditor.MeshEditorObject))]
    public class MeshEditor : UnityEditor.Editor
    {
        public static MeshEditor Instance;

        private PrimitivesPro.MeshEditor.MeshEditorObject editObject;

        private static readonly Color handlesColorHovered = new Color(1.0f, 1.0f, 0.0f, 0.8f);
        private static readonly Color handlesColorSelected = new Color(1.0f, 0.0f, 0.0f, 0.8f);
        private const float handlesSize = 0.2f;
        private const float vertexSnapTolerance = 0.01f;

        private HashSet<int> vertexSelection;
        private HashSet<PPMesh.Face> faceSelection;
        private HashSet<PPMesh.HalfEdge> edgeSelection;
        private Vector3 oldDragPos;
        private Vector2 multiselectStart;
        private Quaternion handlesRotation;
        private SelectionType selectionType;
        private Tool defaultTool;
        private bool painting;
        private bool multiselecting;
        private Grid grid;
		private MeshEditorSettings settings;

        class EditCommand
        {
            public PPMesh.Face Face;
            public PPMesh.HalfEdge Edge;
            public int Vertex;
            public CommandType Command;

            public void Reset()
            {
                Face = null;
                Command = CommandType.None;
            }

            public bool IsSet(CommandType cmd)
            {
                return (Command & cmd) == cmd;
            }

            [Flags]
            public enum CommandType
            {
                None   = 1 << 0,
                Add    = 1 << 1,
                Remove = 1 << 2,
                Clear  = 1 << 3,
            }
        }

        private EditCommand editCommand;

        void OnEnable()
        {
            if (!editObject)
            {
                editObject = ((PrimitivesPro.MeshEditor.MeshEditorObject) target);
            }

            if (!editObject)
            {
                Instance = null;
                return;
            }

            editObject.Init();

            if (Toolbar.Instance)
            {
                selectionType = Toolbar.Instance.MeshSelection;
            }

            vertexSelection = new HashSet<int>();
            faceSelection = new HashSet<PPMesh.Face>();
            edgeSelection = new HashSet<PPMesh.HalfEdge>();

			settings = new MeshEditorSettings();
			settings.Deserialize();

            if (grid == null)
            {
                grid = new Grid(settings);
            }

            editCommand = new EditCommand
            {
                Command = EditCommand.CommandType.None,
                Face = null
            };

            defaultTool = Tools.current;
            if (defaultTool == Tool.None)
            {
                defaultTool = Tool.Move;
            }

            handlesRotation = Quaternion.identity;

            Instance = this;
        }

        public bool IsValid(GameObject obj)
        {
            return obj == editObject.gameObject;
        }

        private void OnSelectionChanged()
        {
            faceSelection.Clear();
            edgeSelection.Clear();
            vertexSelection.Clear();
            editCommand.Reset();

            if (selectionType == SelectionType.None)
            {
                Tools.current = defaultTool;
            }
        }

        public void ShowGrid()
        {
            grid.ShowToggle();
        }

        public Grid GetGrid()
        {
            return grid;
        }

		public MeshEditorSettings GetSettings()
		{
			return settings;
		}

        public bool IsGridVisible()
        {
            return grid.IsVisible();
        }

        public void OnMeshApply()
        {
            editObject.ppMesh.ApplyToMesh();
        }

        public void OnUndo()
        {
            editObject.ppMesh.OnUndo();
        }

        public void OnRedo()
        {
            editObject.ppMesh.OnRedo();
        }

        public bool CanUndo()
        {
            return editObject.ppMesh.CanUndo();
        }

        public bool CanRedo()
        {
            return editObject.ppMesh.CanRedo();
        }

        public void RefreshToolbar()
        {
            Toolbar.Instance.Repaint();
        }

        void UpdateSceneViewGUI()
        {
            Handles.BeginGUI();

            GUI.contentColor = Color.gray;
            var fontStyle = new GUIStyle("label");
            const string content = "PrimitivesPro Mesh Editor";
            fontStyle.fontStyle = FontStyle.Bold;
            fontStyle.fontSize = 20;
            var size = fontStyle.CalcSize(new GUIContent(content));
            fontStyle.fixedHeight = size.y;
            fontStyle.fixedWidth = size.x;

            var boxStyle = new GUIStyle("box");
            boxStyle.fixedHeight = size.y;
            boxStyle.fixedWidth = size.x;
            fontStyle.fontStyle = FontStyle.Bold;
            boxStyle.fontSize = 15;

            GUILayout.BeginHorizontal();
            EditorGUILayout.Space();
            GUILayout.Box(content, boxStyle);
            EditorGUILayout.Space();
            GUILayout.EndHorizontal();

            if (multiselecting)
            {
                var currMouse = Event.current.mousePosition;
                var selectStyle = new GUIStyle(GUI.skin.box);
                selectStyle.border = new RectOffset(2, 2, 2, 2);
                GUI.color = new Color(0.3f, 0.3f, 0.3f, 0.5f);

                var p0 = multiselectStart;
                var p1 = currMouse;

                if (p0.x > p1.x)
                {
                    MeshUtils.Swap(ref p0.x, ref p1.x);
                }
                if (p0.y > p1.y)
                {
                    MeshUtils.Swap(ref p0.y, ref p1.y);
                }

                var rect = new Rect(p0.x, p0.y, p1.x - p0.x, p1.y - p0.y);

                GUI.Box(rect, string.Empty, selectStyle);
            }

            Handles.EndGUI();
        }

        void OnSceneGUI()
        {
            if (Toolbar.Instance)
            {
                if (selectionType != Toolbar.Instance.MeshSelection)
                {
                    selectionType = Toolbar.Instance.MeshSelection;
                    OnSelectionChanged();
                }
            }

            UpdateSceneViewGUI();

            UpdateMouseEditing();

            grid.Draw();

            HandleUtility.Repaint();
        }

        void UpdateMouseEditing()
        {
            if (!editObject)
            {
                return;
            }

            switch (selectionType)
            {
                case SelectionType.Vertex:
                    Tools.current = Tool.None;
                    HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
                    UpdateVertexEditing();
                    break;

                case SelectionType.Polygon:
                    Tools.current = Tool.None;
                    HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
                    UpdatePolygonEditing();
                    break;

                case SelectionType.Edge:
                    Tools.current = Tool.None;
                    HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
                    UpdateEdgesEditing();
                    break;
            }
        }

        void OnMultiselectFinished()
        {
            if (multiselecting)
            {
                multiselecting = false;

                var currMouse = Event.current.mousePosition;
                var p0 = multiselectStart;
                var p1 = currMouse;

                if (p0.x > p1.x)
                {
                    MeshUtils.Swap(ref p0.x, ref p1.x);
                }
                if (p0.y > p1.y)
                {
                    MeshUtils.Swap(ref p0.y, ref p1.y);
                }

                var rect = new Rect(p0.x, p0.y, p1.x - p0.x, p1.y - p0.y);

                switch (selectionType)
                {
                    case SelectionType.Vertex:
                        vertexSelection.UnionWith(editObject.ppMesh.GetPointsInRect2D(rect));
                        break;

                    case SelectionType.Polygon:
                        faceSelection.UnionWith(editObject.ppMesh.GetPolygonsInRect2D(rect));
                        break;

                    case SelectionType.Edge:
                        edgeSelection.UnionWith(editObject.ppMesh.GetEdgesInRect2D(rect));
                        break;
                }
            }
        }

        void UpdateVertexEditing()
        {
            var guiEvent = Event.current.type;

            var mouseRay = Utils.GetSceneViewMouseRay();
            var vertex = -1;
            editObject.ppMesh.GetNearestPoint(mouseRay, out vertex, true);

            if (vertex != -1)
            {
                var p0 = HandleUtility.WorldToGUIPoint(editObject.ppMesh.GetVertexByIndexWorld(vertex));
                var p1 = Event.current.mousePosition;
                var distance = (p0 - p1).sqrMagnitude;

                if (distance > 30*30)
                {
                    vertex = -1;
                }
            }

            var hoverPoint = guiEvent != EventType.mouseDrag;
            var handleSelected = GUIUtility.hotControl != 0;

            if (handleSelected)
            {
                editCommand.Reset();
            }

            if (editCommand.IsSet(EditCommand.CommandType.Clear))
            {
                vertexSelection.Clear();
            }
            else if (editCommand.IsSet(EditCommand.CommandType.Add))
            {
                vertexSelection.Add(editCommand.Vertex);

                if (settings.StickOverlappingPoints)
                {
                    editObject.ppMesh.AddOverlappingVertices(vertexSelection, editCommand.Vertex);
                }
            }
            else if (editCommand.IsSet(EditCommand.CommandType.Remove))
            {
                vertexSelection.Remove(editCommand.Vertex);
            }

            editCommand.Reset();

            if (!Event.current.alt)
            {
                if (vertex != -1)
                {
                    if (guiEvent == EventType.mouseDown)
                    {
                        painting = true;
                    }
                    else if (guiEvent == EventType.mouseUp || guiEvent == EventType.mouseMove)
                    {
                        painting = false;
                    }
                    // adding
                    if (Event.current.shift)
                    {
                        if (painting)
                        {
                            editCommand.Command |= EditCommand.CommandType.Add;
                            editCommand.Vertex = vertex;
                        }
                    }
                    // removing
                    else if (Event.current.control)
                    {
                        if (painting)
                        {
                            editCommand.Command |= EditCommand.CommandType.Remove;
                            editCommand.Vertex = vertex;
                        }
                    }
                    else
                    {
                        if (guiEvent == EventType.mouseDown)
                        {
                            editCommand.Command |= EditCommand.CommandType.Clear;
                            editCommand.Vertex = -1;
                        }

                        if (painting)
                        {
                            editCommand.Command |= EditCommand.CommandType.Add;
                            editCommand.Vertex = vertex;
                        }
                    }
                }
                else
                {
                    if (guiEvent == EventType.mouseDown)
                    {
                        if (!Event.current.shift)
                        {
                            editCommand.Command |= EditCommand.CommandType.Clear;
                        }
                        editCommand.Vertex = -1;
                        multiselectStart = Event.current.mousePosition;
                        multiselecting = true;
                    }
                }
            }
            else
            {
                painting = false;
                multiselecting = false;
            }

            if (guiEvent == EventType.mouseUp)
            {
                OnMultiselectFinished();
            }

            if (handleSelected)
            {
                multiselecting = false;
                painting = false;
            }

            var handleSize = HandleUtility.GetHandleSize(editObject.transform.position)*0.1f;

            foreach (var vert in vertexSelection)
            {
                Handles.color = handlesColorSelected;
                Handles.CubeCap(1, editObject.ppMesh.GetVertexByIndexWorld(vert), Quaternion.identity, handleSize);
            }

            if (!handleSelected)
            {
                if (guiEvent == EventType.mouseUp)
                {
                    var array = new int[vertexSelection.Count];
                    vertexSelection.CopyTo(array);
                    oldDragPos = editObject.ppMesh.GetAveragePos(array);
                    editObject.ppMesh.StartUpdateVerticesDelta(vertexSelection, settings.StickOverlappingPoints);

                    if (Tools.pivotRotation == PivotRotation.Local)
                    {
                        if (vertexSelection.Count > 0)
                        {
                            handlesRotation = editObject.ppMesh.GetLocalRotation(vertexSelection);
                        }
                    }
                    else
                    {
                        handlesRotation = Quaternion.identity;
                    }
                }
            }

            if (guiEvent == EventType.mouseUp)
            {
                editObject.ppMesh.SaveUndo();
                RefreshToolbar();
            }

            var newPos = Handles.PositionHandle(oldDragPos, handlesRotation);

            if (guiEvent == EventType.mouseDrag && handleSelected)
            {
                if (settings.GridSnap)
                {
                    newPos = grid.FindClosestGridPointXZ(newPos);
                }
				else if (settings.VertexSnap)
				{
				    var retPos = Vector3.zero;

                    if (editObject.ppMesh.FindClosestPoint(newPos, vertexSelection, vertexSnapTolerance, ref retPos))
				    {
				        newPos = retPos;
				    }
				}

                var diff = editObject.ppMesh.Transform.InverseTransformPoint(newPos) - editObject.ppMesh.Transform.InverseTransformPoint(oldDragPos);

                editObject.ppMesh.UpdateVerticesDelta(diff);

                editObject.UpdateMesh();
            }

            oldDragPos = newPos;

            if (!handleSelected && vertex != -1 && hoverPoint && !vertexSelection.Contains(vertex))
            {
                Handles.color = handlesColorHovered;
                Handles.CubeCap(1, editObject.ppMesh.GetVertexByIndexWorld(vertex), Quaternion.identity, handleSize);
            }
        }

        private void UpdatePolygonEditing()
        {
            var guiEvent = Event.current.type;

            var mouseRay = Utils.GetSceneViewMouseRay();
            var face = editObject.ppMesh.GetIntersectingPolygon(mouseRay, true);
            var hoverPoint = guiEvent != EventType.mouseDrag;
            var handleSelected = GUIUtility.hotControl != 0;

            if (handleSelected)
            {
                editCommand.Reset();
            }

            if (editCommand.IsSet(EditCommand.CommandType.Clear))
            {
                faceSelection.Clear();
            }
            else if (editCommand.IsSet(EditCommand.CommandType.Add))
            {
                faceSelection.Add(editCommand.Face);
            }
            else if (editCommand.IsSet(EditCommand.CommandType.Remove))
            {
                faceSelection.Remove(editCommand.Face);
            }

            editCommand.Reset();

            if (!Event.current.alt)
            {
                if (face != null)
                {
                    if (guiEvent == EventType.mouseDown)
                    {
                        painting = true;
                    }
                    else if (guiEvent == EventType.mouseUp || guiEvent == EventType.mouseMove)
                    {
                        painting = false;
                    }
                    // adding
                    if (Event.current.shift)
                    {
                        if (painting)
                        {
                            editCommand.Command |= EditCommand.CommandType.Add;
                            editCommand.Face = face;
                        }
                    }
                    // removing
                    else if (Event.current.control)
                    {
                        if (painting)
                        {
                            editCommand.Command |= EditCommand.CommandType.Remove;
                            editCommand.Face = face;
                        }
                    }
                    else
                    {
                        if (guiEvent == EventType.mouseDown)
                        {
                            editCommand.Command |= EditCommand.CommandType.Clear;
                            editCommand.Face = null;
                        }

                        if (painting)
                        {
                            editCommand.Command |= EditCommand.CommandType.Add;
                            editCommand.Face = face;
                        }
                    }
                }
                else
                {
                    if (guiEvent == EventType.mouseDown)
                    {
                        if (!Event.current.shift)
                        {
                            editCommand.Command |= EditCommand.CommandType.Clear;
                        }
                        editCommand.Face = null;
                        multiselectStart = Event.current.mousePosition;
                        multiselecting = true;
                    }
                }
            }
            else
            {
                painting = false;
                multiselecting = false;
            }

            if (guiEvent == EventType.mouseUp)
            {
                OnMultiselectFinished();
            }

            if (handleSelected)
            {
                multiselecting = false;
                painting = false;
            }

            var pointList = new int[faceSelection.Count*3];
            var i = 0;

            foreach (var poly in faceSelection)
            {
                Handles.color = handlesColorSelected;
                Handles.DrawSolidRectangleWithOutline(editObject.ppMesh.GetFaceRectangle(poly), Color.red, Color.black);

                pointList[i++] = poly.p0;
                pointList[i++] = poly.p1;
                pointList[i++] = poly.p2;
            }

            if (!handleSelected)
            {
                if (guiEvent == EventType.mouseUp)
                {
                    oldDragPos = editObject.ppMesh.GetAveragePos(pointList);
					editObject.ppMesh.StartUpdateFacesDelta(faceSelection, settings.StickOverlappingPoints);

                    if (Tools.pivotRotation == PivotRotation.Local)
                    {
                        if (faceSelection.Count > 0)
                        {
                            handlesRotation = editObject.ppMesh.GetLocalRotation(faceSelection);
                        }
                    }
                    else
                    {
                        handlesRotation = Quaternion.identity;
                    }
                }
            }

            if (guiEvent == EventType.mouseUp)
            {
                editObject.ppMesh.SaveUndo();
                RefreshToolbar();
            }

            var newPos = Handles.PositionHandle(oldDragPos, handlesRotation);

            if (guiEvent == EventType.mouseDrag && handleSelected)
            {
                if (settings.GridSnap)
                {
                    newPos = grid.FindClosestGridPointXZ(newPos);
                }

                var diff = editObject.ppMesh.Transform.InverseTransformPoint(newPos) - editObject.ppMesh.Transform.InverseTransformPoint(oldDragPos);

                editObject.ppMesh.UpdateFacesDelta(diff);

                editObject.UpdateMesh();
            }

            oldDragPos = newPos;

            if (!handleSelected && face != null && hoverPoint && !faceSelection.Contains(face))
            {
                Handles.color = handlesColorHovered;
                Handles.DrawSolidRectangleWithOutline(editObject.ppMesh.GetFaceRectangle(face), Color.yellow, Color.black);
            }
        }

        void UpdateEdgesEditing()
        {
            var guiEvent = Event.current.type;

            var mouseRay = Utils.GetSceneViewMouseRay();
            PPMesh.HalfEdge edge = null;
            editObject.ppMesh.GetNearestEdge(mouseRay, out edge, true);

            if (edge != null)
            {
                var line = editObject.ppMesh.GetEdgeLine(edge);
                var p0  = HandleUtility.WorldToGUIPoint(line[0]);
                var p1 = HandleUtility.WorldToGUIPoint(line[1]);
                var m = Event.current.mousePosition;

                var distance = HandleUtility.DistancePointLine(m, p0, p1);

                if (distance > 30)
                {
                    edge = null;
                }
            }

            var hoverPoint = guiEvent != EventType.mouseDrag;
            var handleSelected = GUIUtility.hotControl != 0;

            if (handleSelected)
            {
                editCommand.Reset();
            }

            if (editCommand.IsSet(EditCommand.CommandType.Clear))
            {
                edgeSelection.Clear();
            }
            else if (editCommand.IsSet(EditCommand.CommandType.Add))
            {
                edgeSelection.Add(editCommand.Edge);
            }
            else if (editCommand.IsSet(EditCommand.CommandType.Remove))
            {
                edgeSelection.Remove(editCommand.Edge);
            }

            editCommand.Reset();

            if (!Event.current.alt)
            {
                if (edge != null)
                {
                    if (guiEvent == EventType.mouseDown)
                    {
                        painting = true;
                    }
                    else if (guiEvent == EventType.mouseUp || guiEvent == EventType.mouseMove)
                    {
                        painting = false;
                    }
                    // adding
                    if (Event.current.shift)
                    {
                        if (painting)
                        {
                            editCommand.Command |= EditCommand.CommandType.Add;
                            editCommand.Edge = edge;
                        }
                    }
                    // removing
                    else if (Event.current.control)
                    {
                        if (painting)
                        {
                            editCommand.Command |= EditCommand.CommandType.Remove;
                            editCommand.Edge = edge;
                        }
                    }
                    else
                    {
                        if (guiEvent == EventType.mouseDown)
                        {
                            editCommand.Command |= EditCommand.CommandType.Clear;
                            editCommand.Edge = null;
                        }

                        if (painting)
                        {
                            editCommand.Command |= EditCommand.CommandType.Add;
                            editCommand.Edge = edge;
                        }
                    }
                }
                else
                {
                    if (guiEvent == EventType.mouseDown)
                    {
                        if (!Event.current.shift)
                        {
                            editCommand.Command |= EditCommand.CommandType.Clear;
                        }
                        editCommand.Edge = null;
                        multiselectStart = Event.current.mousePosition;
                        multiselecting = true;
                    }
                }
            }
            else
            {
                painting = false;
                multiselecting = false;
            }

            if (guiEvent == EventType.mouseUp)
            {
                OnMultiselectFinished();
            }

            if (handleSelected)
            {
                multiselecting = false;
                painting = false;
            }

            var array = new int[edgeSelection.Count * 2];
            int i = 0;

            foreach (var e in edgeSelection)
            {
                Handles.color = handlesColorSelected;

                array[i++] = e.point;
                array[i++] = e.nextEdge.point;

                var edgeLine = editObject.ppMesh.GetEdgeLine(e);
                Handles.DrawLine(edgeLine[0], edgeLine[1]);
            }

            if (!handleSelected)
            {
                if (guiEvent == EventType.mouseUp)
                {
                    oldDragPos = editObject.ppMesh.GetAveragePos(array);
					editObject.ppMesh.StartUpdateEdgesDelta(edgeSelection, settings.StickOverlappingPoints);

                    if (Tools.pivotRotation == PivotRotation.Local)
                    {
                        if (edgeSelection.Count > 0)
                        {
                            handlesRotation = editObject.ppMesh.GetLocalRotation(edgeSelection);
                        }
                    }
                    else
                    {
                        handlesRotation = Quaternion.identity;
                    }
                }
            }

            if (guiEvent == EventType.mouseUp)
            {
                editObject.ppMesh.SaveUndo();
                RefreshToolbar();
            }

            var newPos = Handles.PositionHandle(oldDragPos, handlesRotation);

            if (guiEvent == EventType.mouseDrag && handleSelected)
            {
                if (settings.GridSnap)
                {
                    newPos = grid.FindClosestGridPointXZ(newPos);
                }

                var diff = editObject.ppMesh.Transform.InverseTransformPoint(newPos) - editObject.ppMesh.Transform.InverseTransformPoint(oldDragPos);

                editObject.ppMesh.UpdateEdgesDelta(diff);

                editObject.UpdateMesh();
            }

            oldDragPos = newPos;

            if (!handleSelected && edge != null && hoverPoint && !edgeSelection.Contains(edge))
            {
                Handles.color = handlesColorHovered;

                var edgeLine = editObject.ppMesh.GetEdgeLine(edge);
                Handles.DrawLine(edgeLine[0], edgeLine[1]);
            }
        }
    }
}
