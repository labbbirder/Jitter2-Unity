using Jitter2.Collision.Shapes;
using Jitter2.LinearMath;
using UnityEngine;

namespace Jitter2.Unity2D
{
    [AddComponentMenu("JPhysics2D/MeshCollider")]
    [RequireComponent(typeof(JRigidBody2D))]
    public class JMeshCollider : JCollider2DBase
    {
        public Mesh mesh;

        public override Shape CreateShape()
        {
            if (mesh == null) return new ConvexHullShape(new());
            return new ConvexHullShape(mesh.ToTriangleList());
        }

        void Reset()
        {
            mesh = GetComponent<MeshFilter>().sharedMesh;
        }
    }
}

