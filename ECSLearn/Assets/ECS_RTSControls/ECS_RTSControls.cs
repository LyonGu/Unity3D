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
public class UnitMoveSystem : JobComponentSystem {

    private struct Job : IJobForEachWithEntity<MoveTo, Translation, Skeleton_PlayAnim> {

        public float deltaTime;

        public void Execute(Entity entity, int index, ref MoveTo moveTo, ref Translation translation, ref Skeleton_PlayAnim skeletonPlayAnim) {
            if (moveTo.move) {
                float reachedPositionDistance = 1f;
                if (math.distance(translation.Value, moveTo.position) > reachedPositionDistance) {
                    // Far from target position, Move to position
                    float3 moveDir = math.normalize(moveTo.position - translation.Value);
                    moveTo.lastMoveDir = moveDir;
                    translation.Value += moveDir * moveTo.moveSpeed * deltaTime;
                    skeletonPlayAnim.PlayAnim(ECS_UnitAnimType.TypeEnum.dMarine_Walk, moveDir, default);
                } else {
                    // Already there
                    skeletonPlayAnim.PlayAnim(ECS_UnitAnimType.TypeEnum.dMarine_Idle, moveTo.lastMoveDir, default);
                    moveTo.move = false;
                }
            }
        }

    }

    protected override JobHandle OnUpdate(JobHandle inputDeps) {
        Job job = new Job {
            deltaTime = Time.DeltaTime,
        };
        return job.Schedule(this, inputDeps);
    }

}