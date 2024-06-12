using Jitter2.Collision.Shapes;
using Jitter2.LinearMath;
using Jitter2.Unity;
using UnityEngine;

namespace Jitter2.Unity2D
{
    [AddComponentMenu("JPhysics2D/BoxCollider")]
    [RequireComponent(typeof(JRigidBody2D))]
    public class JBoxCollider2D : JCollider2DBase
    {
        public Vector2 size = Vector3.one;
        public override Shape CreateShape()
            => new BoxShape(new JVector(size.x, size.y, 1));

    }
}

