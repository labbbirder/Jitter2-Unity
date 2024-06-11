using Jitter2.Collision.Shapes;
using Jitter2.LinearMath;
using UnityEngine;

namespace Jitter2.Unity
{
    [AddComponentMenu("LSPhysics/MeshCollider")]
    [RequireComponent(typeof(JRigidBody))]
    public class JMeshCollider : JColliderBase
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

