// Platformer Game Kit // https://kybernetik.com.au/platformer // Copyright 2021-2023 Kybernetik //

using System;

/*
 *    反转节点
 *     Executes the child and reverses its result so Result.Pass becomes Result.Fail and vice-versa. Result.Pending is unchanged.
 *
 *     switch (Child.Execute())
        {
            case Result.Pending: return Result.Pending;
            case Result.Pass: return Result.Fail;
            case Result.Fail: return Result.Pass;
        }
 */
namespace PlatformerGameKit.BehaviourTrees
{
    /// <summary>
    /// A <see cref="ModifierNode"/> which executes its <see cref="ModifierNode.Child"/> and returns the opposite of
    /// its <see cref="Result"/> (using <see cref="BehaviourTreeUtilities.Invert"/>).
    /// </summary>
    /// <remarks>
    /// Documentation:
    /// <see href="https://kybernetik.com.au/platformer/docs/characters/brains/behaviour/general#modifiers">
    /// Behaviour Tree Brains - Modifiers</see>
    /// </remarks>
    /// https://kybernetik.com.au/platformer/api/PlatformerGameKit.BehaviourTrees/Invert
    /// 
    [Serializable]
    public sealed class Invert : ModifierNode
    {
        /************************************************************************************************************************/

        /// <inheritdoc/>
        public override Result Execute()
            => Child != null ? Child.Execute().Invert() : Result.Pass;

        /************************************************************************************************************************/
    }
}
