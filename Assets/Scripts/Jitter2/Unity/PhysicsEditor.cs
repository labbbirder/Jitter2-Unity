#if UNITY_EDITOR
using Jitter2.DataStructures;
using Jitter2.Unity2D;
using UnityEditor;
using UnityEngine;

namespace Jitter2.Unity
{

    public class PhysicsEditor : MonoBehaviour
    {
        static World _world;
        public static World World => _world ??= new();

        [InitializeOnLoadMethod]
        static void Init()
        {
            EditorApplication.hierarchyChanged += () =>
            {
                var bodies = FindObjectsByType<JRigidBody>(FindObjectsSortMode.None);
                foreach (var body in bodies)
                {
                    body.Refresh();
                }
                
                var bodies2D = FindObjectsByType<JRigidBody2D>(FindObjectsSortMode.None);
                foreach (var body in bodies2D)
                {
                    body.Refresh();
                }
            };
            EditorApplication.playModeStateChanged += s =>
            {
                if (s == PlayModeStateChange.EnteredPlayMode)
                {
                    _world?.Clear();
                }
            };
        }

        [DrawGizmo(GizmoType.Selected | GizmoType.Active | GizmoType.NonSelected)]
        static void DrawRigidBodyGizmos(JRigidBody rb, GizmoType gizmoType)
        {
            var item = rb.body as IListIndex;
            if (item.ListIndex == -1) return;
            if (!Application.isPlaying)
            {
                rb.UpdateTransform(rb.body);
                rb.body.DebugDraw(JitterGizmosDrawer.Instance);
            }
        }

        [DrawGizmo(GizmoType.Selected | GizmoType.Active | GizmoType.NonSelected)]
        static void DrawRigidBody2DGizmos(JRigidBody2D rb, GizmoType gizmoType)
        {
            if (!Application.isPlaying)
            {
                rb.UpdateTransform(rb.body);
                rb.body.DebugDraw(JitterGizmosDrawer.Instance);
            }
        }
    }
}
#endif