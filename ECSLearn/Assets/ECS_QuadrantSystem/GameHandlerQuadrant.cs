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
using Unity.Transforms;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using CodeMonkey.Utils;
using CodeMonkey.MonoBehaviours;

public class GameHandlerQuadrant : MonoBehaviour {

    public static GameHandlerQuadrant instance;

    public bool useQuadrantSystem;

    [SerializeField] private CameraFollow cameraFollow;
    [SerializeField] private Material unitMaterial;
    [SerializeField] private Material targetMaterial;
    [SerializeField] private Mesh quadMesh;

    private static EntityManager entityManager;
    
    private Vector3 cameraFollowPosition;
    private float cameraFollowZoom;
    private Unity.Mathematics.Random random;

    private void Awake() {
        instance = this;
        random = new Unity.Mathematics.Random(56);
    }

    private void Start() {
        cameraFollowZoom = 5f;
        cameraFollow.Setup(() => cameraFollowPosition, () => cameraFollowZoom, true, true);
        entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

        for (int i = 0; i < 4000; i++) {
            SpawnUnitEntity();
        }

        for (int i = 0; i < 80000; i++) {
            SpawnTargetEntity();
        }
    }

    private float spawnTargetTimer;

    private void Update() {
        HandleCamera();
        //return;
        spawnTargetTimer -= Time.deltaTime;
        if (spawnTargetTimer < 0) {
            spawnTargetTimer = .1f;
            
            for (int i = 0; i < 500; i++) {
                SpawnTargetEntity();
            }
        }
    }
    
    private void HandleCamera() {
        Vector3 moveDir = Vector3.zero;
        if (Input.GetKey(KeyCode.W)) { moveDir.y = +1f; }
        if (Input.GetKey(KeyCode.S)) { moveDir.y = -1f; }
        if (Input.GetKey(KeyCode.A)) { moveDir.x = -1f; }
        if (Input.GetKey(KeyCode.D)) { moveDir.x = +1f; }

        moveDir = moveDir.normalized;
        float cameraMoveSpeed = 50f;
        cameraFollowPosition += moveDir * cameraMoveSpeed * Time.deltaTime;

        float zoomSpeed = 200f;
        if (Input.mouseScrollDelta.y > 0) cameraFollowZoom -= 1 * zoomSpeed * Time.deltaTime;
        if (Input.mouseScrollDelta.y < 0) cameraFollowZoom += 1 * zoomSpeed * Time.deltaTime;

        cameraFollowZoom = Mathf.Clamp(cameraFollowZoom, 4f, 40f);
    }

    private void SpawnUnitEntity() {
        SpawnUnitEntity(new float3(random.NextFloat(-100, +100f), random.NextFloat(-80, +80f), 0));
    }

    private void SpawnUnitEntity(float3 position) {
        Entity entity = entityManager.CreateEntity(
            typeof(Translation),
            typeof(LocalToWorld),
            typeof(RenderMesh),
            typeof(Scale),
            typeof(Unit),
            typeof(UnitSelf),
            typeof(QuadrantEntity),
            typeof(RenderBounds)
        );
        SetEntityComponentData(entity, position, quadMesh, unitMaterial);
        entityManager.SetComponentData(entity, new Scale { Value = 1.5f });
        entityManager.SetComponentData(entity, new UnitSelf { self = entity });
        entityManager.SetComponentData(entity, new QuadrantEntity { typeEnum = QuadrantEntity.TypeEnum.Unit });
    }

    private void SpawnTargetEntity() {
        Entity entity = entityManager.CreateEntity(
            typeof(Translation),
            typeof(LocalToWorld),
            typeof(RenderMesh),
            typeof(Scale),
            typeof(Target),
            typeof(TargetSelf),
            typeof(QuadrantEntity),
            typeof(RenderBounds)
        );
        SetEntityComponentData(entity, new float3(random.NextFloat(-100, +100f), random.NextFloat(-80, +80f), 0), quadMesh, targetMaterial);
        entityManager.SetComponentData(entity, new Scale { Value = .5f });
        entityManager.SetComponentData(entity, new TargetSelf { self = entity });
        entityManager.SetComponentData(entity, new QuadrantEntity { typeEnum = QuadrantEntity.TypeEnum.Target });
    }

    private void SetEntityComponentData(Entity entity, float3 spawnPosition, Mesh mesh, Material material) {
        entityManager.SetSharedComponentData<RenderMesh>(entity,
            new RenderMesh {
                material = material,
                mesh = mesh,
            }
        );

        entityManager.SetComponentData<Translation>(entity, 
            new Translation { 
                Value = spawnPosition
            }
        );
    }

}
//
//public struct Unit : IComponentData { }
//public struct Target : IComponentData { }
//
//public struct HasTarget : IComponentData {
//    public Entity targetEntity;
//}
//
//[DisableAutoCreation]
//public class HasTargetDebug : ComponentSystem {
//
//    protected override void OnUpdate() {
//        Entities.ForEach((Entity entity, ref Translation translation, ref HasTarget hasTarget) => {
//            if (World.Active.EntityManager.Exists(hasTarget.targetEntity)) {
//                Translation targetTranslation = World.Active.EntityManager.GetComponentData<Translation>(hasTarget.targetEntity);
//                Debug.DrawLine(translation.Value, targetTranslation.Value);
//            }
//        });
//    }
//
//}








