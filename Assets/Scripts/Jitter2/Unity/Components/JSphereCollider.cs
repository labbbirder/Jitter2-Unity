using Jitter2.Collision.Shapes;
using Jitter2.LinearMath;
using UnityEditor;
using UnityEngine;

namespace Jitter2.Unity
{
    [RequireComponent(typeof(JRigidBody))]
    public class JSphereCollider : JColliderBase
    {
        public float radius = 1;

        public override Shape CreateShape()
            => new SphereShape(radius);

    }
}

