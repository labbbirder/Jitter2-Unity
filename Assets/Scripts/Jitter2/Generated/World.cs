
using static Jitter2.Sync.HandleIndex;
using Jitter2.Sync;
using UnityEngine.Profiling;

namespace Jitter2
{
    partial class World : ISync, ISyncStageReceiver
    {
        World() { }

        public ISync CreateSimilar(SyncContext ctx) => new World();

        public void SyncFrom(ISync another, SyncContext ctx)
        {

            var other = another as World;

            ; (this as ISyncStageReceiver).OnEnterStage(-1, another, ctx);

            Profiler.BeginSample("memContacts sync");
            ctx.SyncFrom(memContacts, other.memContacts);
            Profiler.EndSample();
            Profiler.BeginSample("memRigidBodies sync");
            ctx.SyncFrom(memRigidBodies, other.memRigidBodies);
            Profiler.EndSample();
            Profiler.BeginSample("memConstraints sync");
            ctx.SyncFrom(memConstraints, other.memConstraints);
            Profiler.EndSample();
            Profiler.BeginSample("memSmallConstraints sync");
            ctx.SyncFrom(memSmallConstraints, other.memSmallConstraints);
            Profiler.EndSample();

            Profiler.BeginSample("native stage 0");
            ; (this as ISyncStageReceiver).OnEnterStage(0, another, ctx);
            Profiler.EndSample();

            Profiler.BeginSample("active list bodies");
            ctx.SyncFrom(bodies, other.bodies);
            Profiler.EndSample();
            Profiler.BeginSample("active list shapes");
            ctx.SyncFrom(shapes, other.shapes);
            Profiler.EndSample();

            Profiler.BeginSample("dict sync");
            if (ctx.GetOrCreate(arbiters, other.arbiters, out var _arbiters))
                ctx.SyncFromExtra(_arbiters, other.arbiters);
            Profiler.EndSample();

            Profiler.BeginSample("brokenArbiters");
            brokenArbiters.Clear();
            for (int i = 0; i < other.brokenArbiters.Count; i++)
            {
                brokenArbiters.Add(ctx.SyncFromExtra(other.brokenArbiters[i], CONTACT_DATA));
            }
            Profiler.EndSample();
            this.NullBody = ctx.SyncFrom(NullBody, other.NullBody);

            ctx.SyncFrom(islands, other.islands);
            this._idCounter = other._idCounter;
            Profiler.BeginSample("DynamicTree");
            ctx.SyncFrom(DynamicTree, other.DynamicTree);
            Profiler.EndSample();
            this.gravity = other.gravity;
            ; (this as ISyncStageReceiver).OnEnterStage(1, another, ctx);

            this.stepper = other.stepper;
            //this.step_dt = other.step_dt;
            //this.substeps = other.substeps;
            //this.substep_dt = other.substep_dt;

        }
    }
}
