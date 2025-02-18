﻿#if (OBI_BURST && OBI_MATHEMATICS && OBI_COLLECTIONS)
using Unity.Collections;
using Unity.Mathematics;


namespace Obi
{
    public struct BurstDistanceField : BurstLocalOptimization.IDistanceFunction, IBurstCollider
    {
        public BurstColliderShape shape;
        public BurstAffineTransform transform;
        public float dt;
        public float collisionMargin;

        public NativeArray<DistanceFieldHeader> distanceFieldHeaders;
        public NativeArray<BurstDFNode> dfNodes;

        public void Evaluate(float4 point, ref BurstLocalOptimization.SurfacePoint projectedPoint)
        {
            point = transform.InverseTransformPoint(point);

            if (shape.is2D != 0)
                point[2] = 0;

            var header = distanceFieldHeaders[shape.dataIndex];
            float4 sample = DFTraverse(point, 0, in header, in dfNodes);
            float4 normal = new float4(math.normalize(sample.xyz), 0);

            projectedPoint.point = transform.TransformPoint(point - normal * (sample[3] - shape.contactOffset));
            projectedPoint.normal = transform.TransformDirection(normal);
        }

        public void Contacts(int colliderIndex,

                              NativeArray<float4> positions,
                              NativeArray<float4> velocities,
                              NativeArray<float4> radii,

                              NativeArray<int> simplices,
                              in BurstAabb simplexBounds,
                              int simplexIndex,
                              int simplexStart,
                              int simplexSize,

                              NativeQueue<BurstContact>.ParallelWriter contacts,
                              int optimizationIterations,
                              float optimizationTolerance)
        {
            if (shape.dataIndex < 0) return;

            var co = new BurstContact() { bodyA = simplexIndex, bodyB = colliderIndex };
            float4 simplexBary = BurstMath.BarycenterForSimplexOfSize(simplexSize);

            var colliderPoint = BurstLocalOptimization.Optimize<BurstDistanceField>(ref this, positions, radii, simplices, simplexStart, simplexSize,
                                                                ref simplexBary, out float4 simplexPoint, optimizationIterations, optimizationTolerance);

            co.pointB = colliderPoint.point;
            co.normal = colliderPoint.normal;
            co.pointA = simplexBary;

            float4 velocity = float4.zero;
            float simplexRadius = 0;
            for (int j = 0; j < simplexSize; ++j)
            {
                int particleIndex = simplices[simplexStart + j];
                simplexRadius += radii[particleIndex].x * simplexBary[j];
                velocity += velocities[particleIndex] * simplexBary[j];
            }

            float dAB = math.dot(simplexPoint - colliderPoint.point, colliderPoint.normal);
            float vel = math.dot(velocity - 0, colliderPoint.normal); // TODO: consider rigidbody velocity here.

            if (vel * dt + dAB <= simplexRadius + shape.contactOffset + collisionMargin)
                contacts.Enqueue(co);
        }

        private static float4 DFTraverse(float4 particlePosition,
                                         int nodeIndex,
                                         in DistanceFieldHeader header,
                                         in NativeArray<BurstDFNode> dfNodes)
        {
            var node = dfNodes[header.firstNode + nodeIndex];

            // if the child node exists, recurse down the df octree:
            if (node.firstChild >= 0)
            {
                int octant = node.GetOctant(particlePosition);
                return DFTraverse(particlePosition, node.firstChild + octant, in header, in dfNodes);
            }
            else
            {
                return node.SampleWithGradient(particlePosition);
            }
        }

    }

}
#endif