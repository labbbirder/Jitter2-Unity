
using static Jitter2.Sync.HandleIndex;
using Jitter2.Sync;

namespace Jitter2.Collision.Shapes
{
    partial class SphereShape : ISync, ISyncStageReceiver
    {
        SphereShape() {}
        
        public override ISync CreateSimilar(SyncContext ctx) => new SphereShape();
                
        public override void SyncFrom(ISync another, SyncContext ctx){
            base.SyncFrom(another, ctx);
            var other = another as SphereShape;
            
            ;(this as ISyncStageReceiver).OnEnterStage(0, another, ctx);
                        
            this.radius = other.radius;
        }
    }
}
