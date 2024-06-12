using Jitter2.Collision;
using Jitter2.Collision.Shapes;
using UnityEngine;

namespace Jitter2
{
    public class LayerFilter : IBroadPhaseFilter
    {
        public bool Filter(Shape shapeA, Shape shapeB)
        {
            var udA = shapeA.RigidBody.Tag as RigidBodyUserData;
            var udB = shapeB.RigidBody.Tag as RigidBodyUserData;
            if ((udA ?? udB) is null) return true;
            return !Physics.GetIgnoreLayerCollision(udA.Layer, udB.Layer);
        }
    }
}