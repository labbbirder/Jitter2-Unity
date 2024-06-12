
using System;
using Jitter2.LinearMath;
using Jitter2.Unity2D;
using UnityEngine;

class Player2D : MonoBehaviour
{
    public float speed = 3;
    JRigidBody2D body2d;
    void Start()
    {
        body2d = GetComponent<JRigidBody2D>();
    }
    public void Update()
    {
        var v = JVector.Zero;
        if (Input.GetKey(KeyCode.UpArrow))
        {
            v += JVector.UnitY * speed;
        }
        if (Input.GetKey(KeyCode.DownArrow))
        {
            v -= JVector.UnitY * speed;
        }
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            v -= JVector.UnitX * speed;
        }
        if (Input.GetKey(KeyCode.RightArrow))
        {
            v += JVector.UnitX * speed;
        }
        if (Input.GetKeyDown(KeyCode.Space))
        {
            body2d.body.AffectedByGravity ^= true;
        }
        body2d.body.SetActivationState(true);
        body2d.body.Velocity = v;
    }
}