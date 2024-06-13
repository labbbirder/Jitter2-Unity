using System.Collections.Generic;
using System.Diagnostics;
using Jitter2.Collision.Shapes;
using Jitter2.LinearMath;

namespace Jitter2.Collision
{
    public class Mesh2DShape : ConvexHullShape
    {
        public Mesh2DShape(List<JTriangle> triangles) : base(triangles)
        {
        }

        public override void CalculateMassInertia(out JMatrix inertia, out JVector com, out float mass)
        {
            inertia = JMatrix.Identity;
            mass = 1;
            com = JVector.Zero;
        }

        public override void SupportMap(in JVector direction, out JVector result)
        {
            base.SupportMap(direction, out result);
            if (direction.Z > 0)
            {
                result.Z = 0.5f;
            }
            else
            {
                result.Z = -0.5f;
            }
        }
    }
}