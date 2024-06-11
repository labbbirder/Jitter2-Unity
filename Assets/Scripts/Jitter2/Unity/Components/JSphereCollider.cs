using Jitter2.Collision.Shapes;
using UnityEngine;

namespace Jitter2.Unity
{
    [AddComponentMenu("LSPhysics/SphereCollider")]
    [RequireComponent(typeof(JRigidBody))]
    public class JSphereCollider : JColliderBase
    {
        public float radius = 1;

        public override Shape CreateShape()
            => new SphereShape(radius);

    }
}

