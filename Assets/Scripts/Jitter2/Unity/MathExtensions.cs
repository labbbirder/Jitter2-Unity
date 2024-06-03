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
    }
}