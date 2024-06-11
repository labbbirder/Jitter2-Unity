using Jitter2.Collision.Shapes;
using UnityEngine;

namespace Jitter2.Unity
{
    [AddComponentMenu("LSPhysics/CapsuleCollider")]
    [RequireComponent(typeof(JRigidBody))]
    public class JCapsuleCollider : JColliderBase
    {
        public float radius = 1;
        public float height = 1;

        public override Shape CreateShape()
            => new CapsuleShape(radius, height);

    }
}

