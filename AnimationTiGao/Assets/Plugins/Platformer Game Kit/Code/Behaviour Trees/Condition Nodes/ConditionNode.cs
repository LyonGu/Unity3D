// Platformer Game Kit // https://kybernetik.com.au/platformer // Copyright 2021-2023 Kybernetik //

using System;

/*
 *    ConditionNodes 类似于叶节点，但它们立即检查条件以返回 Result.Pass 或 Result.Fail（而不是 Result.Pending）
 * 
 */
namespace PlatformerGameKit.BehaviourTrees
{
    /// <summary>
    /// An <see cref="IBehaviourNode"/> that checks a boolean <see cref="Condition"/> without an option for
    /// <see cref="Result.Pending"/>.
    /// </summary>
    /// <remarks>
    /// Documentation:
    /// <see href="https://kybernetik.com.au/platformer/docs/characters/brains/behaviour/specific#conditions">
    /// Behaviour Tree Brains - Conditions</see>
    /// </remarks>
    /// https://kybernetik.com.au/platformer/api/PlatformerGameKit.BehaviourTrees/ConditionNode
    /// 
    [Serializable]
    public abstract class ConditionNode : IBehaviourNode
    {
        /************************************************************************************************************************/

        /// <summary>Accesses the <see cref="Condition"/> and calls <see cref="BehaviourTreeUtilities.ToResult"/>.</summary>
        public Result Execute() => Condition.ToResult(); //给bool值添加了扩展方法

        /// <summary>
        /// Called by <see cref="Execute"/> to run this node's main logic. <c>true</c> returns
        /// <see cref="Result.Pass"/> and <c>false</c> returns <see cref="Result.Fail"/>.
        /// </summary>
        public abstract bool Condition { get; }

        /************************************************************************************************************************/

        /// <inheritdoc/>
        int IBehaviourNode.ChildCount => 0;

        /// <inheritdoc/>
        IBehaviourNode IBehaviourNode.GetChild(int index)
            => throw new NotSupportedException($"A {nameof(ConditionNode)} doesn't have any children.");

        /************************************************************************************************************************/
    }
}
