// Version 2.2
// ©2013 Reindeer Games
// All rights reserved
// Redistribution of source code without permission not allowed

using System;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

using UnityEngine;
using Object = UnityEngine.Object;

namespace PrimitivesPro.MeshEditor
{
    /// <summary>
    /// internal mesh data structure (based on half-edge data structure)
    /// </summary>
    public class PPMesh
    {
        public Transform Transform { get; private set; }

        public UnityEngine.Mesh OriginalMesh { get; private set; }

        private Dictionary<long, HalfEdge> edges;
        private HashSet<Face> faces;
        private Face[] vertexFaces;

        private Vector3[] vertices;
        private Vector3[] normals;
        private Vector2[] uvs;

        private HashSet<int> verticesDelta;
        private static string meshName = "[pp_mesh_edit]";
        private UndoSystem undoSystem;
        private const float overlapEpsylon = 0.0001f;

        public class HalfEdge
        {
            public HalfEdge oppositeEdge;
            public HalfEdge nextEdge;
            public Face face;
            public int point;
        }

        public class Face
        {
            public Face(int p0, int p1, int p2)
            {
                this.p0 = p0;
                this.p1 = p1;
                this.p2 = p2;
            }

            public override String ToString()
            {
                return p0.ToString() + " " + p1.ToString() + " " + p2.ToString();
            }

            public readonly int p0;
            public readonly int p1;
            public readonly int p2;
        }

        public struct Vertex
        {
            public Vector3 point;
            public int index;

            public void Update(Vector3 pos)
            {
                point = pos;
            }
        }

        public PPMesh(UnityEngine.MeshFilter meshFilter, Transform transform)
        {
            if (meshFilter.sharedMesh.name.Contains(meshName))
            {
                FromUnityMesh(meshFilter.sharedMesh, transform);
            }
            else
            {
                var mesh = Object.Instantiate(meshFilter.sharedMesh) as Mesh;
                mesh.name += "[pp_mesh_edit]";
                meshFilter.mesh = mesh;
                FromUnityMesh(mesh, transform);
            }
        }

        public void FromUnityMesh(UnityEngine.Mesh mesh, Transform transform)
        {
            if (!mesh.isReadable)
            {
                UnityEngine.Debug.LogWarning("Unable to edit mesh, mesh is not readable!");
            }

            Transform = transform;
            OriginalMesh = mesh;
            Populate(mesh);

            undoSystem = new UndoSystem();
        }

        public void ApplyToMesh()
        {
            OriginalMesh.vertices = vertices;

            var triangles = new int[faces.Count * 3];
            var tri = 0;

            foreach (var face in faces)
            {
                triangles[tri + 0] = face.p0;
                triangles[tri + 1] = face.p1;
                triangles[tri + 2] = face.p2;

                tri += 3;
            }

            OriginalMesh.triangles = triangles;
            OriginalMesh.Optimize();

            OriginalMesh.RecalculateNormals();
            MeshUtils.CalculateTangents(OriginalMesh);
            OriginalMesh.RecalculateBounds();
        }

        public UnityEngine.Mesh ToUnityMesh()
        {
            var mesh = new UnityEngine.Mesh
            {
                vertices = vertices,
                normals = normals,
                uv = uvs
            };

            var triangles = new int[faces.Count];
            var tri = 0;

            foreach (var face in faces)
            {
                triangles[tri + 0] = face.p0;
                triangles[tri + 1] = face.p1;
                triangles[tri + 2] = face.p2;

                tri += 3;
            }

            mesh.triangles = triangles;

            return mesh;
        }

        public void OnUndo()
        {
            var verts = undoSystem.UndoVertex();

            if (verts != null)
            {
                foreach (var v in verts)
                {
                    vertices[v.Key] -= v.Value;
                }
            }

            ApplyToMesh();
        }

        public void OnRedo()
        {
            var verts = undoSystem.RedoVertex();

            if (verts != null)
            {
                foreach (var v in verts)
                {
                    vertices[v.Key] += v.Value;
                }
            }

            ApplyToMesh();
        }

        public bool CanUndo()
        {
            return undoSystem.CanUndo();
        }

        public bool CanRedo()
        {
            return undoSystem.CanRedo();
        }

        public void SaveUndo()
        {
            undoSystem.CreateRestorePoint();
        }

        public void StartUpdateVerticesDelta(HashSet<int> verts, bool includeOverlaps)
        {
            verticesDelta.Clear();

            foreach (int v in verts)
            {
                var p = vertices[v];

                if (includeOverlaps)
                {
                    for (int i = 0; i < vertices.Length; i++)
                    {
                        if ((vertices[i] - p).sqrMagnitude < overlapEpsylon)
                        {
                            verticesDelta.Add(i);
                        }
                    }
                }
                else
                {
                    verticesDelta.Add(v);
                }
            }
        }

        public void UpdateVerticesDelta(Vector3 delta)
        {
            foreach (int v in verticesDelta)
            {
                vertices[v] += delta;
                undoSystem.OnVertexChanged(v, delta);
            }
        }

        public void StartUpdateFacesDelta(HashSet<Face> changeFaces, bool includeOverlaps)
        {
            verticesDelta.Clear();

            foreach (var face in changeFaces)
            {
                verticesDelta.Add(face.p0);
                verticesDelta.Add(face.p1);
                verticesDelta.Add(face.p2);
            }

            if (includeOverlaps)
            {
                var overlaps = new HashSet<int>();
                foreach (var j in verticesDelta)
                {
                    for (int i = 0; i < vertices.Length; i++)
                    {
                        Vector3 v = vertices[i];
                        if ((v - vertices[j]).sqrMagnitude < overlapEpsylon)
                        {
                            overlaps.Add(i);
                        }
                    }
                }

                verticesDelta.UnionWith(overlaps);
            }
        }

        public void UpdateFacesDelta(Vector3 delta)
        {
            foreach (int faceVert in verticesDelta)
            {
                vertices[faceVert] += delta;
                undoSystem.OnVertexChanged(faceVert, delta);
            }
        }

        public void StartUpdateEdgesDelta(HashSet<HalfEdge> changeEdges, bool includeOverlaps)
        {
            verticesDelta.Clear();

            foreach (var edge in changeEdges)
            {
                verticesDelta.Add(edge.point);
                verticesDelta.Add(edge.nextEdge.point);
            }

            if (includeOverlaps)
            {
                var overlaps = new HashSet<int>();
                foreach (var j in verticesDelta)
                {
                    for (int i = 0; i < vertices.Length; i++)
                    {
                        Vector3 v = vertices[i];
                        if ((v - vertices[j]).sqrMagnitude < overlapEpsylon)
                        {
                            overlaps.Add(i);
                        }
                    }
                }

                verticesDelta.UnionWith(overlaps);
            }
        }

        public void UpdateEdgesDelta(Vector3 delta)
        {
            foreach (int faceVert in verticesDelta)
            {
                vertices[faceVert] += delta;
                undoSystem.OnVertexChanged(faceVert, delta);
            }
        }

        private void Populate(UnityEngine.Mesh mesh)
        {
            edges = new Dictionary<long, HalfEdge>(mesh.triangles.Length);
            faces = new HashSet<Face>();
            vertexFaces = new Face[mesh.vertexCount];
            vertices = new Vector3[mesh.vertexCount];
            normals = new Vector3[mesh.vertexCount];
            uvs = new Vector2[mesh.vertexCount];

            verticesDelta = new HashSet<int>();

            Array.Copy(mesh.vertices, vertices, mesh.vertexCount);
            Array.Copy(mesh.normals, normals, mesh.vertexCount);
            Array.Copy(mesh.uv, uvs, mesh.vertexCount);

            var tris = mesh.triangles;

            for (int i = 0; i < tris.Length; i+=3)
            {
                var edge0 = HashEdge(tris[i],   tris[i + 1]);
                var edge1 = HashEdge(tris[i+1], tris[i + 2]);
                var edge2 = HashEdge(tris[i+2], tris[i]);

                var face = new Face(tris[i], tris[i+1], tris[i+2]);

                var he0 = new HalfEdge { face = face, point = tris[i]   };
                var he1 = new HalfEdge { face = face, point = tris[i+1] };
                var he2 = new HalfEdge { face = face, point = tris[i+2] };

                vertexFaces[tris[i]] = face;
                vertexFaces[tris[i+1]] = face;
                vertexFaces[tris[i+2]] = face;

                try
                {
                    he0.nextEdge = he1;
                    he1.nextEdge = he2;
                    he2.nextEdge = he0;

                    edges.Add(edge0, he0);
                    edges.Add(edge1, he1);
                    edges.Add(edge2, he2);

                    faces.Add(face);
                }
                catch{}
//                catch (Exception ex)
//                {
//                    MeshUtils.Log("Duplicated face detected.");
//                }
            }

            foreach (var halfEdge in edges.Values)
            {
                var p0 = halfEdge.point;
                var p1 = halfEdge.nextEdge.point;

                if (halfEdge.oppositeEdge == null)
                {
                    var oppositeEdgeHash = HashEdge(p1, p0);
                    HalfEdge oppositeHalfEdge;

                    if (edges.TryGetValue(oppositeEdgeHash, out oppositeHalfEdge))
                    {
                        halfEdge.oppositeEdge = oppositeHalfEdge;
                        oppositeHalfEdge.oppositeEdge = halfEdge;
                    }
                }
            }
        }

        private long HashEdge(int v0, int v1)
        {
            return ((uint) v0) | (((long) v1) << 32);
        }

        public float GetNearestPoint(Ray ray, out int index, bool excludeBackSidePolygons)
        {
            // transform ray to local space
            var localOrigin = Transform.InverseTransformPoint(ray.origin);
            var localDir = Transform.InverseTransformDirection(ray.direction);

            var localRay = new Ray(localOrigin, localDir);

            var closestDist = float.MaxValue;
            var idx = -1;

            for (int i = 0; i < vertices.Length; i++)
            {
                var v = vertices[i];
                var dist = MeshUtils.DistanceToLine2(localRay, v);

                if (dist < closestDist)
                {
                    if (excludeBackSidePolygons)
                    {
                        var face = vertexFaces[i];

                        if (face != null)
                        {
                            if (!Utils.Plane.GetSide(vertices[face.p0], vertices[face.p1], vertices[face.p2], localRay.origin))
                            {
                                continue;
                            }
                        }
                    }

                    closestDist = dist;
                    idx = i;
                }
            }

            //MeshUtils.Assert(idx >= 0, "Idx!");

            index = idx;
            return closestDist;
        }

        public Face GetIntersectingPolygon(Ray ray, bool excludeBackSidePolygons)
        {
            var localOrigin = Transform.InverseTransformPoint(ray.origin);
            var localDir = Transform.InverseTransformDirection(ray.direction);
            var localRay = new Ray(localOrigin, localDir);

            Face closestFace = null;
            var closestDistance = float.MaxValue;

            foreach (var face in faces)
            {
                var p0 = vertices[face.p0];
                var p1 = vertices[face.p1];
                var p2 = vertices[face.p2];

                float ot;
                if (MeshUtils.RayTriangleIntersection(p0, p1, p2, localRay.origin, localRay.direction, out ot))
                {
                    if (ot < closestDistance)
                    {
                        if (excludeBackSidePolygons)
                        {
                            if (!Utils.Plane.GetSide(p0, p1, p2, localRay.origin))
                            {
                                continue;
                            }
                        }

                        closestDistance = ot;
                        closestFace = face;
                    }
                }
            }

            return closestFace;
        }

        public float GetNearestEdge(Ray ray, out HalfEdge minEdge, bool excludeBackSidePolygons)
        {
            var localOrigin = Transform.InverseTransformPoint(ray.origin);
            var localDir = Transform.InverseTransformDirection(ray.direction);
            var localRay = new Ray(localOrigin, localDir);

            var minDistance = float.MaxValue;
            minEdge = null;

            foreach (var edge in edges.Values)
            {
                var p0 = vertices[edge.point];
                var p1 = vertices[edge.nextEdge.point];

                var squaredDist = MeshUtils.SegmentSegmentDistance2(p0, p1, localRay.origin, localRay.origin + localRay.direction*10000.0f);

                if (squaredDist < minDistance)
                {
                    if (excludeBackSidePolygons)
                    {
                        var p2 = vertices[edge.nextEdge.nextEdge.point];

                        if (!Utils.Plane.GetSide(p0, p1, p2, localRay.origin))
                        {
                            continue;
                        }
                    }

                    minDistance = squaredDist;
                    minEdge = edge;
                }
            }

            return minDistance;
        }

        public Vector3 GetVertexByIndexWorld(int index)
        {
            return Transform.TransformPoint(vertices[index]);
        }

        public Vector3[] GetFaceRectangle(Face face)
        {
            var p0 = Transform.TransformPoint(vertices[face.p0]);
            var p1 = Transform.TransformPoint(vertices[face.p1]);
            var p2 = Transform.TransformPoint(vertices[face.p2]);

            return new [] {p0, p1, p2, p2};
        }

        public Vector3[] GetEdgeLine(HalfEdge edge)
        {
            var p0 = Transform.TransformPoint(vertices[edge.point]);
            var p1 = Transform.TransformPoint(vertices[edge.nextEdge.point]);

            return new[] {p0, p1};
        }

        public Vector3 GetAveragePos(int[] list)
        {
            var avg = Vector3.zero;

            foreach (var i in list)
            {
                avg += Transform.TransformPoint(vertices[i]);
            }

            return avg/list.Length;
        }

        public Quaternion GetLocalRotation(HashSet<Face> faceSet)
        {
            var avgNorm = Vector3.zero;

            foreach (var face in faceSet)
            {
                avgNorm += GetFaceNormalWorld(face);
            }

            return Quaternion.LookRotation(avgNorm.normalized);
        }

        public Quaternion GetLocalRotation(HashSet<int> vertexSet)
        {
            var avgNorm = Vector3.zero;

            foreach (var i in vertexSet)
            {
                avgNorm += Transform.TransformDirection(normals[i]);
            }

            return Quaternion.LookRotation(avgNorm.normalized);
        }

        public Quaternion GetLocalRotation(HashSet<HalfEdge> edgeSet)
        {
            var avgNorm = Vector3.zero;

            foreach (var edge in edgeSet)
            {
                avgNorm += GetFaceNormalWorld(edge.face);

                if (edge.oppositeEdge != null)
                {
                    avgNorm += GetFaceNormalWorld(edge.oppositeEdge.face);
                }
            }

            return Quaternion.LookRotation(avgNorm.normalized);
        }

        public Vector3 GetFaceNormalWorld(Face face)
        {
            return MeshUtils.ComputePolygonNormal(Transform.TransformPoint(vertices[face.p0]),
                                                  Transform.TransformPoint(vertices[face.p1]),
                                                  Transform.TransformPoint(vertices[face.p2]));
        }

        public HashSet<int> GetPointsInRect2D(Rect rect)
        {
            var hashSet = new HashSet<int>();

#if UNITY_EDITOR
            for (int i = 0; i < vertices.Length; i++)
            {
                var world = Transform.TransformPoint(vertices[i]);
                var gui = HandleUtility.WorldToGUIPoint(world);

                if (rect.Contains(gui))
                {
                    hashSet.Add(i);
                }
            }
#endif

            return hashSet;
        }

        public HashSet<Face> GetPolygonsInRect2D(Rect rect)
        {
            var hashSet = new HashSet<Face>();

            var verts = GetPointsInRect2D(rect);

            foreach (var face in faces)
            {
                if (verts.Contains(face.p0) && verts.Contains(face.p1) && verts.Contains(face.p2))
                {
                    hashSet.Add(face);
                }
            }

            return hashSet;
        }

        public HashSet<HalfEdge> GetEdgesInRect2D(Rect rect)
        {
            var hashSet = new HashSet<HalfEdge>();

            var verts = GetPointsInRect2D(rect);

            foreach (var edge in edges.Values)
            {
                if (verts.Contains(edge.point) && verts.Contains(edge.nextEdge.point))
                {
                    hashSet.Add(edge);
                }
            }

            return hashSet;
        }

        public bool FindClosestPoint(Vector3 p, HashSet<int> exclude, float tolerance2, ref Vector3 ret)
		{
			int closest = -1;
			float dist = float.MaxValue;

            var localP = Transform.InverseTransformPoint(p);

			for (int i = 0; i < vertices.Length; i++)
			{
                if (exclude.Contains(i))
                    continue;

                var v = vertices[i];

                var d = (v - localP).sqrMagnitude;
                if (d < tolerance2 && d < dist)
				{
					closest = i;
					dist = d;
				}
			}

			if (closest != -1)
			{
				ret = Transform.TransformPoint(vertices[closest]);
				return true;
			}

			return false;
		}

        public void AddOverlappingVertices(HashSet<int> verts, int vertex)
        {
            if (vertex != -1)
            {
                var v = vertices[vertex];

                for (int i = 0; i < vertices.Length; i++)
                {
                    if (i == vertex)
                        continue;

                    if ((vertices[i] - v).sqrMagnitude < overlapEpsylon)
                    {
                        verts.Add(i);
                    }
                }
            }
        }
    }
}
