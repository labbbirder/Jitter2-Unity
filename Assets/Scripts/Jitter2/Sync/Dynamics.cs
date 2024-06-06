
using System;
using System.Collections.Generic;
using Jitter2.DataStructures;
using Jitter2.Sync;
using static Jitter2.Sync.HandleIndex;





namespace Jitter2.Dynamics
{
    // unsafe partial class RigidBody : ISync
    // {

    //     internal RigidBody() { }

    //     public ISync CreateSimilar(SyncContext ctx) => new RigidBody();

    //     public void SyncFrom(ISync another, SyncContext ctx)
    //     {
    //         var other = another as RigidBody;

    //         handle.Pointer = other.handle.Pointer + ctx.GetMemoryOffset(RIGID_BODY_DATA);

    //         RigidBodyId = other.RigidBodyId;
    //         ctx.SyncFrom(ref island, other.island);
    //         // shapes;
    //         // island;
    //         // connections;
    //         ctx.SyncFromExtra(connections, other.connections);
    //         // contacts;
    //         // constraints;
    //         ctx.SyncFromExtra(constraints, other.constraints);

    //         islandMarker = other.islandMarker;
    //         sleepTime = other.sleepTime;

    //         inactiveThresholdLinearSq = other.inactiveThresholdLinearSq;
    //         inactiveThresholdAngularSq = other.inactiveThresholdAngularSq;
    //         deactivationTimeThreshold = other.deactivationTimeThreshold;

    //         linearDampingMultiplier = other.linearDampingMultiplier;
    //         angularDampingMultiplier = other.angularDampingMultiplier;

    //         inverseInertia = other.inverseInertia;
    //         inverseMass = other.inverseMass;

    //         Friction = other.Friction;
    //         Restitution = other.Restitution;

    //         hashCode = other.hashCode;
    //         World = ctx.SyncFrom(World, other.World);

    //         AffectedByGravity = other.AffectedByGravity;

    //         // /// <summary>
    //         // /// A managed pointer to custom user data. This is not utilized by the engine.
    //         // /// </summary>
    //         // public object? Tag { get; set; }

    //         EnableSpeculativeContacts = other.EnableSpeculativeContacts;

    //         // for Force and Torque, update just after World step
    //     }

    // }


    // namespace Constraints
    // {

    //     partial class Constraint : ISync
    //     {
    //         public ISync CreateSimilar(SyncContext ctx)
    //         {
    //             throw new NotImplementedException();
    //         }

    //         public void SyncFrom(ISync another, SyncContext ctx)
    //         {
    //             var other = another as Constraint;

    //             Body1 = ctx.SyncFrom(Body1, other.Body1);
    //             Body2 = ctx.SyncFrom(Body2, other.Body2);

    //             // Handle = ctx.CreateFrom(other.Handle, CONSTRAINT_DATA);
    //         }
    //     }
    // }
}


namespace Jitter2
{

    unsafe partial class World : ISyncStageReceiver
    {

        void ISyncStageReceiver.OnEnterStage(int stage, ISync another, SyncContext ctx)
        {
            var other = another as World;
            if (stage == 0)
            {
                ctx.RegisterMemoryOffset(RIGID_BODY_DATA, memRigidBodies.Addr - other.memRigidBodies.Addr);
                ctx.RegisterMemoryOffset(CONTACT_DATA, memContacts.Addr - other.memContacts.Addr);
                ctx.RegisterMemoryOffset(CONSTRAINT_DATA, memConstraints.Addr - other.memConstraints.Addr);
                ctx.RegisterMemoryOffset(SMALL_CONSTRAINT_DATA, memSmallConstraints.Addr - other.memSmallConstraints.Addr);

                foreach (ref var ele in memContacts.Elements)
                {
                    ele.Body1.Pointer += ctx.GetMemoryOffset(RIGID_BODY_DATA);
                    ele.Body2.Pointer += ctx.GetMemoryOffset(RIGID_BODY_DATA);
                }
                foreach (ref var ele in memConstraints.Elements)
                {
                    ele.Body1.Pointer += ctx.GetMemoryOffset(RIGID_BODY_DATA);
                    ele.Body2.Pointer += ctx.GetMemoryOffset(RIGID_BODY_DATA);
                }
                foreach (ref var ele in memSmallConstraints.Elements)
                {
                    ele.Body1.Pointer += ctx.GetMemoryOffset(RIGID_BODY_DATA);
                    ele.Body2.Pointer += ctx.GetMemoryOffset(RIGID_BODY_DATA);
                }
            }
        }
    }
}
