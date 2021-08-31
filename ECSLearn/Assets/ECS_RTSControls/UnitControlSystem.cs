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
using Unity.Transforms;
using Unity.Mathematics;
using CodeMonkey.Utils;

public struct UnitSelected : IComponentData {
}

public class UnitControlSystem : ComponentSystem {

    private float3 startPosition;

    protected override void OnUpdate() {
        if (Input.GetMouseButtonDown(0)) {
            // Mouse Pressed
            ECS_RTSControls.instance.selectionAreaTransform.gameObject.SetActive(true);
            startPosition = UtilsClass.GetMouseWorldPosition();
            ECS_RTSControls.instance.selectionAreaTransform.position = startPosition;
        }

        if (Input.GetMouseButton(0)) {
            // Mouse Held Down
            float3 selectionAreaSize = (float3)UtilsClass.GetMouseWorldPosition() - startPosition;
            ECS_RTSControls.instance.selectionAreaTransform.localScale = selectionAreaSize;
        }

        if (Input.GetMouseButtonUp(0)) {
            // Mouse Released
            ECS_RTSControls.instance.selectionAreaTransform.gameObject.SetActive(false);
            float3 endPosition = UtilsClass.GetMouseWorldPosition();

            float3 lowerLeftPosition = new float3(math.min(startPosition.x, endPosition.x), math.min(startPosition.y, endPosition.y), 0);
            float3 upperRightPosition = new float3(math.max(startPosition.x, endPosition.x), math.max(startPosition.y, endPosition.y), 0);

            bool selectOnlyOneEntity = false;
            float selectionAreaMinSize = 10f;
            float selectionAreaSize = math.distance(lowerLeftPosition, upperRightPosition);
            if (selectionAreaSize < selectionAreaMinSize) {
                // Selection area too small
                lowerLeftPosition +=  new float3(-1, -1, 0) * (selectionAreaMinSize - selectionAreaSize) * .5f;
                upperRightPosition += new float3(+1, +1, 0) * (selectionAreaMinSize - selectionAreaSize) * .5f;
                selectOnlyOneEntity = true;
            }

            // Deselect all selected Entities
            Entities.WithAll<UnitSelected>().ForEach((Entity entity) => {
                PostUpdateCommands.RemoveComponent<UnitSelected>(entity);
            });

            // Select Entities inside selection area
            int selectedEntityCount = 0;
            Entities.ForEach((Entity entity, ref Translation translation) => {
                if (selectOnlyOneEntity == false || selectedEntityCount < 1) {
                    float3 entityPosition = translation.Value;
                    if (entityPosition.x >= lowerLeftPosition.x &&
                        entityPosition.y >= lowerLeftPosition.y &&
                        entityPosition.x <= upperRightPosition.x &&
                        entityPosition.y <= upperRightPosition.y) {
                        // Entity inside selection area
                        PostUpdateCommands.AddComponent(entity, new UnitSelected());
                        selectedEntityCount++;
                    }
                }
            });
        }

        if (Input.GetMouseButtonDown(1)) {
            // Right mouse button down
            float3 targetPosition = UtilsClass.GetMouseWorldPosition();
            List<float3> movePositionList = GetPositionListAround(targetPosition, new float[] { 10f, 20f, 30f }, new int[] { 5, 10, 20 });
            int positionIndex = 0;
            Entities.WithAll<UnitSelected>().ForEach((Entity entity, ref MoveTo moveTo) => {
                moveTo.position = movePositionList[positionIndex];
                positionIndex = (positionIndex + 1) % movePositionList.Count;
                moveTo.move = true;
            });
        }
    }

    private List<float3> GetPositionListAround(float3 startPosition, float[] ringDistance, int[] ringPositionCount) {
        List<float3> positionList = new List<float3>();
        positionList.Add(startPosition);
        for (int ring = 0; ring < ringPositionCount.Length; ring++) {
            List<float3> ringPositionList = GetPositionListAround(startPosition, ringDistance[ring], ringPositionCount[ring]);
            positionList.AddRange(ringPositionList);
        }
        return positionList;
    }

    private List<float3> GetPositionListAround(float3 startPosition, float distance, int positionCount) {
        List<float3> positionList = new List<float3>();
        for (int i = 0; i < positionCount; i++) {
            int angle = i * (360 / positionCount);
            float3 dir = ApplyRotationToVector(new float3(0, 1, 0), angle);
            float3 position = startPosition + dir * distance;
            positionList.Add(position);
        }
        return positionList;
    }

    private float3 ApplyRotationToVector(float3 vec, float angle) {
        return Quaternion.Euler(0, 0, angle) * vec;
    }

}

public class UnitSelectedRenderer : ComponentSystem {
    
    protected override void OnUpdate() {
        Entities.WithAll<UnitSelected>().ForEach((ref Translation translation) => {
            float3 position = translation.Value + new float3(0, -3f, +5f);
            Graphics.DrawMesh(
                ECS_RTSControls.instance.unitSelectedCircleMesh, 
                position,
                Quaternion.identity, 
                ECS_RTSControls.instance.unitSelectedCircleMaterial, 
                0
            );
        });
    }

}
