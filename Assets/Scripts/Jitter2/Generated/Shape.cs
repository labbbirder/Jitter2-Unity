
using static Jitter2.Sync.HandleIndex;
using Jitter2.Sync;

namespace Jitter2.Collision.Shapes
{
    partial class Shape : ISync, ISyncStageReceiver
    {
        
        
        public virtual ISync CreateSimilar(SyncContext ctx) => default;
                
        public virtual void SyncFrom(ISync another, SyncContext ctx){
            
            var other = another as Shape;
            
            ;(this as ISyncStageReceiver).OnEnterStage(0, another, ctx);
                        
            this.ShapeId = other.ShapeId;
            this.WorldBoundingBox = other.WorldBoundingBox;
            this.Inertia = other.Inertia;
            this.GeometricCenter = other.GeometricCenter;
            this.Mass = other.Mass;
            (this as IDynamicTreeProxy).NodePtr = (other as IDynamicTreeProxy).NodePtr;
        }
        
        public virtual void ReferencePhaseSyncFrom(ISync another, SyncContext ctx){
            
            var other = another as Shape;
            
            this.RigidBody = ctx.SyncFrom(RigidBody, other.RigidBody);
        }
                    
    }
}
