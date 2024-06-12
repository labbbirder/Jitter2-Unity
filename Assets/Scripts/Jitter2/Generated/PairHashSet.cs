
using static Jitter2.Sync.HandleIndex;
using Jitter2.Sync;

namespace Jitter2.Collision
{
    partial class PairHashSet : ISync, ISyncStageReceiver
    {
        
        
        public  ISync CreateSimilar(SyncContext ctx) => new PairHashSet();
                
        public  void SyncFrom(ISync another, SyncContext ctx){
            
            var other = another as PairHashSet;
            
            ;(this as ISyncStageReceiver).OnEnterStage(0, another, ctx);
                        
            this.Count = other.Count;
            if (ctx.GetOrCreate(Slots, other.Slots,out var _Slots))
                this.Slots = ctx.SyncUnmanagedArray(_Slots, other.Slots, true);
            this.modder = other.modder;
        }
        
    }
}
