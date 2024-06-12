
using static Jitter2.Sync.HandleIndex;
using Jitter2.Sync;

namespace Jitter2.Collision.Shapes
{
    partial class CylinderShape : ISync, ISyncStageReceiver
    {
        CylinderShape() {}
        
        public override ISync CreateSimilar(SyncContext ctx) => new CylinderShape();
                
        public override void SyncFrom(ISync another, SyncContext ctx){
            base.SyncFrom(another, ctx);
            var other = another as CylinderShape;
            
            ;(this as ISyncStageReceiver).OnEnterStage(0, another, ctx);
                        
            this.radius = other.radius;
            this.height = other.height;
        }
        
    }
}
