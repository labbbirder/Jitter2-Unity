using System.Collections.Generic;
using Jitter2.Collision.Shapes;
using UnityEngine;

namespace Jitter2.Unity2D
{
    [AddComponentMenu("JPhysics2D/CapsuleCollider")]
    [RequireComponent(typeof(JRigidBody2D))]
    public class JCapsuleCollider2D : JCollider2DBase
    {
        public float radius = 1;
        public float height = 1;

        public override IEnumerable<Shape> CreateShape()
        {
            yield return new CapsuleShape(radius, height);
        }

    }
}

