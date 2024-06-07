
using static Jitter2.Sync.HandleIndex;
using Jitter2.Sync;

namespace Jitter2.Collision
{
    partial class DynamicTree<T> : ISync, ISyncStageReceiver
    {
        DynamicTree() { }

        public ISync CreateSimilar(SyncContext ctx) => new DynamicTree<T>();

        public void SyncFrom(ISync another, SyncContext ctx)
        {

            var other = another as DynamicTree<T>;

            ; (this as ISyncStageReceiver).OnEnterStage(0, another, ctx);

            if (ctx.GetOrCreate(lists, other.lists, out var _lists))
                this.lists = ctx.SyncArray(_lists, other.lists);
            ctx.SyncFrom(activeList, other.activeList);
            ctx.SyncFrom(PotentialPairs, other.PotentialPairs);
            ctx.EnsureArraySize(ref Nodes, other.Nodes.Length);
            for (int i = 0; i < other.Nodes.Length; i++)
            {
                ref var node = ref Nodes[i];
                var oldProxy = node.Proxy;
                node = other.Nodes[i];
                node.Proxy = (T)ctx.SyncFrom((ISync)oldProxy, (ISync)node.Proxy);
            }
            // if (ctx.GetOrCreate(Nodes, other.Nodes, out var _Nodes))
            //     this.Nodes = ctx.SyncArray(_Nodes, other.Nodes);
            ctx.SyncFromExtraUnmanaged(freeNodes, other.freeNodes);
            this.root = other.root;
            this.nodePointer = other.nodePointer;
        }
    }
}
