/* 
    ------------------- Code Monkey -------------------

    Thank you for downloading this package
    I hope you find it useful in your projects
    If you have any questions let me know
    Cheers!

               unitycodemonkey.com
    --------------------------------------------------
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Collections;
using Unity.Transforms;
using Unity.Rendering;
using Unity.Mathematics;
using Unity.Jobs;
using Unity.Burst;

namespace ECS_AnimationSystem {

    public static class ECS_AnimationSystems {

    }
    
    /*
    public struct Skeleton_Material : IComponentData {

        public TypeEnum materialTypeEnum;

        public enum TypeEnum {
            Marine,
            Zombie
        }
    }*/

    
    // Play the Animation currently stored in Skeleton_PlayAnim
    [UpdateAfter(typeof(Skeleton_Callbacks))]
    public class Skeleton_PlayAnimSystem : JobComponentSystem {

        private struct Job : IJobForEachWithEntity<Skeleton_Data, Skeleton_PlayAnim> {
        
            public EntityCommandBuffer.ParallelWriter entityCommandBuffer;

            public void Execute(Entity entity, int index, ref Skeleton_Data skeletonData, ref Skeleton_PlayAnim skeletonPlayAnim) {
                if (skeletonPlayAnim.forced) {
                    skeletonPlayAnim.forced = false;
                    ECS_Animation.PlayAnimForcedJobs(entity, index, entityCommandBuffer, skeletonPlayAnim.ecsUnitAnimTypeEnum, skeletonPlayAnim.animDir, skeletonPlayAnim.onComplete);
                } else {
                    ECS_Animation.PlayAnimJobs(entity, index, entityCommandBuffer, skeletonData, skeletonPlayAnim.ecsUnitAnimTypeEnum, skeletonPlayAnim.animDir, skeletonPlayAnim.onComplete);
                }
            }

        }
    
        private EndSimulationEntityCommandBufferSystem endSimulationEntityCommandBufferSystem;

        protected override void OnCreate() {
            endSimulationEntityCommandBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps) {
            Job job = new Job {
                entityCommandBuffer = endSimulationEntityCommandBufferSystem.CreateCommandBuffer().AsParallelWriter(),
            };
            JobHandle jobHandle = job.Schedule(this, inputDeps);

            endSimulationEntityCommandBufferSystem.AddJobHandleForProducer(jobHandle);

            return jobHandle;
        }

    }

    // Update the Current Skeleton Frame
    public class Skeleton_UpdaterJob : JobComponentSystem {

        [BurstCompile]
        private struct Job : IJobForEach<Skeleton_Data> {

            public float deltaTime;

            public void Execute(ref Skeleton_Data skeletonData) {
                //skeletonRefreshTimer.refreshTimer -= deltaTime;
                skeletonData.frameTimer -= deltaTime;
                while (skeletonData.frameTimer < 0) {
                    skeletonData.frameTimer += skeletonData.frameRate;
                    skeletonData.currentFrame = skeletonData.currentFrame + 1;
                    if (skeletonData.currentFrame >= skeletonData.frameCount) {
                        skeletonData.currentFrame = 0;
                        skeletonData.loopCount++;
                    }
                }
            }
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps) {
            Job job = new Job {
                deltaTime = Time.DeltaTime
            };
            return job.Schedule(this, inputDeps);
        }

    }

    [UpdateAfter(typeof(Skeleton_UpdaterJob))]
    public class Skeleton_Callbacks : JobComponentSystem {
    
        private struct Job : IJobForEachWithEntity<Skeleton_Data, Skeleton_PlayAnim> {

            public void Execute(Entity entity, int index, ref Skeleton_Data skeletonData, ref Skeleton_PlayAnim skeletonPlayAnim) {
                if (skeletonData.loopCount > 0 && skeletonData.onComplete.hasOnComplete) {
                    skeletonPlayAnim.PlayAnim(skeletonData.onComplete.unitAnimTypeEnum, skeletonData.onComplete.animDir, default);
                }
            }

        }

        protected override JobHandle OnUpdate(JobHandle inputDeps) {
            Job job = new Job {
            };
            return job.Schedule(this, inputDeps);
        }

    }


    // Display the Mesh
    [UpdateAfter(typeof(Skeleton_PlayAnimSystem))]
    public class Skeleton_MeshDisplay : ComponentSystem {

        // Display Mesh
        protected override void OnUpdate() {
            Material marineMaterial = ECS_RTSControls.instance.marineMaterial;
            Quaternion quaternionIdentity = Quaternion.identity;

            EntityQuery skeletonQuery = GetEntityQuery(ComponentType.ReadOnly<Translation>(), ComponentType.ReadOnly<Skeleton_Data>());
            NativeArray<Translation> translationArray = skeletonQuery.ToComponentDataArray<Translation>(Allocator.TempJob);
            NativeArray<Skeleton_Data> skeletonDataArray = skeletonQuery.ToComponentDataArray<Skeleton_Data>(Allocator.TempJob);

            float3 skeletonOffset = new float3(0, 5f, 0);
            //*
            for (int i = 0; i < translationArray.Length; i++) {
                Vector3 position = translationArray[i].Value + skeletonOffset;
                position.z = position.y * .05f;
                Skeleton_Data skeletonData = skeletonDataArray[i];
                // #### TODO: Performance issue when grabbing mesh list
                List<Mesh> meshList = ECS_UnitAnim.GetMeshList(skeletonData.activeUnitAnimTypeEnum, skeletonData.activeAnimDir);
                Mesh mesh = meshList[skeletonData.currentFrame];
                Material material = marineMaterial;
                Graphics.DrawMesh(mesh, position, quaternionIdentity, material, 0);

                Graphics.DrawMesh(ECS_RTSControls.instance.shadowMesh, (float3)position + new float3(0, -8f, -.01f), quaternionIdentity, ECS_RTSControls.instance.shadowMaterial, 0);
            }
            //*/

            translationArray.Dispose();
            skeletonDataArray.Dispose();
        }

    }



}
