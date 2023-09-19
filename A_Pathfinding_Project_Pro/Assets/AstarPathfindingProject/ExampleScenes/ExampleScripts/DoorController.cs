using UnityEngine;

namespace Pathfinding.Examples {
	/// <summary>Example script used in the example scenes</summary>
	[HelpURL("http://arongranberg.com/astar/documentation/stable/class_pathfinding_1_1_examples_1_1_door_controller.php")]
	public class DoorController : MonoBehaviour {
		private bool open = false;

		public int opentag = 1;
		public int closedtag = 1;
		public bool updateGraphsWithGUO = true;
		public float yOffset = 5;

		Bounds bounds;

		public void Start () {
			// Capture the bounds of the collider while it is closed
			bounds = GetComponent<Collider>().bounds;

			// Initially open the door
			SetState(open);
		}

		void OnGUI () {
			// Show a UI button for opening and closing the door
			if (GUI.Button(new Rect(5, yOffset, 100, 22), "Toggle Door")) {
				SetState(!open);
			}
		}

		public void SetState (bool open) {
			this.open = open;

			if (updateGraphsWithGUO) {
				// Update the graph below the door
				// Set the tag of the nodes below the door
				// To something indicating that the door is open or closed
				// 使用代码动态更新tag GraphUpdateObject
				GraphUpdateObject guo = new GraphUpdateObject(bounds);
				int tag = open ? opentag : closedtag;

				// There are only 32 tags
				if (tag > 31) { Debug.LogError("tag > 31"); return; }

				guo.modifyTag = true;
				guo.setTag = tag;
				
				/*
				 * /// Use physics checks to update nodes.
					/// When updating a grid graph and this is true, the nodes' position and walkability will be updated using physics checks
					/// with settings from "Collision Testing" and "Height Testing".
					///
					/// When updating a PointGraph, setting this to true will make it re-evaluate all connections in the graph which passes through the <see cref="bounds"/>.
					///
					/// This has no effect when updating GridGraphs if <see cref="modifyWalkability"/> is turned on.
					/// You should not combine <see cref="updatePhysics"/> and <see cref="modifyWalkability"/>.
					///
					/// On RecastGraphs, having this enabled will trigger a complete recalculation of all tiles intersecting the bounds.
					/// This is quite slow (but powerful). If you only want to update e.g penalty on existing nodes, leave it disabled.
				 * 
				 */
				guo.updatePhysics = false; //设置会true，会刷新地图数据，如果只是简单更改tag或者penalty 可以设置为false

				AstarPath.active.UpdateGraphs(guo);
			}

			// Play door animations
			if (open) {
				GetComponent<Animation>().Play("Open");
			} else {
				GetComponent<Animation>().Play("Close");
			}
		}
	}
}
