using System.Collections.Generic;
using Jitter2.Collision.Shapes;
using UnityEngine;

namespace Jitter2.Unity2D
{
    [AddComponentMenu("JPhysics2D/CircleCollider")]
    [RequireComponent(typeof(JRigidBody2D))]
    public class JCircleCollider : JCollider2DBase
    {
        public float radius = 1;

        public override IEnumerable<Shape> CreateShape()
        {
            yield return new SphereShape(radius);
        }
    }
}

