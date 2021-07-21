using Unity.Entities;
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
                if (translation.Value.y > 5f || translation.Value.y < -5f)
                {
                    moveSpeedComponent.MoveSpeed *= -1;
                }
            }).ScheduleParallel();
        }
    }
}
