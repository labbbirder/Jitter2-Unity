using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Jitter2;
using Jitter2.Collision.Shapes;
using Jitter2.DataStructures;
using Jitter2.LinearMath;
using Jitter2.Sync;
using Jitter2.Unity;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using Random = LSMath.Random;

public class Manager : MonoBehaviour
{
    public TextMeshProUGUI text;
    Random rand = new();
    [ShowInInspector]
    World world1;
    [ShowInInspector]
    World world2;
    [ShowInInspector]
    ReadOnlyActiveList<Jitter2.Dynamics.RigidBody> Rb1;
    void OnDrawGizmos()
    {
        if (world1?.RigidBodies is null) return;
        JitterGizmosDrawer.Instance.color = Color.green;
        foreach (var rb in world1.RigidBodies)
        {
            rb.DebugDraw(JitterGizmosDrawer.Instance);
        }
        JitterGizmosDrawer.Instance.color = Color.blue;
        foreach (var rb in world2.RigidBodies)
        {
            rb.DebugDraw(JitterGizmosDrawer.Instance);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        Application.targetFrameRate = -1;
        world1 = CreateInitialWorld("world1");
        world2 = CreateInitialWorld("world2");
        Rb1 = world1.RigidBodies;
        var seed = (ulong)DateTime.Now.Ticks;
        rand.Seed = seed;
        // world1.SyncFrom(world2, new());
    }
    World CreateInitialWorld(string name)
    {
        var world = new World() { Name = name };
        foreach (var rb in FindObjectsOfType<JRigidBody>())
        {
            rb.CreateBody(world);
        }
        // foreach (var box in FindObjectsOfType<BoxCollider>())
        // {
        //     var urb = box.GetComponent<Rigidbody>();
        //     var rgb = world.CreateRigidBody();
        //     rgb.Position = (box.transform.position - box.center).ToVector();
        //     rgb.Orientation = box.transform.rotation.ToQuaternion();
        //     rgb.IsStatic = !urb;
        //     var size = box.size;
        //     size.Scale(box.transform.localScale);
        //     rgb.AddShape(new BoxShape(size.ToVector()));
        //     // var link = box.gameObject.AddComponent<EntityLink>();
        //     // link.rb = rgb;
        //     if (urb) Destroy(urb);
        //     Destroy(box);
        // }
        world.Step(0.02f, false);
        world.Step(0.02f, false);
        world.Step(0.02f, false);
        return world;
    }
    void FixedUpdate()
    {
        // Dictionary<long, int> id2idx = new();
        // world.PostStep += (float f) =>
        // {
        //     id2idx.Clear();
        //     foreach (var rb in world.RigidBodies)
        //     {
        //         id2idx[rb.RigidBodyId] = rb.Data._index;
        //     }
        // };
        world1.Step(0.02f, false);
    }

    void DroppingBoxesUpdate()
    {
        if (rand.Norm01() > 0.9f)
        {
            var go = new GameObject();
            var rb = go.AddComponent<JRigidBody>();
            var box = go.AddComponent<JBoxCollider>();
            go.transform.position = new Vector3(rand.Range(-1, 1), 0, rand.Range(-1, 1));
            rb.CreateBody(world1);
            text.text = $"{world1.RigidBodies.Active} / {world1.RigidBodies.Count}";
        }
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.S))
        {
            new SyncContext().SyncFrom(ref world2, world1);
        }
        if (Input.GetKeyDown(KeyCode.R))
        {
            var ctx = new SyncContext();
            ctx.SyncFrom(ref world1, world2);
            world1.Step(0.02f, false);
            world1.Step(0.02f, false);
            // VisitAndCompare(world1, world2);
        }
        if (Input.GetKey(KeyCode.KeypadMinus))
        {
            var idx = rand.Range(0, world1.RigidBodies.Count);
            print($"{idx} {world1.RigidBodies.Count}");
            world1.Remove(world1.RigidBodies[idx]);
        }
        if (Input.GetKey(KeyCode.KeypadPlus))
        {
            var go = new GameObject();
            var rb = go.AddComponent<JRigidBody>();
            var box = go.AddComponent<JBoxCollider>();
            go.transform.position = new Vector3(rand.Range(-1, 1), 0, rand.Range(-1, 1));
            rb.CreateBody(world1);
        }
        // DroppingBoxesUpdate();
        text.text = $"{world1.RigidBodies.Active} / {world1.RigidBodies.Count}";
    }
    HashSet<object> lut = new();
    void VisitAndCompare(object a, object b, string path = "")
    {
        print($"cmp {a}-{b} {path}");
        var type = a.GetType();
        var fields = type.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
        lut.Add(a);
        foreach (var f in fields)
        {
            var ftype = f.FieldType;
            var vala = f.GetValue(a);
            var valb = f.GetValue(b);
            VisitField(path + $"{type}.{f.Name}", vala, valb);
        }
        void VisitField(string tip, object vala, object valb)
        {
            if ((vala ?? valb) is null) return;
            var ftype = (vala ?? valb).GetType();
            // var vala = f.GetValue(a);
            // var valb = f.GetValue(b);
            if (ftype.IsPointer || typeof(Delegate).IsAssignableFrom(ftype) || ftype == typeof(Pointer))
            {
                return;
            }
            if (ftype.Name.Contains("Dictionary")) return;
            if (ftype.Name.Contains("HashList")) return;
            if (ftype.IsValueType)
            {
                Assert.IsTrue(vala == valb, $"{type} {tip}");
            }
            else
            {
                if (vala is null && valb is null) return;
                Assert.IsTrue(vala != null && valb != null, $"{type}  is not null. {tip}");
                if (ftype.IsArray)
                {
                    var arra = vala as Array;
                    var arrb = valb as Array;
                    for (int i = 0; i < arrb.Length; i++)
                    {
                        VisitField(tip + $" {ftype} {i}", arra.GetValue(i), arrb.GetValue(i));
                    }
                }
                else
                {
                    print($"{type} {ftype}");
                    if (lut.Contains(vala)) return;
                    VisitAndCompare(vala, valb, $"{tip} ");
                }
            }
        }
    }
    void RandomStep(Random rand)
    {

    }
}
