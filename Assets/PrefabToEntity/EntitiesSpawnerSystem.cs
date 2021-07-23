using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

namespace PrefabToEntity
{
    public class EntitiesSpawnerSystem : SystemBase
    {
        [BurstCompile]
        private struct SetBoidPosition : IJobParallelFor
        {
            [NativeDisableContainerSafetyRestriction]
            [NativeDisableParallelForRestriction]
            public ComponentDataFromEntity<Translation> TranslationFromEntity;
            public NativeArray<Entity> Entites;

            public void Execute(int index)
            {
                var entity = Entites[index];
                var random = new Random(((uint)(entity.Index + index + 1) * 0x9F6ABC1));
                var translation = new Translation { Value = new float3(random.NextFloat(-5f, 5f), random.NextFloat(-5f, 5f), 0) };
                TranslationFromEntity[entity] = translation;
            }
        }

        protected override void OnUpdate()
        { 
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
                    var setBoidPositionJob = new SetBoidPosition
                    {
                        TranslationFromEntity = translationFromEntity,
                        Entites = entities
                    };
                    Dependency = setBoidPositionJob.Schedule(prefabEntityComponent.Count, 64, Dependency);
                    Dependency = entities.Dispose(Dependency);

                    EntityManager.DestroyEntity(entity);
                    //entities.Dispose();
                }).Run();
        }
    }
}