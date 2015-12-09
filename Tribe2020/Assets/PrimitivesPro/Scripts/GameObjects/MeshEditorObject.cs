// Version 2.2
// ©2013 Reindeer Games
// All rights reserved
// Redistribution of source code without permission not allowed

using PrimitivesPro.Utils;
using UnityEngine;

namespace PrimitivesPro.MeshEditor
{
    [ExecuteInEditMode]
    [DisallowMultipleComponent]
    public class MeshEditorObject : MonoBehaviour
    {
        public PPMesh ppMesh;

        public void UpdateMesh()
        {
            ppMesh.ApplyToMesh();
        }

        public void Init()
        {
            if (ppMesh == null)
            {
                var meshFilter = GetComponent<MeshFilter>();

                if (meshFilter && meshFilter.sharedMesh)
                {
                    ppMesh = new PPMesh(meshFilter, transform);
                }
            }
        }
    }
}
