// Platformer Game Kit // https://kybernetik.com.au/platformer // Copyright 2021-2023 Kybernetik //

using System;

/*
 *  顺序节点，必须子节点都满足才返回通过
 *    Executes each child until one returns Result.Pending or Result.Fail similar to the && operator in C#.
 *     if (Children[0].Execute() &&
            Children[1].Execute() &&
            Children[2].Execute() &&
            ...
 */
namespace PlatformerGameKit.BehaviourTrees
{
    /// <summary>A <see cref="GroupNode"/> which executes each child until one returns <see cref="Result.Fail"/>.</summary>
    /// <remarks>
    /// Not sure whether this class description is darker than <see cref="Selector"/> or not.
    /// <para></para>
    /// Documentation:
    /// <see href="https://kybernetik.com.au/platformer/docs/characters/brains/behaviour/general#groups">
    /// Behaviour Tree Brains - Groups</see>
    /// </remarks>
    /// https://kybernetik.com.au/platformer/api/PlatformerGameKit.BehaviourTrees/Sequence
    [Serializable]
    public sealed class Sequence : GroupNode
    {
        /************************************************************************************************************************/

        /// <inheritdoc/>
        public override Result Execute()
        {
            for (int i = 0; i < Children.Length; i++)
            {
                var behaviour = Children[i];
                if (behaviour == null)
                    continue;

                var result = behaviour.Execute();
                if (result != Result.Pass)
                    return result;
            }

            return Result.Pass;
        }

        /************************************************************************************************************************/
    }
}
