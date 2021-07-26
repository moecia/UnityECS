using Unity.Entities;

namespace Boid.ECS
{
    [GenerateAuthoringComponent]
    public struct BoidPrefabComponent : IComponentData
    {
        public Entity BoidPrefab;
        public int Count;
        public int CageSize;
    }
}