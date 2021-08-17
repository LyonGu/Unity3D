using System;
using Unity.Entities;
using UnityEngine;
using Unity.Transforms;

namespace Samples.Boids
{
    [Serializable]
    [WriteGroup(typeof(LocalToWorld))] //如果有个system关心了Boid组件，并且标记了writeGroupFilterOption，就会剔除LocalToWorld的Entity
    public struct Boid : ISharedComponentData
    {
        public float CellRadius;
        public float SeparationWeight;
        public float AlignmentWeight;
        public float TargetWeight;
        public float ObstacleAversionDistance;
        public float MoveSpeed;
    }
}
