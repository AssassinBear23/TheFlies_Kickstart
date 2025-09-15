using System.Collections.Generic;
using UnityEngine;


namespace GeoImport.EditorUtil
{
    /// <summary>
    /// Provides utility methods for constructing 2D polyline meshes in the Unity Editor.
    /// </summary>
    public static class PolylineMeshBuilder
    {
        /// <summary>
        /// Constructs a 2D mesh representing a strip or ribbon based on the provided points and width.
        /// </summary>
        /// <remarks>
        /// The method generates a mesh with vertices, UVs, and triangles based on the input
        /// points and width. The mesh is constructed such that the strip follows the path defined by <paramref name="points"/>, 
        /// with the width evenly distributed on both sides of the centerline.
        /// <para>
        /// The resulting mesh includes recalculated bounds and normals, making it ready for rendering.
        /// </para>
        /// </remarks>
        /// <param name="points">A list of 2D points defining the centerline of the strip. Must contain at least two points.</param>
        /// <param name="stripWidth">The total width of the strip. Must be a positive value.</param>
        /// <returns>
        /// A <see cref="Mesh"/> object representing the generated strip. If <paramref name="points"/> is null or contains
        /// fewer than two points, an empty mesh is returned.
        /// </returns>
        public static Mesh Build(List<Vector2> points, float stripWidth)
        {
            var mesh = new Mesh();
            if (points == null || points.Count < 2) return mesh;

            int pointCount = points.Count;
            var vertices = new List<Vector3>(pointCount * 2);
            var uv0 = new List<Vector2>(pointCount * 2);
            var triangleIndices = new List<int>((pointCount - 1) * 6);

            for (int i = 0; i < pointCount; i++)
            {
                Vector2 currentPoint = points[i];
                Vector2 direction = Vector2.zero;

                if (i == 0)
                {
                    direction = (points[i + 1] - currentPoint).normalized;
                }
                else if (i == pointCount - 1)
                {
                    direction = (currentPoint - points[i - 1]).normalized;
                }
                else
                {
                    Vector2 directionToPrev = (currentPoint - points[i - 1]).normalized;
                    Vector2 directionToNext = (points[i + 1] - currentPoint).normalized;
                    direction = (directionToPrev + directionToNext).normalized;
                    if (direction.sqrMagnitude < 1e-6f) direction = directionToNext; // handle sharp turns
                }

                Vector2 perpendicular = new Vector2(-direction.y, direction.x);
                float halfWidth = stripWidth * 0.5f;

                vertices.Add(new Vector3(currentPoint.x + perpendicular.x * halfWidth, currentPoint.y + perpendicular.y * halfWidth, 0));
                vertices.Add(new Vector3(currentPoint.x - perpendicular.x * halfWidth, currentPoint.y - perpendicular.y * halfWidth, 0));

                float vCoord = (float)i / (pointCount - 1);
                uv0.Add(new Vector2(0, vCoord));
                uv0.Add(new Vector2(1, vCoord));
            }

            for (int i = 0; i < pointCount - 1; i++)
            {
                int index0 = i * 2;
                int index1 = i * 2 + 1;
                int index2 = i * 2 + 2;
                int index3 = i * 2 + 3;

                triangleIndices.Add(index0); triangleIndices.Add(index2); triangleIndices.Add(index1);
                triangleIndices.Add(index2); triangleIndices.Add(index3); triangleIndices.Add(index1);
            }

            mesh.SetVertices(vertices);
            mesh.SetUVs(0, uv0);
            mesh.SetTriangles(triangleIndices, 0);
            mesh.RecalculateBounds();
            mesh.RecalculateNormals();
            return mesh;
        }
    }
}