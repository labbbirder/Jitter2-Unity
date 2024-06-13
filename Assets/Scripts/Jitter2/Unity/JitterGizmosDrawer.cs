using Jitter2;
using Jitter2.LinearMath;
using UnityEngine;

public class JitterGizmosDrawer : IDebugDrawer
{
    static JitterGizmosDrawer m_Instance;
    public static JitterGizmosDrawer Instance => m_Instance ??= new();
    public Color color { get; set; } = Color.green;

    public void DrawCube(in JVector p, in JQuaternion ori, in JVector size)
    {
        Gizmos.matrix = Matrix4x4.TRS(p.ToVector(), ori.ToQuaternion(), Vector3.one);
        Gizmos.color = color;
        Gizmos.DrawWireCube(Vector3.zero, size.ToVector());
        Gizmos.matrix = Matrix4x4.identity;
    }

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

    public void DrawSphere(in JVector p, in JQuaternion ori, float radius)
    {
        Gizmos.matrix = Matrix4x4.TRS(p.ToVector(), ori.ToQuaternion(), Vector3.one);
        Gizmos.color = color;
        Gizmos.DrawWireSphere(Vector3.zero, radius);
        Gizmos.matrix = Matrix4x4.identity;
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