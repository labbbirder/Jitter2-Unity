using Jitter2.Collision.Shapes;
using UnityEngine;

namespace Jitter2.Unity
{
    [AddComponentMenu("LSPhysics/CylinderCollider")]
    [RequireComponent(typeof(JRigidBody))]
    public class JCylinderCollider : JColliderBase
    {
        public float radius = 1;
        public float height = 1;

        public override Shape CreateShape()
            => new CylinderShape(height, radius);

    }
}

