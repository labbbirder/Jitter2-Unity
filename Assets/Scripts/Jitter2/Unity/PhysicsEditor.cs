#if UNITY_EDITOR
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
            };
            EditorApplication.playModeStateChanged += s =>
            {
                if (s == PlayModeStateChange.ExitingEditMode)
                {
                    _world?.Clear();
                }
            };
        }

        [DrawGizmo(GizmoType.Selected | GizmoType.Active | GizmoType.NonSelected)]
        static void DrawRigidBodyGizmos(JRigidBody rb, GizmoType gizmoType)
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