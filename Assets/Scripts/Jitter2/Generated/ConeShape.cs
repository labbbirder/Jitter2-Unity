
using static Jitter2.Sync.HandleIndex;
using Jitter2.Sync;

namespace Jitter2.Collision.Shapes
{
    partial class ConeShape : ISync, ISyncStageReceiver
    {
        ConeShape() {}
        
        public override ISync CreateSimilar(SyncContext ctx) => new ConeShape();
                
        public override void SyncFrom(ISync another, SyncContext ctx){
            base.SyncFrom(another, ctx);
            var other = another as ConeShape;
            
            ;(this as ISyncStageReceiver).OnEnterStage(0, another, ctx);
                        
            this.radius = other.radius;
            this.height = other.height;
        }
        
    }
}
