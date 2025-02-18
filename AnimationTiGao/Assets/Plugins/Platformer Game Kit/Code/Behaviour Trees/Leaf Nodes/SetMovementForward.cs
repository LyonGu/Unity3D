// Platformer Game Kit // https://kybernetik.com.au/platformer // Copyright 2021-2023 Kybernetik //

using PlatformerGameKit.Characters;
using System;
//将Character.MovementDirectionX 设置为他们当前面向的方向
namespace PlatformerGameKit.BehaviourTrees
{
    /// <summary>
    /// A <see cref="LeafNode"/> which sets the <see cref="Character.MovementDirection"/> to match their
    /// <see cref="CharacterAnimancerComponent.Facing"/>.
    /// </summary>
    /// <remarks>
    /// Documentation:
    /// <see href="https://kybernetik.com.au/platformer/docs/characters/brains/behaviour/specific#leaves">
    /// Behaviour Tree Brains - Leaves</see>
    /// </remarks>
    /// https://kybernetik.com.au/platformer/api/PlatformerGameKit.BehaviourTrees/SetMovementForward
    /// 
    [Serializable]
    public sealed class SetMovementForward : LeafNode
    {
        /************************************************************************************************************************/

        /// <inheritdoc/>
        public override Result Execute()
        {
            var character = Context<Character>.Current;
            //移动组件 UpdateVelocity里会用到
            character.MovementDirection = character.Animancer.Facing;
            return Result.Pass;
        }

        /************************************************************************************************************************/
    }
}
