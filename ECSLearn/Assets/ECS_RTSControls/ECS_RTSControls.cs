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
using V_AnimationSystem;
using System.Threading;
using ECS_AnimationSystem;
using CodeMonkey.Utils;
using CodeMonkey.MonoBehaviours;

public class ECS_RTSControls : MonoBehaviour {

    public static ECS_RTSControls instance;

    [SerializeField] private CameraFollow cameraFollow;
    private Vector3 cameraFollowPosition;
    private float cameraFollowZoom;

    public Mesh quadMesh;
    public Material marineMaterial;
    public Material shadowMaterial;
    public Mesh shadowMesh;
    public Transform selectionAreaTransform;
    public Material unitSelectedCircleMaterial;
    public Mesh unitSelectedCircleMesh;

    private EntityManager entityManager;


    private void Awake() {
        instance = this;
    }

    private void Start() {
        cameraFollowZoom = 80f;
        cameraFollow.Setup(() => cameraFollowPosition, () => cameraFollowZoom, true, true);
        entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        
        //动作数据初始化
        ECS_Animation.Init();

        shadowMesh = ECS_Animation.CreateMesh(9f, 6f);
        unitSelectedCircleMesh = ECS_Animation.CreateMesh(8f, 5f);

        for (int i = 0; i < 30; i++) {
            SpawnMarine();
        }
    }

    private void SpawnMarine() {
        SpawnMarine(new float3(UnityEngine.Random.Range(-70f, 70f), UnityEngine.Random.Range(-60f, 60f), 0f));
    }

    private void SpawnMarine(float3 spawnPosition) {
        EntityArchetype entityArchetype = entityManager.CreateArchetype(
            typeof(Marine),
            typeof(Translation),
            typeof(MoveTo),
            typeof(Skeleton_Data),
            typeof(Skeleton_PlayAnim)
        );

        Entity entity = entityManager.CreateEntity(entityArchetype);

        entityManager.SetComponentData(entity, new Translation { Value = spawnPosition });
        entityManager.SetComponentData(entity, new Skeleton_Data { frameRate = 1f });
        entityManager.SetComponentData(entity, new Skeleton_PlayAnim { ecsUnitAnimTypeEnum = ECS_UnitAnimType.TypeEnum.dBareHands_Idle, animDir = UnitAnim.AnimDir.Down });
        entityManager.SetComponentData(entity, new MoveTo { move = true, position = spawnPosition, moveSpeed = 40f });
            
        ECS_Animation.PlayAnimForced(entity, ECS_UnitAnimType.TypeEnum.dBareHands_Idle, new Vector3(0, -1), default);
    }

    private void Update() {
        HandleCamera();
    }

    private void HandleCamera() {
        Vector3 moveDir = Vector3.zero;
        if (Input.GetKey(KeyCode.W)) { moveDir.y = +1f; }
        if (Input.GetKey(KeyCode.S)) { moveDir.y = -1f; }
        if (Input.GetKey(KeyCode.A)) { moveDir.x = -1f; }
        if (Input.GetKey(KeyCode.D)) { moveDir.x = +1f; }

        moveDir = moveDir.normalized;
        float cameraMoveSpeed = 300f;
        cameraFollowPosition += moveDir * cameraMoveSpeed * Time.deltaTime;

        float zoomSpeed = 1500f;
        if (Input.mouseScrollDelta.y > 0) cameraFollowZoom -= 1 * zoomSpeed * Time.deltaTime;
        if (Input.mouseScrollDelta.y < 0) cameraFollowZoom += 1 * zoomSpeed * Time.deltaTime;

        cameraFollowZoom = Mathf.Clamp(cameraFollowZoom, 20f, 200f);
    }

    public static float GetCameraShakeIntensity() {
        float intensity = Mathf.Clamp(.7f - instance.cameraFollowZoom / 170f, .0f, 2f);
        return intensity;
    }

}


public struct Marine : IComponentData { }


public struct MoveTo : IComponentData {
    public bool move;
    public float3 position;
    public float3 lastMoveDir;
    public float moveSpeed;
}


// Unit go to Move Position
//public class UnitMoveSystem : JobComponentSystem {
//
//    private struct Job : IJobForEachWithEntity<MoveTo, Translation, Skeleton_PlayAnim> {
//
//        public float deltaTime;
//
//        public void Execute(Entity entity, int index, ref MoveTo moveTo, ref Translation translation, ref Skeleton_PlayAnim skeletonPlayAnim) {
//            if (moveTo.move) {
//                float reachedPositionDistance = 1f;
//                if (math.distancesq(translation.Value, moveTo.position) > reachedPositionDistance * reachedPositionDistance) {
//                    // Far from target position, Move to position
//                    float3 moveDir = math.normalize(moveTo.position - translation.Value);
//                    moveTo.lastMoveDir = moveDir;
//                    translation.Value += moveDir * moveTo.moveSpeed * deltaTime;
//                    skeletonPlayAnim.PlayAnim(ECS_UnitAnimType.TypeEnum.dMarine_Walk, moveDir, default);
//                } else {
//                    // Already there
//                    skeletonPlayAnim.PlayAnim(ECS_UnitAnimType.TypeEnum.dMarine_Idle, moveTo.lastMoveDir, default);
//                    moveTo.move = false;
//                }
//            }
//        }
//
//    }
//
//    protected override JobHandle OnUpdate(JobHandle inputDeps) {
//        Job job = new Job {
//            deltaTime = Time.DeltaTime,
//        };
//        return job.Schedule(this, inputDeps);
//    }
//
//}

public class UnitMoveSystemEx : SystemBase
{
    private EntityQuery _entityQuery;
    protected override void OnCreate()
    {
        base.OnCreate();
        _entityQuery = GetEntityQuery(
            typeof(Marine),
            typeof(Translation),
            typeof(MoveTo),
            typeof(Skeleton_PlayAnim)
        );
    }

    struct MoveJob : IJobEntityBatch
    {
        
        //ref MoveTo moveTo, ref Translation translation, ref Skeleton_PlayAnim skeletonPlayAnim
        public float deltaTime;
        public ComponentTypeHandle<Translation> TranslationTypeHandle;
        public ComponentTypeHandle<MoveTo> MoveToTypeHandle;
        public ComponentTypeHandle<Skeleton_PlayAnim> Skeleton_PlayAnimTypeHandle;
        
        public void Execute(ArchetypeChunk batchInChunk, int batchIndex)
        {
            NativeArray<Translation> positions = batchInChunk.GetNativeArray<Translation>(TranslationTypeHandle);
            NativeArray<MoveTo> moveTos = batchInChunk.GetNativeArray<MoveTo>(MoveToTypeHandle);
            NativeArray<Skeleton_PlayAnim> skeleton_PlayAnims = batchInChunk.GetNativeArray<Skeleton_PlayAnim>(Skeleton_PlayAnimTypeHandle);

            for (int i = 0; i < positions.Length; i++)
            {
                var moveTo = moveTos[i];
                var translation = positions[i];
                var skeletonPlayAnim = skeleton_PlayAnims[i];
                if (moveTo.move) {
                    float reachedPositionDistance = 1f;
                    if (math.distancesq(translation.Value, moveTo.position) > reachedPositionDistance * reachedPositionDistance) {
                        // Far from target position, Move to position
                        float3 moveDir = math.normalize(moveTo.position - translation.Value);
                        moveTo.lastMoveDir = moveDir;
                        moveTos[i] = moveTo;
                        
                        translation.Value += moveDir * moveTo.moveSpeed * deltaTime;
                        positions[i] = translation;
                        
                        skeletonPlayAnim.PlayAnim(ECS_UnitAnimType.TypeEnum.dMarine_Walk, moveDir, default);
                        skeleton_PlayAnims[i] = skeletonPlayAnim;
                    } else {
                        // Already there
                        skeletonPlayAnim.PlayAnim(ECS_UnitAnimType.TypeEnum.dMarine_Idle, moveTo.lastMoveDir, default);
                        skeleton_PlayAnims[i] = skeletonPlayAnim;
                        moveTo.move = false;
                        moveTos[i] = moveTo;
                    }
                }
}
            
        }
    }
    protected override void OnUpdate()
    {
        var job = new MoveJob
        {
            deltaTime = Time.DeltaTime,
            TranslationTypeHandle = this.GetComponentTypeHandle<Translation>(),
            MoveToTypeHandle = this.GetComponentTypeHandle<MoveTo>(),
            Skeleton_PlayAnimTypeHandle = this.GetComponentTypeHandle<Skeleton_PlayAnim>()
        };
        this.Dependency = job.ScheduleParallel(_entityQuery, 1, this.Dependency);
    }
}