using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using UnitControl;

namespace PrefabToEntity
{
    public class EntitiesSpawnerSystem : SystemBase
    {
        [BurstCompile]
        private struct SetPosition : IJobParallelFor
        {
            [NativeDisableContainerSafetyRestriction]
            [NativeDisableParallelForRestriction]
            public ComponentDataFromEntity<Translation> TranslationFromEntity;
            public NativeArray<Entity> Entites;
            [ReadOnly]
            public SpawnAxis SpawnAxis;
            public void Execute(int index)
            {
                var entity = Entites[index];
                var random = new Random(((uint)(entity.Index + index + 1) * 0x9F6ABC1));
                var translation = new Translation { };
                if (SpawnAxis == SpawnAxis.Y)
                {
                    translation = new Translation { Value = new float3(random.NextFloat(-5f, 5f), random.NextFloat(-5f, 5f), 0) };
                }
                else if (SpawnAxis == SpawnAxis.Z)
                {
                    translation = new Translation { Value = new float3(random.NextFloat(-10f, 10f), 1.1f, random.NextFloat(-10f, 10f)) };
                }
                TranslationFromEntity[entity] = translation;
            }
        }
  
        protected override void OnUpdate()
        {
            // What is structural changes: https://docs.unity3d.com/Packages/com.unity.entities@0.4/manual/sync_points.html
            Entities
                .WithStructuralChanges()
                .WithName("EntitiesSpawnerSystem")
                .ForEach((Entity entity, in PrefabEntityComponent prefabEntityComponent) =>
                {
                    var entities = new NativeArray<Entity>(prefabEntityComponent.Count, Allocator.TempJob);
                    EntityManager.Instantiate(prefabEntityComponent.PrefabEntity, entities);

                    // Nested job
                    //Entities.ForEach((ref Translation translation) =>
                    //{
                    //    translation.Value.x = random.NextFloat(-5f, 5f);
                    //    translation.Value.y = random.NextFloat(-5f, 5f);
                    //}).WithoutBurst().Run();

                    // Use Job
                    var translationFromEntity = GetComponentDataFromEntity<Translation>();
                    var setBoidPositionJob = new SetPosition
                    {
                        TranslationFromEntity = translationFromEntity,
                        Entites = entities,
                        SpawnAxis = prefabEntityComponent.SpawnAxis
                    };
                    Dependency = setBoidPositionJob.Schedule(prefabEntityComponent.Count, 64, Dependency);
                    Dependency = entities.Dispose(Dependency);

                    EntityManager.DestroyEntity(entity);
                    //entities.Dispose();
                }).Run();

            Entities
                .WithStructuralChanges()
                .WithAll<SelectCircleInitializerComponent>()
                .ForEach((Entity entity) => 
                {
                    EntityManager.RemoveComponent<SelectCircleInitializerComponent>(entity);
                    EntityManager.RemoveComponent<RenderBounds>(entity);
                    EntityManager.RemoveComponent<WorldRenderBounds>(entity);
                }).Run();
        }
    }
}