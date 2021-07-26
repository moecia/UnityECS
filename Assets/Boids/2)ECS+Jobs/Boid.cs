using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

namespace Boid.ECS
{
    [GenerateAuthoringComponent]
    public struct Boid : IComponentData
    {
        public float CellRadius;
        public float SeparationWeight;
        public float AlighmentWeight;
        public float CohesionWeight;
        public float MoveSpeed;
        public float AvoidWallsWeight;
        public float AvoidWallTurnDistance;
    }
}
