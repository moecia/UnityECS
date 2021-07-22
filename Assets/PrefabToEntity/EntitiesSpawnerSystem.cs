using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

public class EntitiesSpawnerSystem : SystemBase
{
    private float spawnTimer;
    private Random random;

    BeginInitializationEntityCommandBufferSystem entityCommandBufferSystem;

    protected override void OnCreate()
    {
        random = new Random(123);
        entityCommandBufferSystem = World.GetOrCreateSystem<BeginInitializationEntityCommandBufferSystem>();
    }

    protected override void OnUpdate()
    {
        var commandBuffer = entityCommandBufferSystem.CreateCommandBuffer().AsParallelWriter();
        spawnTimer -= Time.DeltaTime;

        if (spawnTimer <= 0f)
        {
            //Instantiate prefab by Job System
            var x = random.NextFloat(-5f, 5f);
            var y = random.NextFloat(-5f, 5f);
            spawnTimer = .001f;
            Entities
                .WithName("EntitiesSpawnerSystem")
                .WithBurst(FloatMode.Default, FloatPrecision.Standard, true)
                .ForEach((Entity entity, int entityInQueryIndex, in PrefabEntityComponent prefabEntityComponent, in LocalToWorld location) =>
            {
                var spawnedEntity = commandBuffer.Instantiate(entityInQueryIndex, prefabEntityComponent.PrefabEntity);

                var position = math.transform(location.Value, new float3(x, y, 0));
                commandBuffer.SetComponent(entityInQueryIndex, spawnedEntity, new Translation { Value = position });
            }).ScheduleParallel();

            //Instantiate prefab from BlobAssetStore
            //spawnTimer = .5f;
            //var spawnedEntity = EntityManager.Instantiate(PrefabEntites.PrefabEntity);
            //EntityManager.SetComponentData(spawnedEntity, new Translation { Value = new float3(random.NextFloat(-5f, 5f), random.NextFloat(-5f, 5f), 0) });
        }
    }
}
