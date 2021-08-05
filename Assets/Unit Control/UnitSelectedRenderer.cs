using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;

namespace UnitControl
{
    public class UnitSelectedRenderer : SystemBase
    {
        protected override void OnUpdate()
        {
            var ecb = new EntityCommandBuffer(Allocator.TempJob);

            Entities
                .WithAll<SelectCircleComponent>()
                .ForEach((Entity entity, in Parent parent) =>
                {
                    if (HasComponent<UnitSelectedComponent>(parent.Value))
                    {
                        ecb.AddComponent<WorldRenderBounds>(entity);
                        ecb.AddComponent<RenderBounds>(entity);
                    }
                }).Run();

            ecb.Playback(EntityManager);
            ecb.Dispose();
        }
    }
}
