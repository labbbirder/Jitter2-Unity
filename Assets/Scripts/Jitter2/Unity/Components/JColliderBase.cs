using System.Diagnostics;
using Jitter2.Collision.Shapes;
using Jitter2.LinearMath;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Jitter2.Unity
{
    [RequireComponent(typeof(JRigidBody))]
    public abstract class JColliderBase : MonoBehaviour
    {
        public Vector3 center;
        public Vector3 rotation;

        public JRigidBody RigidBody => GetComponent<JRigidBody>();

        public abstract Shape CreateShape();

        public Shape CreateTransformedShape()
        {
            var shape = CreateShape();
            if (center == Vector3.zero && rotation == Vector3.zero)
            {
                return shape;
            }

            var translation = center.ToVector();
            var orientation = Matrix4x4.Rotate(Quaternion.Euler(rotation)).ToJMatrix();
            return new TransformedShape(shape, translation, orientation);
        }

        [Conditional("UNITY_EDITOR")]
        void OnValidate()
        {
            RigidBody.Refresh();
        }

    }
}