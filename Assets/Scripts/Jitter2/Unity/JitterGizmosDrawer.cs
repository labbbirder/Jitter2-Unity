using Jitter2;
using Jitter2.LinearMath;
using UnityEngine;

public class JitterGizmosDrawer : IDebugDrawer
{
    static JitterGizmosDrawer m_Instance;
    public static JitterGizmosDrawer Instance => m_Instance ??= new();
    readonly Color color = Color.green;
    public void DrawPoint(in JVector p)
    {
        Gizmos.color = color;
        Gizmos.DrawSphere(p.ToVector(), 0.1f);
    }

    public void DrawSegment(in JVector pA, in JVector pB)
    {
        Gizmos.color = color;
        Gizmos.DrawLine(pA.ToVector(), pB.ToVector());
    }

    public void DrawTriangle(in JVector pA, in JVector pB, in JVector pC)
    {
        Gizmos.color = color;
        Gizmos.DrawLineStrip(new[]{
            pA.ToVector(),
            pB.ToVector(),
            pC.ToVector(),
        }, true);
    }
}