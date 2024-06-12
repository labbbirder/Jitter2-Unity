
using static Jitter2.Sync.HandleIndex;
using Jitter2.Sync;

namespace Jitter2.Collision.Shapes
{
    partial class CapsuleShape : ISync, ISyncStageReceiver
    {
        CapsuleShape() {}
        
        public override ISync CreateSimilar(SyncContext ctx) => new CapsuleShape();
                
        public override void SyncFrom(ISync another, SyncContext ctx){
            base.SyncFrom(another, ctx);
            var other = another as CapsuleShape;
            
            ;(this as ISyncStageReceiver).OnEnterStage(0, another, ctx);
                        
            this.radius = other.radius;
            this.halfLength = other.halfLength;
        }
        
    }
}
