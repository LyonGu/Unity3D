using UnityEngine;
using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using CodeMonkey.Utils;

public class UnitMoveOrderSystem : ComponentSystem
{

	private PathfindingGridSetup _pathfindingGridSetup;
	protected override void OnCreate()
	{
		base.OnCreate();
		_pathfindingGridSetup = PathfindingGridSetup.Instance;
	}

	protected override void OnUpdate() {
        if (Input.GetMouseButtonDown(0)) {
	        Vector3 mousePosition = UtilsClass.GetMouseWorldPosition();
	        if (_pathfindingGridSetup == null)
		        _pathfindingGridSetup = PathfindingGridSetup.Instance;
	        float cellSize = _pathfindingGridSetup.pathfindingGrid.GetCellSize();

	        _pathfindingGridSetup.pathfindingGrid.GetXY(mousePosition + new Vector3(1, 1) * cellSize * +.5f, out int endX, out int endY);
			
	        //确保否在格子地图内
	        ValidateGridPosition(ref endX, ref endY);
	        //CMDebug.TextPopupMouse(x + ", " + y);
			
	        //主循环遍历
	        Entities.ForEach((Entity entity, DynamicBuffer<PathPosition> pathPositionBuffer, ref Translation translation) => {
		        //Debug.Log("Add Component!");
		        _pathfindingGridSetup.pathfindingGrid.GetXY(translation.Value + new float3(1, 1, 0) * cellSize * +.5f, out int startX, out int startY);

		        ValidateGridPosition(ref startX, ref startY);
				
		        //添加寻路 Component 设置起点和终点  Pathfinding会去遍历
		        EntityManager.AddComponentData(entity, new PathfindingParams { 
			        startPosition = new int2(startX, startY), 
                    endPosition = new int2(endX, endY) 
		        });
	        });

            /*
            Entities.ForEach((Entity entity, ref Translation translation) => {
                // Add Pathfinding Params
                EntityManager.AddComponentData(entity, new PathfindingParams {
                    startPosition = new int2(0, 0),
                    endPosition = new int2(4, 0)
                });
            });
            */
        }
    }

    private void ValidateGridPosition(ref int x, ref int y) {
        x = math.clamp(x, 0, PathfindingGridSetup.Instance.pathfindingGrid.GetWidth() - 1);
        y = math.clamp(y, 0, PathfindingGridSetup.Instance.pathfindingGrid.GetHeight() - 1);
    }

}
