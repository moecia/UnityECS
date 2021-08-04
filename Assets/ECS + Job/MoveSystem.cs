using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace EcsSample
{
    public class MoveSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            var dt = Time.DeltaTime;
            Entities.ForEach((ref Translation translation, ref MoveSpeedComponent moveSpeedComponent) =>
            {
                translation.Value.y += moveSpeedComponent.MoveSpeed * dt;
                if (translation.Value.y > 5f)
                {
                    moveSpeedComponent.MoveSpeed = -math.abs(moveSpeedComponent.MoveSpeed);
                }
                else if (translation.Value.y < -5f)
                {
                    moveSpeedComponent.MoveSpeed = math.abs(moveSpeedComponent.MoveSpeed);
                }
            }).ScheduleParallel();
        }
    }
}
