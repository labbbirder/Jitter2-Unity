
using static Jitter2.Sync.HandleIndex;
using Jitter2.Sync;

namespace Jitter2.Dynamics
{
    partial class RigidBody : ISync, ISyncStageReceiver
    {
        RigidBody() {}
        
        public  ISync CreateSimilar(SyncContext ctx) => new RigidBody();
                
        public  void SyncFrom(ISync another, SyncContext ctx){
            
            var other = another as RigidBody;
            
            ;(this as ISyncStageReceiver).OnEnterStage(0, another, ctx);
                        
            this.RigidBodyId = other.RigidBodyId;
            this.Friction = other.Friction;
            this.Restitution = other.Restitution;
            this.World = ctx.SyncFrom(World, other.World);
            this.AffectedByGravity = other.AffectedByGravity;
            this.EnableSpeculativeContacts = other.EnableSpeculativeContacts;
            this.handle = ctx.SyncFromExtra(other.handle, RIGID_BODY_DATA);
            if (ctx.GetOrCreate(shapes, other.shapes,out var _shapes))
                ctx.SyncFromExtra(_shapes, other.shapes);
            this.island = ctx.SyncFrom(island, other.island);
            if (ctx.GetOrCreate(connections, other.connections,out var _connections))
                ctx.SyncFromExtra(_connections, other.connections);
            if (ctx.GetOrCreate(contacts, other.contacts,out var _contacts))
                ctx.SyncFromExtra(_contacts, other.contacts);
            this.islandMarker = other.islandMarker;
            this.sleepTime = other.sleepTime;
            this.inactiveThresholdLinearSq = other.inactiveThresholdLinearSq;
            this.inactiveThresholdAngularSq = other.inactiveThresholdAngularSq;
            this.deactivationTimeThreshold = other.deactivationTimeThreshold;
            this.linearDampingMultiplier = other.linearDampingMultiplier;
            this.angularDampingMultiplier = other.angularDampingMultiplier;
            this.inverseInertia = other.inverseInertia;
            this.inverseMass = other.inverseMass;
            this.hashCode = other.hashCode;
        }
    }
}
