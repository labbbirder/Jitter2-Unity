
using static Jitter2.Sync.HandleIndex;
using Jitter2.Sync;

namespace Jitter2.Collision
{
    partial class DynamicTree<T> : ISync, ISyncStageReceiver
    {
        DynamicTree() {}
        
        public  ISync CreateSimilar(SyncContext ctx) => new DynamicTree<T>();
                
        public  void SyncFrom(ISync another, SyncContext ctx){
            
            var other = another as DynamicTree<T>;
            
            ;(this as ISyncStageReceiver).OnEnterStage(0, another, ctx);
                        
            if (ctx.GetOrCreate(lists, other.lists,out var _lists))
                this.lists = ctx.SyncArray(_lists, other.lists);
            ctx.SyncFrom(PotentialPairs, other.PotentialPairs);
            if (ctx.GetOrCreate(freeNodes, other.freeNodes,out var _freeNodes))
                ctx.SyncFromExtraUnmanaged(_freeNodes, other.freeNodes);
            this.nodePointer = other.nodePointer;
            this.root = other.root;
        }
        
    }
}
