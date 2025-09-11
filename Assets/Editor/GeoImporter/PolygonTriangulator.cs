using System.Collections.Generic;
using UnityEngine;

namespace GeoImport.EditorUtil
{
    /// <summary>
    /// Provides methods for triangulating simple polygons using the ear clipping algorithm.
    /// </summary>
    public static class PolygonTriangulator
    {
        /// <summary>
        /// Triangulates a simple polygon defined by a list of vertices.
        /// The resulting triangle indices are written to <paramref name="indicesOut"/>.
        /// </summary>
        /// <param name="poly">The polygon vertices in 2D space.</param>
        /// <param name="indicesOut">The output list to receive triangle vertex indices.</param>
        public static void Triangulate(IList<Vector2> poly, List<int> indicesOut)
        {
            indicesOut.Clear();
            int n = poly.Count;
            if (n < 3) return;

            // Build a list of vertex indices (assume polygon is in CCW or CW; detect and ensure CCW)
            var V = new List<int>(n);
            float area = SignedArea(poly);
            bool isCCW = area > 0f;
            if (isCCW) { for (int v = 0; v < n; v++) V.Add(v); }
            else { for (int v = 0; v < n; v++) V.Add(n - 1 - v); }

            int nv = n;
            int count = 2 * nv; // fail-safe
            int vtx = 0;
            while (nv > 2 && count-- > 0)
            {
                int i0 = V[(vtx + 0) % nv];
                int i1 = V[(vtx + 1) % nv];
                int i2 = V[(vtx + 2) % nv];

                if (IsEar(i0, i1, i2, poly, V))
                {
                    indicesOut.Add(i0);
                    indicesOut.Add(i1);
                    indicesOut.Add(i2);
                    V.RemoveAt((vtx + 1) % nv); // clip ear (middle vertex)
                    nv--;
                    vtx = 0;
                    continue;
                }
                vtx++;
            }
        }

        /// <summary>
        /// Computes the signed area of a polygon.
        /// Positive value indicates counter-clockwise winding.
        /// </summary>
        /// <param name="p">The polygon vertices.</param>
        /// <returns>The signed area of the polygon.</returns>
        static float SignedArea(IList<Vector2> p)
        {
            double a = 0;
            for (int i = 0, j = p.Count - 1; i < p.Count; j = i++)
            {
                a += (double)(p[j].x * p[i].y - p[i].x * p[j].y);
            }
            return (float)(a * 0.5);
        }

        /// <summary>
        /// Determines if the triangle formed by the given indices is an ear.
        /// </summary>
        /// <param name="i0">Index of the first vertex.</param>
        /// <param name="i1">Index of the second vertex.</param>
        /// <param name="i2">Index of the third vertex.</param>
        /// <param name="poly">The polygon vertices.</param>
        /// <param name="V">The current list of vertex indices.</param>
        /// <returns>True if the triangle is an ear; otherwise, false.</returns>
        static bool IsEar(int i0, int i1, int i2, IList<Vector2> poly, List<int> V)
        {
            Vector2 a = poly[i0];
            Vector2 b = poly[i1];
            Vector2 c = poly[i2];
            if (Area2(a, b, c) <= 0f) return false; // not CCW

            // check if any other point lies inside triangle abc
            for (int k = 0; k < V.Count; k++)
            {
                int vi = V[k];
                if (vi == i0 || vi == i1 || vi == i2) continue;
                if (PointInTriangle(poly[vi], a, b, c)) return false;
            }
            return true;
        }

        /// <summary>
        /// Computes twice the signed area of the triangle defined by three points.
        /// </summary>
        /// <param name="a">First vertex.</param>
        /// <param name="b">Second vertex.</param>
        /// <param name="c">Third vertex.</param>
        /// <returns>Twice the signed area of the triangle.</returns>
        static float Area2(Vector2 a, Vector2 b, Vector2 c) => (b.x - a.x) * (c.y - a.y) - (b.y - a.y) * (c.x - a.x);

        /// <summary>
        /// Determines if a point lies inside the triangle defined by three vertices.
        /// </summary>
        /// <param name="p">The point to test.</param>
        /// <param name="a">First triangle vertex.</param>
        /// <param name="b">Second triangle vertex.</param>
        /// <param name="c">Third triangle vertex.</param>
        /// <returns>True if the point is inside the triangle; otherwise, false.</returns>
        static bool PointInTriangle(Vector2 p, Vector2 a, Vector2 b, Vector2 c)
        {
            float a1 = Area2(p, a, b);
            float a2 = Area2(p, b, c);
            float a3 = Area2(p, c, a);
            bool hasNeg = (a1 < 0) || (a2 < 0) || (a3 < 0);
            bool hasPos = (a1 > 0) || (a2 > 0) || (a3 > 0);
            return !(hasNeg && hasPos);
        }
    }
}