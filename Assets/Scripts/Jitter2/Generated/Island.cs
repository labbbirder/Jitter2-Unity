
using static Jitter2.Sync.HandleIndex;
using Jitter2.Sync;

namespace Jitter2.Collision
{
    partial class Island : ISync, ISyncStageReceiver
    {
        
        
        public  ISync CreateSimilar(SyncContext ctx) => new Island();
                
        public  void SyncFrom(ISync another, SyncContext ctx){
            
            var other = another as Island;
            
            ;(this as ISyncStageReceiver).OnEnterStage(0, another, ctx);
                        
            if (ctx.GetOrCreate(bodies, other.bodies,out var _bodies))
                ctx.SyncFromExtra(_bodies, other.bodies);
            this.MarkedAsActive = other.MarkedAsActive;
            this.NeedsUpdate = other.NeedsUpdate;
        }
    }
}
