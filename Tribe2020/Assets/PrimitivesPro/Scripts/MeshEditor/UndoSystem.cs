using System.Collections.Generic;
using UnityEngine;

namespace PrimitivesPro.MeshEditor
{
    class UndoSystem
    {
        private readonly LinkedList<RestorePoint> restorePoints;
        private readonly Dictionary<int, Vector3> changedVertex;
        private LinkedListNode<RestorePoint> current;

        public UndoSystem()
        {
            restorePoints = new LinkedList<RestorePoint>();
            changedVertex = new Dictionary<int, Vector3>();

            current = restorePoints.AddLast(new RestorePoint(changedVertex));
        }

        public void CreateRestorePoint()
        {
            if (changedVertex.Count > 0)
            {
                // delete remaining in case of branching undo/redo
                if (current != restorePoints.First && current != restorePoints.Last)
                {
                    while (current != restorePoints.Last)
                    {
                        restorePoints.RemoveLast();
                    }
                }

                current = restorePoints.AddLast(new RestorePoint(changedVertex));
                changedVertex.Clear();
            }
        }

        public void OnVertexChanged(int index, Vector3 delta)
        {
            if (delta.sqrMagnitude > Mathf.Epsilon)
            {
                if (!changedVertex.ContainsKey(index))
                {
                    changedVertex[index] = Vector3.zero;
                }

                changedVertex[index] += delta;
            }
        }

        public Dictionary<int, Vector3> RedoVertex()
        {
            if (current.Next != null)
            {
                current = current.Next;

                return current.Value.Data;
            }

            return null;
        }

        public Dictionary<int, Vector3> UndoVertex()
        {
            if (current != restorePoints.First)
            {
                var toRet = current;
                current = current.Previous;

                return toRet.Value.Data;
            }

            return null;
        }

        public bool CanUndo()
        {
            return current != restorePoints.First;
        }

        public bool CanRedo()
        {
            return current.Next != null;
        }

        class RestorePoint
        {
            public readonly Dictionary<int, Vector3> Data;

            public RestorePoint(Dictionary<int, Vector3> changedData)
            {
                Data = new Dictionary<int, Vector3>(changedData);
            }
        }
    }
}
