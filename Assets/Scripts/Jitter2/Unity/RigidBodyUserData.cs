using Jitter2.Sync;

namespace Jitter2
{
    public class RigidBodyUserData : ISync
    {
        public int Layer { get; set; }
        public ISync CreateSimilar(SyncContext ctx) => new RigidBodyUserData();

        public void SyncFrom(ISync another, SyncContext ctx)
        {
            var other = another as RigidBodyUserData;
            
            Layer = other.Layer;
        }
    }
}