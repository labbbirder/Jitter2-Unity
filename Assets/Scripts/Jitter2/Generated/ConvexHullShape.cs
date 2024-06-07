
using static Jitter2.Sync.HandleIndex;
using Jitter2.Sync;

namespace Jitter2.Collision.Shapes
{
    partial class ConvexHullShape : ISync, ISyncStageReceiver
    {
        
        
        public override ISync CreateSimilar(SyncContext ctx) => new ConvexHullShape();
                
        public override void SyncFrom(ISync another, SyncContext ctx){
            base.SyncFrom(another, ctx);
            var other = another as ConvexHullShape;
            
            ;(this as ISyncStageReceiver).OnEnterStage(0, another, ctx);
                        
            if (ctx.GetOrCreate(vertices, other.vertices,out var _vertices))
                this.vertices = ctx.SyncUnmanagedArray(_vertices, other.vertices,false);
            if (ctx.GetOrCreate(indices, other.indices,out var _indices))
                this.indices = ctx.SyncUnmanagedArray(_indices, other.indices,false);
            if (ctx.GetOrCreate(neighborList, other.neighborList,out var _neighborList))
                this.neighborList = ctx.SyncFromExtraUnmanaged(_neighborList, other.neighborList);
            this.shifted = other.shifted;
            this.initBox = other.initBox;
        }
    }
}
