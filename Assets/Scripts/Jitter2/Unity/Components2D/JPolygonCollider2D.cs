using System;
using System.Collections.Generic;
using System.Linq;
using Jitter2.Collision;
using Jitter2.Collision.Shapes;
using Jitter2.LinearMath;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Jitter2.Unity2D
{
    [AddComponentMenu("JPhysics2D/PolygonCollider2D")]
    [RequireComponent(typeof(JRigidBody2D))]
    public class JPolygonCollider2D : JCollider2DBase
    {
        [Header("Create From...")]
        public Sprite sprite;
        public PolygonCollider2D poly2d;
        // [HideInInspector]
        public List<JTriangle> triangles = new();
        public override IEnumerable<Shape> CreateShape()
        {
            if (sprite)
            {
                foreach (var tl in sprite.ToTriangleList())
                {
                    Assert.IsTrue(tl.Count > 0);
                    yield return new Mesh2DShape(tl);
                }
            }
            else if (poly2d)
            {
                foreach (var tl in poly2d.ToTriangleList())
                {
                    Assert.IsTrue(tl.Count > 0);
                    yield return new Mesh2DShape(tl);
                }
            }
        }

        void OnValidate()
        {
            if (!sprite)
            {
                var sr = GetComponent<SpriteRenderer>();
                if (sr) sprite = sr.sprite;
            }
            if (!sprite)
            {
                triangles.Clear();
                return;
            }

            // var mesh = CreatePhysicsMesh(sprite);
            // triangles = mesh.ToTriangleList();
        }
    }
}

