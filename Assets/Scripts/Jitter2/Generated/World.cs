
using static Jitter2.Sync.HandleIndex;
using Jitter2.Sync;

namespace Jitter2
{
    partial class World : ISync, ISyncStageReceiver
    {
        World() {}
        
        public  ISync CreateSimilar(SyncContext ctx) => new World();
                
        public  void SyncFrom(ISync another, SyncContext ctx){
            
            var other = another as World;
            
            ;(this as ISyncStageReceiver).OnEnterStage(-1, another, ctx);

            ctx.SyncFrom(memContacts, other.memContacts);
            ctx.SyncFrom(memRigidBodies, other.memRigidBodies);
            ctx.SyncFrom(memConstraints, other.memConstraints);
            ctx.SyncFrom(memSmallConstraints, other.memSmallConstraints);
            ;(this as ISyncStageReceiver).OnEnterStage(0, another, ctx);
                        
            this.NullBody = ctx.SyncFrom(NullBody, other.NullBody);
            // if (ctx.GetOrCreate(arbiters, other.arbiters,out var _arbiters))
            //     ctx.SyncFromExtra(_arbiters, other.arbiters);
            // ctx.SyncFrom(islands, other.islands);
            // ctx.SyncFrom(bodies, other.bodies);
            // ctx.SyncFrom(shapes, other.shapes);
            this._idCounter = other._idCounter;
            ctx.SyncFrom(DynamicTree, other.DynamicTree);
            this.gravity = other.gravity;
            ;(this as ISyncStageReceiver).OnEnterStage(1, another, ctx);
                        
            this.stepper = other.stepper;
        }
        
    }
}
