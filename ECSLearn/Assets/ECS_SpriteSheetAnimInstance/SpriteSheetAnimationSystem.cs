﻿/* 
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
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Transforms;
using Unity.Mathematics;


public struct SpriteSheetAnimation_Data : IComponentData {
    public int currentFrame;  //当前处于第几帧
    public int frameCount;    //总帧数
    public float frameTimer;  //当前时间
    public float frameTimerMax; //帧间隔，数值越大，动画越慢

    public Vector4 uv;
    public Matrix4x4 matrix;
}


public class SpriteSheetAnimation_Animate : JobComponentSystem {

    [BurstCompile]
    public struct Job : IJobForEach<SpriteSheetAnimation_Data, Translation> {

        public float deltaTime;

        public void Execute(ref SpriteSheetAnimation_Data spriteSheetAnimationData, ref Translation translation) {
            spriteSheetAnimationData.frameTimer += deltaTime;
            while (spriteSheetAnimationData.frameTimer >= spriteSheetAnimationData.frameTimerMax) {
                spriteSheetAnimationData.frameTimer -= spriteSheetAnimationData.frameTimerMax;
                spriteSheetAnimationData.currentFrame = (spriteSheetAnimationData.currentFrame + 1) % spriteSheetAnimationData.frameCount;

                float uvWidth = 1f / spriteSheetAnimationData.frameCount;
                float uvHeight = 1f;
                float uvOffsetX = uvWidth * spriteSheetAnimationData.currentFrame;
                float uvOffsetY = 0f;
                spriteSheetAnimationData.uv = new Vector4(uvWidth, uvHeight, uvOffsetX, uvOffsetY);
                
                //调整Z值，越高的z值越大，会被前面的遮挡
                float3 position = translation.Value;
                position.z = position.y * .01f;
                spriteSheetAnimationData.matrix = Matrix4x4.TRS(position, Quaternion.identity, Vector3.one);
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