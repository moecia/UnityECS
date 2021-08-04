using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace UnitControl
{
    public class UnitMoveSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            var dt = Time.DeltaTime;
            Entities
                .WithAll<MoveComponent>()
                .ForEach((Entity entity, ref MoveComponent moveComp, ref Translation translation) => 
            {
                if (moveComp.IsMoving)
                {
                    float reachedPositionDist = 1f;
                    if (math.distance(translation.Value, moveComp.TargetPosition) > reachedPositionDist)
                    {
                        var moveDir = math.normalize(moveComp.TargetPosition - translation.Value);
                        moveComp.LastMoveDirection = moveDir;
                        translation.Value += moveDir * moveComp.MoveSpeed * dt;
                    }
                    else
                    {
                        moveComp.IsMoving = false;
                    }
                }
            }).Run();
        }
    }
}