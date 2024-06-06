
using static Jitter2.Sync.HandleIndex;
using Jitter2.Sync;

namespace Jitter2.Collision.Shapes
{
    partial class BoxShape : ISync, ISyncStageReceiver
    {
        BoxShape() {}
        
        public override ISync CreateSimilar(SyncContext ctx) => new BoxShape();
                
        public override void SyncFrom(ISync another, SyncContext ctx){
            base.SyncFrom(another, ctx);
            var other = another as BoxShape;
            
            ;(this as ISyncStageReceiver).OnEnterStage(0, another, ctx);
                        
            this.halfSize = other.halfSize;
        }
    }
}
