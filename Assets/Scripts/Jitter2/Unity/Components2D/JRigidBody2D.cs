using System.Diagnostics;
using Jitter2.Dynamics;
using Jitter2.LinearMath;
using Jitter2.Unity;
using UnityEngine;

namespace Jitter2.Unity2D
{

    public class JRigidBody2D : MonoBehaviour
    {
        public bool isStatic;
        public bool isBullet;
        public bool useGravity;
        public float mass = 1;
        public float friction = 0.2f;
        public float restitution = 0;
        public RigidBody body;
        public Vector2Int translationConstraints = Vector2Int.one;
        public bool rotationConstraints = true;

        public RigidBody CreateBody(World world)
        {
            body = world.CreateRigidBody();
            UpdateProperties(body);
            UpdateShapes(body);
            UpdateTransform(body);
            body.Tag = new RigidBodyUserData()
            {
                Layer = gameObject.layer,
            };
            return body;
        }

        public void Refresh()
        {
#if UNITY_EDITOR
            if (Application.isPlaying) return;
            if (body == null)
            {
                body = PhysicsEditor.World.CreateRigidBody();
            }
            UpdateProperties(body);
            UpdateShapes(body);
            UpdateTransform(body);
#endif
        }

        internal void UpdateProperties(RigidBody rb)
        {
            rb.IsStatic = isStatic;
            rb.EnableSpeculativeContacts = isBullet;
            rb.Friction = friction;
            rb.AffectedByGravity = useGravity;
            rb.Restitution = restitution;
            rb.Data.hasTransformConstraints = true;
            rb.Data.RotationConstraint = new JVector(0, 0, rotationConstraints ? 1 : 0);
            rb.Data.TranslationConstraint = new JVector(translationConstraints.x, translationConstraints.y, 0);
            rb.SetMassInertia(mass);
        }

        internal void UpdateShapes(RigidBody rb)
        {
            rb.RemoveShape(rb.shapes, false);
            foreach (var shape in GetComponents<JCollider2DBase>())
            {
                rb.AddShape(shape.CreateTransformedShape(), false);
            }
        }

        internal void UpdateTransform(RigidBody rb)
        {
            var pos = transform.position;
            rb.Position = new JVector(pos.x, pos.y, 0);
            rb.Orientation = transform.rotation.ToQuaternion();
        }

        // [Conditional("UNITY_EDITOR")]
        // internal void OnDrawGizmos()
        // {
        //     UpdateTransform(body);
        //     body.DebugDraw(JitterGizmosDrawer.Instance);
        // }
        [Conditional("UNITY_EDITOR")]
        void OnValidate()
        {
            Refresh();
        }
    }
}