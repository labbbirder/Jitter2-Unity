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

        public static List<JTriangle> ToTriangleList(this Mesh mesh)
        {
            var list = new List<JTriangle>();
            var vertices = mesh.vertices;
            var triangles = mesh.triangles;
            for (int i = 0; i < triangles.Length / 3; i++)
            {
                list.Add(new JTriangle(
                    vertices[triangles[i * 3 + 0]].ToVector(),
                    vertices[triangles[i * 3 + 1]].ToVector(),
                    vertices[triangles[i * 3 + 2]].ToVector()
                ));
            }
            return list;
        }
    }
}