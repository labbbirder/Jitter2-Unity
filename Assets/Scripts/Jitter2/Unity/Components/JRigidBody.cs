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
        public bool useGravity = true;
        public float mass = 1;
        public float friction = 0.2f;
        public float restitution = 0;
        internal RigidBody body;
        public Vector3Int translationConstraints = Vector3Int.one;
        public Vector3Int rotationConstraints = Vector3Int.one;

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
            rb.Data.hasTransformConstraints = rotationConstraints != Vector3Int.one || translationConstraints != Vector3Int.one;
            rb.Data.RotationConstraint = new JVector(rotationConstraints.x, rotationConstraints.y, rotationConstraints.z);
            rb.Data.TranslationConstraint = new JVector(translationConstraints.x, translationConstraints.y, translationConstraints.z);
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