using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

namespace Boid.ECS
{
    public class BoidSpawnSystem : SystemBase
    {
        [BurstCompile]
        private struct SetBoidPosition : IJobParallelFor
        {
            [NativeDisableContainerSafetyRestriction]
            [NativeDisableParallelForRestriction]
            public ComponentDataFromEntity<Translation> TranslationFromEntity;
            [NativeDisableContainerSafetyRestriction]
            [NativeDisableParallelForRestriction]
            public ComponentDataFromEntity<Rotation> RotationFromEntity;

            public NativeArray<Entity> Entites;
            public int CageSize;

            public void Execute(int index)
            {
                var entity = Entites[index];
                var random = new Random(((uint)(entity.Index + index + 1) * 0x9F6ABC1));
                var translation = new Translation
                {
                    Value = new float3(random.NextFloat(-CageSize / 2, CageSize / 2),
                                       random.NextFloat(-CageSize / 2, CageSize / 2),
                                       random.NextFloat(-CageSize / 2, CageSize / 2))
                };
                var rotation = new Rotation { Value = quaternion.LookRotationSafe(math.normalizesafe(random.NextFloat3()), math.up()) };
                TranslationFromEntity[entity] = translation;
                RotationFromEntity[entity] = rotation;
            }
        }

        protected override void OnUpdate()
        {
            Entities
                .WithStructuralChanges()
                .WithName("EntitiesSpawnerSystem")
                .ForEach((Entity entity, in BoidPrefabComponent boidPrefabEntity) =>
                {
                    var entities = new NativeArray<Entity>(boidPrefabEntity.Count, Allocator.TempJob);
                    EntityManager.Instantiate(boidPrefabEntity.BoidPrefab, entities);

                    // Use Job
                    var translationFromEntity = GetComponentDataFromEntity<Translation>();
                    var rotationFromEntity = GetComponentDataFromEntity<Rotation>();

                    var setBoidPositionJob = new SetBoidPosition
                    {
                        TranslationFromEntity = translationFromEntity,
                        RotationFromEntity = rotationFromEntity,
                        Entites = entities,
                        CageSize = boidPrefabEntity.CageSize
                    };
                    Dependency = setBoidPositionJob.Schedule(boidPrefabEntity.Count, 64, Dependency);
                    Dependency = entities.Dispose(Dependency);

                    EntityManager.DestroyEntity(entity);
                }).Run();
        }
    }
}