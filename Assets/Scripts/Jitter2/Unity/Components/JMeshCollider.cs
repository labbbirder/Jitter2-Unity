using System.Linq;
using Jitter2.Collision.Shapes;
using Jitter2.LinearMath;
using UnityEngine;

namespace Jitter2.Unity
{
    [AddComponentMenu("JPhysics/MeshCollider")]
    [RequireComponent(typeof(JRigidBody))]
    public class JMeshCollider : JColliderBase
    {
        public Mesh mesh;

        public override Shape CreateShape()
        {
            if (mesh == null) return new ConvexHullShape(new());
            return new ConvexHullShape(mesh.ToTriangleList().First());
        }

        void Reset()
        {
            mesh = GetComponent<MeshFilter>().sharedMesh;
        }
    }
}

