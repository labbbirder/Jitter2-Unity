
using static Jitter2.Sync.HandleIndex;
using Jitter2.Sync;

namespace Jitter2.Collision.Shapes
{
    partial class PointCloudShape : ISync, ISyncStageReceiver
    {
        
        
        public override ISync CreateSimilar(SyncContext ctx) => new PointCloudShape();
                
        public override void SyncFrom(ISync another, SyncContext ctx){
            base.SyncFrom(another, ctx);
            var other = another as PointCloudShape;
            
            ;(this as ISyncStageReceiver).OnEnterStage(0, another, ctx);
                        
            if (ctx.GetOrCreate(vertices, other.vertices,out var _vertices))
                this.vertices = ctx.SyncFromExtraUnmanaged(_vertices, other.vertices);
            this.shifted = other.shifted;
        }
        
    }
}
