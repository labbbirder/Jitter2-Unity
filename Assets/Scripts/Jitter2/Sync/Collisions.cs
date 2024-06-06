using System;
using Jitter2.Sync;
using System.Collections.Generic;

// namespace Jitter2.Collision
// {

//     partial class Island : ISync
//     {
//         static Stack<Island> Pool = new();

//         public ISync CreateSimilar(SyncContext ctx)
//         {
//             if (!Pool.TryPop(out var inst))
//             {
//                 inst = new();
//             }
//             return inst;
//         }

//         public void SyncFrom(ISync another, SyncContext ctx)
//         {
//             var other = another as Island;
//             ctx.SyncFromExtra(bodies, other.bodies);
//             MarkedAsActive = other.MarkedAsActive;
//             NeedsUpdate = other.NeedsUpdate;
//         }
//     }
// }

// namespace Jitter2.Collision.Shapes
// {
//     partial class Shape : ISync
//     {
//         public virtual ISync CreateSimilar(SyncContext ctx)
//         {
//             throw new NotImplementedException();
//         }



//         public virtual void SyncFrom(ISync another, SyncContext ctx)
//         {
//             var other = another as Shape;
//             ShapeId = other.ShapeId;
//             RigidBody = ctx.CreateFrom(other.RigidBody);

//             throw new NotImplementedException();
//         }

//     }
//     partial class BoxShape
//     {
//         static Stack<BoxShape> Pool = new();

//         public override ISync CreateSimilar(SyncContext ctx)
//         {
//             if (!Pool.TryPop(out var inst))
//             {
//                 inst = new(Size);
//             }
//             return inst;
//         }

//         public override void SyncFrom(ISync another, SyncContext ctx)
//         {
//             base.SyncFrom(another, ctx);
//             var other = another as BoxShape;

//             halfSize = other.halfSize;
//         }
//     }
//     partial class CapsuleShape
//     {
//         public override void Sync(CapsuleShape other)
//         {
//             base.Sync(other);
//             radius = other.radius;
//             halfLength = other.halfLength;

//         }
//     }
//     partial class ConeShape
//     {
//         public override void Sync(ConeShape other)
//         {
//             base.Sync(other);
//             radius = other.radius;
//             height = other.height;
//         }
//     }
//     partial class ConeShape
//     {
//         public override void Sync(ConeShape other)
//         {
//             base.Sync(other);
//             radius = other.radius;
//             height = other.height;
//         }
//     }
//     partial class CylinderShape
//     {
//         public override void Sync(CylinderShape other)
//         {
//             base.Sync(other);
//             radius = other.radius;
//             height = other.height;
//         }
//     }
// }