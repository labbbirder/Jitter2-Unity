
using System;
using System.Collections.Generic;
using Jitter2.DataStructures;
using Jitter2.Dynamics;
using Jitter2.Sync;
using static Jitter2.Sync.HandleIndex;


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

                brokenArbiters.Clear();
                for (int i = 0; i < other.brokenArbiters.Count; i++)
                {
                    brokenArbiters.Add(ctx.SyncFromExtra(other.brokenArbiters[i], CONTACT_DATA));
                }

                SyncIdentitiedActiveList(bodies, other.bodies, id2idx, ctx);
                SyncIdentitiedActiveList(shapes, other.shapes, id2idx, ctx);
                SyncIdentitiedActiveList(islands, other.islands, id2idx, ctx);

                // sync arbiters
                foreach (var (k, marb) in arbiters)
                {
                    if (!other.arbiters.ContainsKey(k))
                    {
                        removingArbiters.Push((k, marb));
                    }
                }

                foreach (var (k, arb) in removingArbiters)
                {
                    arbiters.Remove(k);
                }

                foreach (var (k, oarb) in other.arbiters)
                {
                    if (!arbiters.TryGetValue(k, out var marb))
                    {
                        if (removingArbiters.TryPop(out var pooled))
                        {
                            marb = pooled.arbiter;
                        }
                        else
                        {
                            marb = oarb.CreateSimilar(ctx) as Arbiter;
                        }

                        AddingArbiters.Push((k, marb));
                    }

                    marb.ReferencePhaseSyncFrom(oarb, ctx);
                    ctx.SyncFrom(ref marb, oarb);
                }

                removingArbiters.Clear();

                while (AddingArbiters.TryPop(out var rec))
                {
                    arbiters[rec.k] = rec.arbiter;
                }

                ctx.RaiseOnCreate = true;

                for (int i = 0; i < bodies.Count; i++)
                {
                    bodies[i].ReferencePhaseSyncFrom(other.bodies[i], ctx);
                }

                for (int i = 0; i < shapes.Count; i++)
                {
                    shapes[i].ReferencePhaseSyncFrom(other.shapes[i], ctx);
                }

                for (int i = 0; i < islands.Count; i++)
                {
                    islands[i].ReferencePhaseSyncFrom(other.islands[i], ctx);
                }

                foreach (var (k, arb) in arbiters)
                {
                    Assert.IsTrue(other.arbiters.ContainsKey(k));
                    memContacts.IsActive(arb.Handle);
                    arb.ReferencePhaseSyncFrom(other.arbiters[k], ctx);
                }
            }
        }

        void SyncIdentitiedActiveList<T>(ActiveList<T> self, ActiveList<T> other, SortedList<long, int> id2idx, SyncContext ctx) where T : class, IListIndex, IIdentityObject, ISync
        {
            id2idx.Clear();
            GetId2Idx(self, id2idx);

            ctx.SetCache(self, other);

            ctx.EnsureArraySize(ref self.Elements, other.Count, true);
            int i = 0;
            for (; i < other.Count; i++)
            {
                var oitem = other.Elements[i];
                ref var mitem = ref self.Elements[i];
                if (mitem is null)
                {
                    ctx.SyncFrom(ref mitem, oitem);
                    mitem.ListIndex = i;
                    Assert.IsTrue(self[i].ListIndex == i);
                    continue;
                }

                var mid = mitem.Id;
                var oid = oitem.Id;
                if (mid != oid)
                {
                    if (!id2idx.TryGetValue(oid, out var ridx))
                    {
                        ridx = self.Count;
                        self.Add(ctx.SyncFrom(null, oitem), false);
                    }
                    else
                    {
                        if (typeof(T) == typeof(RigidBody))
                        {
                            var m = self[ridx] as RigidBody;
                            var o = oitem as RigidBody;
                            if (m.shapes.Count > 0)
                                Assert.IsTrue(m.shapes[0].Id == o.shapes[0].Id);

                        }
                        self[ridx] = ctx.SyncFrom(self[ridx], oitem);
                    }

                    id2idx[mid] = ridx;
                    self.Swap(ridx, i);
                }
                else
                {
                    ctx.SyncFrom(ref mitem, oitem);
                }
                if (typeof(T) == typeof(RigidBody))
                {
                    var m = self[i] as RigidBody;
                    var o = other[i] as RigidBody;
                    if (m.shapes.Count>0)
                    Assert.IsTrue(m.shapes[0].Id == o.shapes[0].Id);

                }
            }

            for (; i < self.Count; i++)
            {
                ref var item = ref self.Elements[i];
                if (item != null)
                {
                    for (int j = 0; j < i; j++)
                    {
                        Assert.IsTrue(self[j] != item);
                    }
                    item.ListIndex = -1;
                    ctx.SyncFrom(ref item, null);
                }
            }

            self.Count = other.Count;
            self.Active = other.Active;

            id2idx.Clear();
        }

        void GetId2Idx<T>(ActiveList<T> activeList, SortedList<long, int> dict) where T : class, IListIndex, IIdentityObject
        {
            dict.Clear();
            for (int i = 0; i < activeList.Count; i++)
            {
                var b = activeList[i];
                dict[b.Id] = i;
            }
        }
        static SortedList<long,int> id2idx = new();
        // static Dictionary<long, int> id2idx = new();
        static Stack<(ArbiterKey k, Arbiter arbiter)> removingArbiters = new();
        static Stack<(ArbiterKey k, Arbiter arbiter)> AddingArbiters = new();
    }
}
