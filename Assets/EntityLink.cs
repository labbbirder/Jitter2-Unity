using System.Collections;
using System.Collections.Generic;
using Jitter2;
using Jitter2.Collision.Shapes;
using Jitter2.LinearMath;
using UnityEngine;

public class EntityLink : MonoBehaviour
{
    public Jitter2.Dynamics.RigidBody rb;
    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = rb.Position.ToVector();
        transform.rotation = rb.Orientation.ToQuaternion();
    }
}
