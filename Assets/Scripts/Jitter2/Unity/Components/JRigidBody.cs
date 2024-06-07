using System.Diagnostics;
using Jitter2.Dynamics;
using Jitter2.LinearMath;
using UnityEngine;

namespace Jitter2.Unity
{

    public class JRigidBody : MonoBehaviour
    {
        public bool isStatic;
        public bool isBullet;
        public float mass = 1;
        public float friction = 0.2f;
        public float restitution = 0;
        internal RigidBody body;
        
        public RigidBody CreateBody(World world)
        {
            body = world.CreateRigidBody();
            UpdateProperties(body);
            UpdateShapes(body);
            UpdateTransform(body);
            return body;
        }
        public void Refresh()
        {
            if (Application.isPlaying) return;
            if (body == null)
            {
                body = PhysicsEditor.World.CreateRigidBody();
            }
            UpdateProperties(body);
            UpdateShapes(body);
            UpdateTransform(body);
        }

        internal void UpdateProperties(RigidBody rb)
        {
            rb.IsStatic = isStatic;
            rb.EnableSpeculativeContacts = isBullet;
            rb.Friction = friction;
            rb.Restitution = restitution;
            rb.SetMassInertia(mass);
        }

        internal void UpdateShapes(RigidBody rb)
        {
            rb.RemoveShape(rb.shapes, false);
            foreach (var shape in GetComponents<JColliderBase>())
            {
                rb.AddShape(shape.CreateTransformedShape(), false);
            }
        }

        internal void UpdateTransform(RigidBody rb)
        {
            rb.Position = transform.position.ToVector();
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