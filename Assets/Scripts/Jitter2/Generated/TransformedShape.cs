
using static Jitter2.Sync.HandleIndex;
using Jitter2.Sync;

namespace Jitter2.Collision.Shapes
{
    partial class TransformedShape : ISync, ISyncStageReceiver
    {
        TransformedShape() {}
        
        public override ISync CreateSimilar(SyncContext ctx) => new TransformedShape();
                
        public override void SyncFrom(ISync another, SyncContext ctx){
            base.SyncFrom(another, ctx);
            var other = another as TransformedShape;
            
            ;(this as ISyncStageReceiver).OnEnterStage(0, another, ctx);
                        
            this.OriginalShape = ctx.SyncFrom(OriginalShape, other.OriginalShape);
            this.translation = other.translation;
            this.transformation = other.transformation;
            this.type = other.type;
        }
    }
}
