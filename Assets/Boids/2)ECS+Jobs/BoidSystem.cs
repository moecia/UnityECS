using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

namespace Boid.ECS
{
    public class BoidSystem : SystemBase
    {
        [BurstCompile]
        private struct CalcutateForce : IJobParallelFor
        {
            //[DeallocateOnJobCompletion]
            [NativeDisableParallelForRestriction]
            [ReadOnly]
            public NativeArray<EntityWithLocalToWorld> BoidsArray;
            [ReadOnly]
            public float deltaTime;
            [ReadOnly]
            public Boid Boid;

            [NativeDisableContainerSafetyRestriction]
            [WriteOnly]
            public ComponentDataFromEntity<Translation> TranslationFromEntity;
            [NativeDisableContainerSafetyRestriction]
            [WriteOnly]
            public ComponentDataFromEntity<Rotation> RotationFromEntity;

            public void Execute(int index)
            {
                var entity = BoidsArray[index].Entity;
                var boidPosition = BoidsArray[index].LocalToWorld.Position;

                var seperationSum = float3.zero;
                var positionSum = float3.zero;
                var headingSum = float3.zero;

                int boidsNearby = 0;

                for (int i = 0; i < BoidsArray.Length; i++)
                {
                    if (entity != BoidsArray[i].Entity)
                    {
                        var otherBoidsPosition = BoidsArray[i].LocalToWorld.Position;
                        var distToOtherBoid = math.length(boidPosition - otherBoidsPosition);

                        if (distToOtherBoid < Boid.CellRadius)
                        {
                            seperationSum += -(otherBoidsPosition - boidPosition) * (1f / math.max(distToOtherBoid, .0001f));
                            positionSum += otherBoidsPosition;
                            headingSum += BoidsArray[i].LocalToWorld.Forward;
                            boidsNearby++;
                        }
                    }
                }

                var separationForce = float3.zero;
                var cohesionForce = float3.zero;
                var alignmentForce = float3.zero;
                var avoidWallsForce = float3.zero;
                if (boidsNearby > 0)
                {
                    separationForce = seperationSum / boidsNearby;
                    cohesionForce = (positionSum / boidsNearby) - boidPosition;
                    alignmentForce = headingSum / boidsNearby;
                }
                if (math.min(math.min((500 / 2f) - math.abs(boidPosition.x),
                                      (500 / 2f) - math.abs(boidPosition.y)),
                                      (500 / 2f) - math.abs(boidPosition.z)) < Boid.AvoidWallTurnDistance)
                {
                    avoidWallsForce = -math.normalize(boidPosition);
                }

                var force = separationForce * Boid.SeparationWeight +
                        cohesionForce * Boid.CohesionWeight +
                        alignmentForce * Boid.AlighmentWeight +
                        avoidWallsForce * Boid.AvoidWallsWeight;
                var velocity = BoidsArray[index].LocalToWorld.Forward * Boid.MoveSpeed;
                velocity += force * deltaTime;
                velocity = math.normalize(velocity) * Boid.MoveSpeed;


                var translation = new Translation { Value = BoidsArray[index].LocalToWorld.Position + velocity * deltaTime };
                var rotation = new Rotation { Value = quaternion.LookRotationSafe(velocity, BoidsArray[index].LocalToWorld.Up) };
                TranslationFromEntity[entity] = translation;
                RotationFromEntity[entity] = rotation;
            }
        }

        private struct EntityWithLocalToWorld
        {
            public Entity Entity;
            public LocalToWorld LocalToWorld;
        }


        protected override void OnUpdate()
        {
            var dt = Time.DeltaTime;
            var boidsQuery = GetEntityQuery(ComponentType.ReadOnly<Boid>(), ComponentType.ReadOnly<LocalToWorld>());
            var entitiesArray = boidsQuery.ToEntityArray(Allocator.TempJob);
            var localToWorldArray = boidsQuery.ToComponentDataArray<LocalToWorld>(Allocator.TempJob);
            var boidsArray = new NativeArray<EntityWithLocalToWorld>(entitiesArray.Length, Allocator.TempJob);
            for (int i = 0; i < entitiesArray.Length; i++)
            {
                boidsArray[i] = new EntityWithLocalToWorld
                {
                    Entity = entitiesArray[i],
                    LocalToWorld = localToWorldArray[i]
                };
            }
            entitiesArray.Dispose();
            localToWorldArray.Dispose();

            Entities
                .WithStructuralChanges()
                .WithName("BoidSystem")
                .ForEach((Boid boid) =>
                {

                    var translationFromEntity = GetComponentDataFromEntity<Translation>();
                    var rotationFromEntity = GetComponentDataFromEntity<Rotation>();

                    var calcutateForceJob = new CalcutateForce
                    {
                        TranslationFromEntity = translationFromEntity,
                        RotationFromEntity = rotationFromEntity,
                        Boid = boid,
                        BoidsArray = boidsArray,
                        deltaTime = dt
                    };
                    calcutateForceJob.Schedule(boidsArray.Length, 64, Dependency).Complete();
                    if (boidsArray.Length > 0)
                    {
                        boidsArray.Dispose();
                    }
                }).Run();

        }
    }
}