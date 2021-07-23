using Unity.Entities;

namespace PrefabToEntity
{
    public struct PrefabEntityComponent : IComponentData
    {
        public Entity PrefabEntity;
        public int Count;
    }
}