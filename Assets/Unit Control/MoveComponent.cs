using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace UnitControl
{
    [GenerateAuthoringComponent]
    public struct MoveComponent : IComponentData 
    {
        [HideInInspector] public bool IsMoving;
        [HideInInspector] public float3 TargetPosition;
        [HideInInspector] public float3 LastMoveDirection;
        public float MoveSpeed;
    }
}