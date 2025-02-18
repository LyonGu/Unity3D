// Platformer Game Kit // https://kybernetik.com.au/platformer // Copyright 2021-2023 Kybernetik //

using System;
/*
 *  Action 委托节点，执行无异常就返回通过，
 *  Invokes a System.Action Delegate. Returns Result.Pass as long as no Exception is thrown. Otherwise Result.Fail. This node is not serializable.
 * 
 */
namespace PlatformerGameKit.BehaviourTrees
{
    /// <summary>A <see cref="LeafNode"/> which invokes an <see cref="System.Action"/>.</summary>
    /// <remarks>
    /// Documentation:
    /// <see href="https://kybernetik.com.au/platformer/docs/characters/brains/behaviour/general#leaves">
    /// Behaviour Tree Brains - Leaves</see>
    /// </remarks>
    /// https://kybernetik.com.au/platformer/api/PlatformerGameKit.BehaviourTrees/ActionNode
    /// 
    public sealed class ActionNode : LeafNode
    {
        /************************************************************************************************************************/

        /// <summary>The delegate which will be invoked by <see cref="Execute"/>.</summary>
        public Action Action { get; set; }

        /************************************************************************************************************************/

        /// <summary>Creates a new <see cref="ActionNode"/>.</summary>
        public ActionNode() { }

        /// <summary>Creates a new <see cref="ActionNode"/>.</summary>
        public ActionNode(Action action)
        {
            Action = action;
        }

        /************************************************************************************************************************/

        /// <inheritdoc/>
        public override Result Execute()
        {
            try
            {
                Action?.Invoke();
                return Result.Pass;
            }
            catch
            {
                return Result.Fail;
            }
        }

        /************************************************************************************************************************/
    }
}
