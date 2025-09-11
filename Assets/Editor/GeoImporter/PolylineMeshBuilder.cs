using System.Collections.Generic;
using UnityEngine;


namespace GeoImport.EditorUtil
{
    public static class PolylineMeshBuilder
    {
        public static Mesh Build(List<Vector2> pts, float width)
        {
            var mesh = new Mesh();
            if (pts == null || pts.Count < 2) return mesh;


            int n = pts.Count;
            var verts = new List<Vector3>(n * 2);
            var uvs = new List<Vector2>(n * 2);
            var tris = new List<int>((n - 1) * 6);


            Vector2 prevDir = Vector2.zero;
            for (int i = 0; i < n; i++)
            {
                Vector2 p = pts[i];
                Vector2 dir = Vector2.zero;
                if (i == 0) dir = (pts[i + 1] - p).normalized;
                else if (i == n - 1) dir = (p - pts[i - 1]).normalized;
                else
                {
                    Vector2 d1 = (p - pts[i - 1]).normalized;
                    Vector2 d2 = (pts[i + 1] - p).normalized;
                    dir = (d1 + d2).normalized;
                    if (dir.sqrMagnitude < 1e-6f) dir = d2; // handle sharp turns
                }
                Vector2 normal = new Vector2(-dir.y, dir.x);
                float half = width * 0.5f;
                verts.Add(new Vector3(p.x + normal.x * half, p.y + normal.y * half, 0));
                verts.Add(new Vector3(p.x - normal.x * half, p.y - normal.y * half, 0));
                float v = (float)i / (n - 1);
                uvs.Add(new Vector2(0, v));
                uvs.Add(new Vector2(1, v));
            }


            for (int i = 0; i < n - 1; i++)
            {
                int i0 = i * 2;
                int i1 = i * 2 + 1;
                int i2 = i * 2 + 2;
                int i3 = i * 2 + 3;
                tris.Add(i0); tris.Add(i2); tris.Add(i1);
                tris.Add(i2); tris.Add(i3); tris.Add(i1);
            }


            mesh.SetVertices(verts);
            mesh.SetUVs(0, uvs);
            mesh.SetTriangles(tris, 0);
            mesh.RecalculateBounds();
            mesh.RecalculateNormals();
            return mesh;
        }
    }
}