using Jitter2.Collision.Shapes;
using UnityEngine;

namespace Jitter2.Unity
{
    [AddComponentMenu("JPhysics/ConeCollider")]
    [RequireComponent(typeof(JRigidBody))]
    public class JConeCollider : JColliderBase
    {
        public float radius = 1;
        public float height = 1;
        
        public override Shape CreateShape()
            => new ConeShape(radius, height);
    }
}

