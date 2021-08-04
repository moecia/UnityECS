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

            var controller = SceneController.Instance;
            Entities
                .WithAll<SelectCircleComponent>()
                .WithAll<UnitSelectedComponent>()
                .ForEach((Entity entity) =>
            {
                ecb.AddComponent<WorldRenderBounds>(entity);
                ecb.AddComponent<RenderBounds>(entity);
            }).Run();

            ecb.Playback(EntityManager);
            ecb.Dispose();
        }
    }
}
