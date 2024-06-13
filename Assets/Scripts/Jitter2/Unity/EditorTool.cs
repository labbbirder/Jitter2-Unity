using System.Collections.Generic;
using System.Linq;
using UnityEngine;
internal class EditorTool : MonoBehaviour
{
    static Dictionary<System.Type, Component> s_instances = new();
    public static T GetSingleton<T>() where T : Component
    {
        if (!s_instances.TryGetValue(typeof(T), out var inst) || (inst as T) == null)
        {
            var tool = Resources.FindObjectsOfTypeAll<EditorTool>().FirstOrDefault();
            if (!tool)
            {
                var go = new GameObject()
                {
                    hideFlags = HideFlags.HideAndDontSave,
                };
                go.transform.position = Vector3.zero;
                go.transform.rotation = Quaternion.identity;
                go.transform.localScale = Vector3.one;
                tool = go.AddComponent<EditorTool>();
            }
            var poly = tool.GetComponent<T>();
            if (!poly)
            {
                poly = tool.gameObject.AddComponent<T>();
            }
            s_instances[typeof(T)] = inst = poly;
        }
        return inst as T;
    }

}