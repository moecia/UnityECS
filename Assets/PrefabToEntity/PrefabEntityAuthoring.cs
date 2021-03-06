using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

namespace PrefabToEntity
{
    public class PrefabEntityAuthoring : MonoBehaviour, IDeclareReferencedPrefabs, IConvertGameObjectToEntity
    {
        public GameObject Prefab;
        public int Count;
        public SpawnAxis SpawnAxis;

        public void DeclareReferencedPrefabs(List<GameObject> referencedPrefabs)
        {
            referencedPrefabs.Add(Prefab);
        }

        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            var spawnerData = new PrefabEntityComponent
            {
                PrefabEntity = conversionSystem.GetPrimaryEntity(Prefab),
                Count = Count,
                SpawnAxis = SpawnAxis
            };
            dstManager.AddComponentData(entity, spawnerData);
        }
    }
}
