using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Boid.ECS
{
    public class BoidSystem : SystemBase
    {
        private struct EntityWithLocalToWorld
        {
            public Entity entity;
            public LocalToWorld localToWorld;
        }

        protected override void OnUpdate()
        {
            var dt = Time.DeltaTime;
            var boidsQuery = GetEntityQuery(ComponentType.ReadOnly<Boid>(), ComponentType.ReadOnly<LocalToWorld>());
            var entityArray = boidsQuery.ToEntityArray(Allocator.TempJob);
            var localToWorldArray = boidsQuery.ToComponentDataArray<LocalToWorld>(Allocator.TempJob);

            var jobHandle = Entities.
                ForEach((Entity entity, ref Translation translation, ref Rotation rotation, in LocalToWorld localToWorld, in Boid boid) =>
            {
                var boidPosition = localToWorld.Position;

                var seperationSum = float3.zero;
                var positionSum = float3.zero;
                var headingSum = float3.zero;

                int boidsNearby = 0;

                for (int i = 0; i < entityArray.Length; i++)
                {
                    if (entity != entityArray[i])
                    {
                        var otherBoidsPosition = localToWorldArray[i].Position;
                        var distToOtherBoid = math.length(boidPosition - otherBoidsPosition);

                        if (distToOtherBoid < boid.CellRadius)
                        {
                            seperationSum += -(otherBoidsPosition - boidPosition) * (1f / math.max(distToOtherBoid, .0001f));
                            positionSum += otherBoidsPosition;
                            headingSum += localToWorldArray[i].Forward;
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
                var force = separationForce * boid.SeparationWeight +
                        cohesionForce * boid.CohesionWeight +
                        alignmentForce * boid.AlighmentWeight;
                if (math.min(math.min((500f / 2f) - math.abs(boidPosition.x),
                      (500f / 2f) - math.abs(boidPosition.y)),
                      (500f / 2f) - math.abs(boidPosition.z)) < boid.AvoidWallTurnDistance)
                {
                    force += math.normalize(boidPosition) * boid.AvoidWallsWeight * -1f;
                }

                var velocity = localToWorld.Forward * boid.MoveSpeed;
                velocity += force * dt;
                velocity = math.normalize(velocity) * boid.MoveSpeed;

                translation.Value = localToWorld.Position + velocity * dt;
                rotation.Value = quaternion.LookRotationSafe(velocity, localToWorld.Up);
            }).Schedule(Dependency);

            Dependency = jobHandle;
            entityArray.Dispose(Dependency);
            localToWorldArray.Dispose(Dependency);
        }
    }
}