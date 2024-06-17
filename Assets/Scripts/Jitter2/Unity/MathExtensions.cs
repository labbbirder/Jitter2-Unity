using System;
using System.Collections.Generic;
using UnityEngine;

namespace Jitter2.LinearMath
{

    public static class MathExtensions
    {
        public static JVector ToVector(this Vector3 vec)
        {
            return new(vec.x, vec.y, vec.z);
        }

        public static JVector ToVector(this Vector2 vec)
        {
            return new(vec.x, vec.y, 0);
        }

        public static Vector3 ToVector(this JVector vec)
        {
            return new(vec.X, vec.Y, vec.Z);
        }

        public static JQuaternion ToQuaternion(this Quaternion vec)
        {
            return new(vec.x, vec.y, vec.z, vec.w);
        }

        public static Quaternion ToQuaternion(this JQuaternion vec)
        {
            return new(vec.X, vec.Y, vec.Z, vec.W);
        }

        public static JMatrix ToJMatrix(this Matrix4x4 m)
        {
            return new(
                m.m00, m.m01, m.m02,
                m.m10, m.m11, m.m12,
                m.m20, m.m21, m.m22
            );
        }

        // public static List<JTriangle> ToTriangleList(this Mesh mesh)
        // {
        //     var list = new List<JTriangle>();
        //     var vertices = mesh.vertices;
        //     var triangles = mesh.triangles;
        //     for (int i = 0; i < triangles.Length / 3; i++)
        //     {
        //         list.Add(new JTriangle(
        //             vertices[triangles[i * 3 + 0]].ToVector(),
        //             vertices[triangles[i * 3 + 1]].ToVector(),
        //             vertices[triangles[i * 3 + 2]].ToVector()
        //         ));
        //         if (vertices[triangles[i * 3 + 0]].x > 0)
        //         {
        //             Debug.Log(vertices[triangles[i * 3 + 0]].x);
        //         }
        //     }
        //     return list;
        // }

        public static IEnumerable<List<JTriangle>> ToTriangleList(this Mesh mesh)
        {
            var vertices = mesh.vertices;
            var triangles = mesh.triangles;
            for (int iSub = 0; iSub < mesh.subMeshCount; iSub++)
            {
                var list = new List<JTriangle>();
                var subMesh = mesh.GetSubMesh(iSub);
                for (int i = subMesh.indexStart; i < subMesh.indexStart + subMesh.indexCount;)
                {
                    list.Add(new JTriangle(
                        vertices[triangles[i++]].ToVector(),
                        vertices[triangles[i++]].ToVector(),
                        vertices[triangles[i++]].ToVector()
                    ));
                }
                yield return list;
            }
        }

        public static IEnumerable<List<JTriangle>> ToTriangleList(this PolygonCollider2D poly2d)
        {
            // split pathes to hull shape
            var group = new PhysicsShapeGroup2D();
            var hullCount = poly2d.GetShapes(group);

            var vertices2D = new List<Vector2>();
            var shapes = new List<PhysicsShape2D>();
            var matInv = poly2d.transform.localToWorldMatrix.inverse;
            group.GetShapeData(shapes, vertices2D);

            // transform hull to triangles
            for (int ihull = 0; ihull < hullCount; ihull++)
            {
                var shape = group.GetShape(ihull);
                if (shape.shapeType != PhysicsShapeType2D.Polygon)
                {
                    Debug.LogWarning($"unsupported shape type {shape.shapeType}");
                }
                var triangles = new List<JTriangle>();

                for (int i = 0; i < shape.vertexCount - 2; i++)
                {
                    triangles.Add(new JTriangle(
                        matInv.MultiplyPoint(vertices2D[shape.vertexStartIndex]).ToVector(),
                        matInv.MultiplyPoint(vertices2D[shape.vertexStartIndex + i + 1]).ToVector(),
                        matInv.MultiplyPoint(vertices2D[shape.vertexStartIndex + i + 2]).ToVector()
                    ));
                }
                yield return triangles;
            }
        }

        public static IEnumerable<List<JTriangle>> ToTriangleList(this Sprite sprite)
        {
            var poly2d = EditorTool.GetSingleton<PolygonCollider2D>();

            // feed pathes to PolygonCollider2D 
            var pathCount = sprite.GetPhysicsShapeCount();
            poly2d.pathCount = pathCount;
            for (int i = 0; i < pathCount; i++)
            {
                var vertices = new List<Vector2>();
                sprite.GetPhysicsShape(i, vertices);
                poly2d.SetPath(i, vertices);
            }

            return poly2d.ToTriangleList();
        }

    }
}