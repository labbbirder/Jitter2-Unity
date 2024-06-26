
using System.Runtime.CompilerServices;
using Jitter2.Sync;

namespace Jitter2.DataStructures
{
    partial class SlimBag<T> : ISync// where T : class, ISync
    {
        public void SyncFrom(ISync other, SyncContext ctx)
        {
            var another = other as SlimBag<T>;
            counter = another.counter;
            nullOut = another.nullOut;
            array = ctx.SyncArray(array, another.array);
        }

        public ISync CreateSimilar(SyncContext ctx)
            => new SlimBag<T>(InternalSize);
    }

    partial class ActiveList<T> //: ISync //where T : class, ISync
    {
        internal ref T[] Elements => ref elements;
        // public ISync CreateSimilar(SyncContext ctx)
        //     => new ActiveList<T>(elements.Length);

        // public void SyncFrom(ISync other, SyncContext ctx)
        // {
        //     var another = other as ActiveList<T>;
        //     Active = another.Active;
        //     Count = another.Count;
        //     elements = ctx.SyncArray(elements, another.elements);
        //     for (int i = 0; i < Count; i++)
        //     {
        //         this[i].ListIndex = i;
        //     }
        // }
    }
}

namespace Jitter2.UnmanagedMemory
{

    unsafe partial class UnmanagedActiveList<T> : ISync
    {
        internal nint Addr => (nint)handles;
        int _maxEverCount;
        public ISync CreateSimilar(SyncContext ctx)
        {
            throw new System.NotImplementedException();
        }

        public void SyncFrom(ISync another, SyncContext ctx)
        {
            var other = another as UnmanagedActiveList<T>;
            memory = ctx.SyncMemory(memory, size * sizeof(T), other.memory, other.size * sizeof(T));
            handles = (T**)ctx.SyncMemory((nint*)handles, size * sizeof(nint), (nint*)other.handles, other.size * sizeof(nint));


            active = other.active;
            size = other.size;
            disposed = other.disposed;
            Count = other.Count;
            for (int i = 0; i < size; i++)
            {
                handles[i] = (T*)((nint)handles[i] - (nint)other.memory + (nint)memory);
            }
        }
    }
}

namespace Jitter2.Collision
{
    partial class DynamicTree<T> : ISyncStageReceiver
    {
        void ISyncStageReceiver.OnEnterStage(int stage, ISync another, SyncContext ctx)
        {
            if (stage == 0)
            {
                var other = another as DynamicTree<T>;
                ctx.EnsureArraySize(ref Nodes, other.Nodes.Length);
                for (int i = 0; i < other.Nodes.Length; i++)
                {
                    var node = other.Nodes[i];
                    ctx.GetCache(null, node.Proxy, out node.Proxy);
                    Nodes[i] = node;
                }
            }
        }
    }
}