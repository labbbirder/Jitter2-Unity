using System.Diagnostics;
using Jitter2.Collision.Shapes;
using Jitter2.LinearMath;
using UnityEngine;

namespace Jitter2.Unity2D
{
    [RequireComponent(typeof(JRigidBody2D))]
    public abstract class JCollider2DBase : MonoBehaviour
    {
        public Vector2 center;
        public float rotation;

        public JRigidBody2D RigidBody => GetComponent<JRigidBody2D>();

        public abstract Shape CreateShape();

        public Shape CreateTransformedShape()
        {
            var shape = CreateShape();
            if (center == Vector2.zero && rotation == 0)
            {
                return shape;
            }

            var translation = new JVector(center.x, center.y, 0);
            var orientation = Matrix4x4.Rotate(Quaternion.AngleAxis(rotation, new(0, 0, 1))).ToJMatrix();
            return new TransformedShape(shape, translation, orientation);
        }

        [Conditional("UNITY_EDITOR")]
        void OnValidate()
        {
            RigidBody.Refresh();
        }

    }
}