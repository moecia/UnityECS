using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;

namespace UnitControl
{
    public class UnityControlSystem : SystemBase
    {
        private float3 startPosition;

        protected override void OnUpdate()
        {
            if (Input.GetMouseButtonDown(0))
            {
                SceneController.Instance?.SelectionArea.gameObject.SetActive(true);
                startPosition = MousePositionUtils.MouseToTerrainPositionECS();
                SceneController.Instance?.SetSelectionAreaPosition(startPosition);
            }

            if (Input.GetMouseButton(0))
            {
                var selectionAreaSize = MousePositionUtils.MouseToTerrainPositionECS() - startPosition;
                SceneController.Instance?.SetSelectionAreaScale(selectionAreaSize);
            }

            if (Input.GetMouseButtonUp(0))
            {
                SceneController.Instance?.SelectionArea.gameObject.SetActive(false);

                var endPosition = MousePositionUtils.MouseToTerrainPositionECS();
                var lowerLeftPosition = new float3(math.min(startPosition.x, endPosition.x),
                                        0,
                                        math.min(startPosition.z, endPosition.z));
                var upperRightPosition = new float3(math.max(startPosition.x, endPosition.x),
                                        0,
                                        math.max(startPosition.z, endPosition.z));

                // Implement click select
                var selectionMinSize = 2f;
                var singleSelect = false;
                var selectEntitiesCount = 0;
                var selectionAreaSize = math.distance(lowerLeftPosition, upperRightPosition);
                if (selectionAreaSize < selectionMinSize)
                {
                    lowerLeftPosition += new float3(-1, 0, -1) * (selectionMinSize - selectionAreaSize) *.5f;
                    upperRightPosition += new float3(1, 0, 1) * (selectionMinSize - selectionAreaSize) *.5f;
                    singleSelect = true;
                }

                var ecb = new EntityCommandBuffer(Allocator.TempJob);
                
                // Deselect all selected entities
                Entities.WithAll<UnitSelectedComponent>().ForEach((Entity entity) =>
                {
                    ecb.RemoveComponent<UnitSelectedComponent>(entity);
                }).Run();
                // Hide all select circles
                Entities.WithAll<SelectCircleComponent>().ForEach((Entity entity) =>
                {
                    ecb.RemoveComponent<RenderBounds>(entity);
                    ecb.RemoveComponent<WorldRenderBounds>(entity);
                }).Run();

                Entities
                    .WithEntityQueryOptions(EntityQueryOptions.IncludeDisabled)
                    .ForEach((Entity entity, ref LocalToWorld localToWorld) =>
                    {
                        if (singleSelect == false || selectEntitiesCount < 2)
                        {
                            var pos = localToWorld.Value.c3;
                            if (pos.x >= lowerLeftPosition.x && pos.x <= upperRightPosition.x
                                && pos.z >= lowerLeftPosition.z && pos.z <= upperRightPosition.z)
                            {
                                ecb.AddComponent<UnitSelectedComponent>(entity);
                                selectEntitiesCount++;
                            }
                        }
                    }).Run();

                ecb.Playback(EntityManager);
                ecb.Dispose();
            }

            if (Input.GetMouseButtonDown(1))
            {
                var mousePosition = MousePositionUtils.MouseToTerrainPositionECS();
                var selectedEntity = GetEntityQuery(ComponentType.ReadOnly<UnitSelectedComponent>());
                int count = selectedEntity.CalculateEntityCount() / 2 + 1;
                var movePositionList = GetPositionListAround(mousePosition, 1f, count);
                int positionIndex = 0;
                Entities
                   .WithAll<UnitSelectedComponent>()
                   .ForEach((Entity entity, ref MoveComponent moveComp, in Translation translation) =>
                   {
                       moveComp.TargetPosition = new float3(movePositionList[positionIndex].x,
                           translation.Value.y,
                           movePositionList[positionIndex].z);
                       positionIndex++;
                       moveComp.IsMoving = true;
                   }).Run();
                movePositionList.Dispose();
            }
        }

        private NativeArray<float3> GetPositionListAround(float3 startPosition, float distance, int positionCount)
        {
            var positionList = new NativeArray<float3>(positionCount, Allocator.Temp);
            positionList[0] = startPosition;
            for (int i = 1; i < positionCount; i++)
            {
                int angle = i * (360 / positionCount);
                var dir = ApplyRotationToVector(new float3(0, 0, 1), angle);
                var position = startPosition + dir * distance;
                positionList[i] = position;
            }
            return positionList;
        }

        private float3 ApplyRotationToVector(float3 vec, float angle)
        {
            return Quaternion.Euler(0, angle, 0) * vec;
        }
    }
}
