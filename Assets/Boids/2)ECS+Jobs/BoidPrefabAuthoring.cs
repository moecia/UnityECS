using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

namespace Boid.ECS
{
    public class BoidPrefabAuthoring : MonoBehaviour, IDeclareReferencedPrefabs, IConvertGameObjectToEntity
    {
        public GameObject BoidPrefab;
        public int Count;
        public int CageSize;

        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            var spawnerData = new BoidPrefabComponent
            {
                BoidPrefab = conversionSystem.GetPrimaryEntity(BoidPrefab),
                Count = Count,
                CageSize = CageSize
            };
            dstManager.AddComponentData(entity, spawnerData);
        }

        public void DeclareReferencedPrefabs(List<GameObject> referencedPrefabs)
        {
            referencedPrefabs.Add(BoidPrefab);
        }
    }
}