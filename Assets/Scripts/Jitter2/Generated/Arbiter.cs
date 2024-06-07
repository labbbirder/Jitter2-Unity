
using static Jitter2.Sync.HandleIndex;
using Jitter2.Sync;

namespace Jitter2.Dynamics
{
    partial class Arbiter : ISync, ISyncStageReceiver
    {
        
        
        public  ISync CreateSimilar(SyncContext ctx) => new Arbiter();
                
        public  void SyncFrom(ISync another, SyncContext ctx){
            
            var other = another as Arbiter;
            
            ;(this as ISyncStageReceiver).OnEnterStage(0, another, ctx);
                        
            this.Body1 = ctx.SyncFrom(Body1, other.Body1);
            this.Body2 = ctx.SyncFrom(Body2, other.Body2);
            this.Handle = ctx.SyncFromExtra(other.Handle, CONTACT_DATA);
        }
    }
}
