using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;
using Random = UnityEngine.Random;

namespace EcsSample
{
    public class TestController : MonoBehaviour
    {
        [SerializeField] private Mesh mesh;
        [SerializeField] private Material material;
        [SerializeField] private int count = 1000;
        void Start()
        {

            var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
            // Add entity achetype
            var entityArchetype = entityManager.CreateArchetype(
                typeof(MoveSpeedComponent),
                typeof(Translation),
                typeof(RenderMesh),
                typeof(LocalToWorld),
                typeof(RenderBounds));
            var entityArray = new NativeArray<Entity>(count, Allocator.Temp);
            // Instantiate entities
            entityManager.CreateEntity(entityArchetype, entityArray);
            for (int i = 0; i < entityArray.Length; i++)
            {
                var entity = entityArray[i];
                entityManager.SetComponentData(entity, new MoveSpeedComponent { MoveSpeed = Random.Range(1f, 2f) });
                entityManager.SetComponentData(entity, new Translation { Value = new float3(Random.Range(-8f, 8f), Random.Range(-5f, 5f), 0) });

                entityManager.SetSharedComponentData(entity, new RenderMesh
                {
                    mesh = this.mesh,
                    material = this.material
                });
            }

            entityArray.Dispose();
        }
    }
}
