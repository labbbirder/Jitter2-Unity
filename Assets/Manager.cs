using System;
using System.Collections;
using System.Collections.Generic;
using Jitter2;
using Jitter2.Collision.Shapes;
using Jitter2.LinearMath;
using UnityEngine;
using Random = LSMath.Random;

public class Manager : MonoBehaviour
{
    Random rand = new();
    World world;
    void OnDrawGizmos()
    {
        if (world?.RigidBodies is null) return;
        foreach (var rb in world.RigidBodies)
        {
            rb.DebugDraw(JitterGizmosDrawer.Instance);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        world = new World();
        foreach (var box in FindObjectsOfType<BoxCollider>())
        {
            var urb = box.GetComponent<Rigidbody>();
            var rgb = world.CreateRigidBody();
            rgb.Position = (box.transform.position - box.center).ToVector();
            rgb.Orientation = box.transform.rotation.ToQuaternion();
            rgb.IsStatic = !urb;
            var size = box.size;
            size.Scale(box.transform.localScale);
            rgb.AddShape(new BoxShape(size.ToVector()));
            var link = box.gameObject.AddComponent<EntityLink>();
            link.rb = rgb;
            if (urb) Destroy(urb);
            Destroy(box);
        }
        var seed = (ulong)DateTime.Now.Ticks;
        rand.Seed = seed;

    }

    void FixedUpdate()
    {
        world.Step(Time.fixedDeltaTime, false);
    }
}
