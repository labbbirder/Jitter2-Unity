using Jitter2.Collision.Shapes;
using Jitter2.LinearMath;
using UnityEngine;

namespace Jitter2.Unity
{
    [AddComponentMenu("LSPhysics/BoxCollider")]
    [RequireComponent(typeof(JRigidBody))]
    public class JBoxCollider : JColliderBase
    {
        public Vector3 size = Vector3.one;
        public override Shape CreateShape()
            => new BoxShape(size.ToVector());

    }
}

