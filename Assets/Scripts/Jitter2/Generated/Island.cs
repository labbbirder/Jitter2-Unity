
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
                        
            this.MarkedAsActive = other.MarkedAsActive;
            this.NeedsUpdate = other.NeedsUpdate;
        }
        
        public  void ReferencePhaseSyncFrom(ISync another, SyncContext ctx){
            
            var other = another as Island;
            bodies.Clear();
            foreach(var b in other.bodies)
            {
                var f = ctx.GetCache(null, b, out var res);
                Assert.IsTrue(f.needCreate == false);
                Assert.IsTrue(f.needSync == false);
                bodies.Add(res);
            }
        }
                    
    }
}
