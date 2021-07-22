using Unity.Entities;

namespace EcsSample
{
    [GenerateAuthoringComponent]
    public struct MoveSpeedComponent : IComponentData
    {
        public float MoveSpeed;
    }
}