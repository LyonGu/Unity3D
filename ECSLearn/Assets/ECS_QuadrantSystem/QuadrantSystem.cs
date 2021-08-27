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
using Unity.Collections;
using Unity.Transforms;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Jobs;
using Unity.Burst;
using CodeMonkey.Utils;


public struct QuadrantEntity : IComponentData {
     public TypeEnum typeEnum;
 
     public enum TypeEnum {
         Unit,
         Target
     }
 }

public struct QuadrantData {
    public Entity entity;
    public float3 position;
    public QuadrantEntity quadrantEntity;
}

public class QuadrantSystem : ComponentSystem {

    // 使用NativeMultiHashMap存储，一个象限Index包含多个对象数据
    public static NativeMultiHashMap<int, QuadrantData> quadrantMultiHashMap;

    public const int quadrantYMultiplier = 1000;
    private const int quadrantCellSize = 10;

    public static int GetPositionHashMapKey(float3 position) {
        return (int) (math.floor(position.x / quadrantCellSize) + (quadrantYMultiplier * math.floor(position.y / quadrantCellSize)));
    }

    private static void DebugDrawQuadrant(float3 position) {
        Vector3 lowerLeft = new Vector3(math.floor(position.x / quadrantCellSize) * quadrantCellSize, math.floor(position.y / quadrantCellSize) * quadrantCellSize);
        Debug.DrawLine(lowerLeft, lowerLeft + new Vector3(+1, +0) * quadrantCellSize);
        Debug.DrawLine(lowerLeft, lowerLeft + new Vector3(+0, +1) * quadrantCellSize);
        Debug.DrawLine(lowerLeft + new Vector3(+1, +0) * quadrantCellSize, lowerLeft + new Vector3(+1, +1) * quadrantCellSize);
        Debug.DrawLine(lowerLeft + new Vector3(+0, +1) * quadrantCellSize, lowerLeft + new Vector3(+1, +1) * quadrantCellSize);
        Debug.Log(GetPositionHashMapKey(position) + " " + position);
    }

    private static int GetEntityCountInHashMap(NativeMultiHashMap<int, QuadrantData> quadrantMultiHashMap, int hashMapKey) {
        QuadrantData quadrantData;
        NativeMultiHashMapIterator<int> nativeMultiHashMapIterator;
        int count = 0;
        if (quadrantMultiHashMap.TryGetFirstValue(hashMapKey, out quadrantData, out nativeMultiHashMapIterator)) {
            do {
                count++;
            } while (quadrantMultiHashMap.TryGetNextValue(out quadrantData, ref nativeMultiHashMapIterator));
        }
        return count;
    }

    [BurstCompile]
    private struct SetQuadrantDataHashMapJob : IJobForEachWithEntity<Translation, QuadrantEntity> {

        public NativeMultiHashMap<int, QuadrantData>.ParallelWriter quadrantMultiHashMap;

        public void Execute(Entity entity, int index, ref Translation translation, ref QuadrantEntity quadrantEntity) {
            int hashMapKey = GetPositionHashMapKey(translation.Value);
            quadrantMultiHashMap.Add(hashMapKey, new QuadrantData {
                entity = entity,
                position = translation.Value,
                quadrantEntity = quadrantEntity
            });
        }

    }

    private EntityQuery entityQuery;
    protected override void OnCreate() {
        quadrantMultiHashMap = new NativeMultiHashMap<int, QuadrantData>(0, Allocator.Persistent);
        base.OnCreate();
        entityQuery = GetEntityQuery(typeof(Translation), typeof(QuadrantEntity));
    }

    protected override void OnDestroy() {
        quadrantMultiHashMap.Dispose();
        base.OnDestroy();
    }

    protected override void OnUpdate() {
        
        quadrantMultiHashMap.Clear();
        int count = entityQuery.CalculateEntityCount();
        
        //动态扩展NativeMultiHashMap的大小
        if (count > quadrantMultiHashMap.Capacity) {
            quadrantMultiHashMap.Capacity = count;
        }

        SetQuadrantDataHashMapJob setQuadrantDataHashMapJob = new SetQuadrantDataHashMapJob {
            quadrantMultiHashMap = quadrantMultiHashMap.AsParallelWriter(),
        };
        JobHandle jobHandle = JobForEachExtensions.Schedule(setQuadrantDataHashMapJob, entityQuery);
        jobHandle.Complete();

        //DebugDrawQuadrant(UtilsClass.GetMouseWorldPosition());
        //Debug.Log(GetEntityCountInHashMap(quadrantMultiHashMap, GetPositionHashMapKey(UtilsClass.GetMouseWorldPosition())));
    }

}
