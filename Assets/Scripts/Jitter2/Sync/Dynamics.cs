
using System;
using System.Collections.Generic;
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
            }
        }
    }
}
