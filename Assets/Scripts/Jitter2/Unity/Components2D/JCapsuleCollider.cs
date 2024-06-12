using Jitter2.Collision.Shapes;
using UnityEngine;

namespace Jitter2.Unity2D
{
    [AddComponentMenu("JPhysics2D/CapsuleCollider")]
    [RequireComponent(typeof(JRigidBody2D))]
    public class JCapsuleCollider : JCollider2DBase
    {
        public float radius = 1;
        public float height = 1;

        public override Shape CreateShape()
            => new CapsuleShape(radius, height);

    }
}

