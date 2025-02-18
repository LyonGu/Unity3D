// Platformer Game Kit // https://kybernetik.com.au/platformer // Copyright 2021-2023 Kybernetik //

using UnityEngine;

namespace PlatformerGameKit.Characters.States
{
    /// <summary>A <see cref="CharacterState"/> that plays an attack animation then returns to idle.</summary>
    /// <remarks>
    /// Documentation: <see href="https://kybernetik.com.au/platformer/docs/characters/states/attack">Attack</see>
    /// </remarks>
    /// https://kybernetik.com.au/platformer/api/PlatformerGameKit.Characters.States/AttackState
    /// 
    [HelpURL(APIDocumentation + nameof(AttackState))]
    public abstract class AttackState : CharacterState
    {
        /************************************************************************************************************************/

        /// <inheritdoc/> 攻击状态不能转向
        public override bool CanTurn => false;

        /************************************************************************************************************************/

        /// <inheritdoc/> 攻击状态不能退出
        public override bool CanExitState => false;

        /************************************************************************************************************************/

        /// <inheritdoc/>
        public override void OnExitState()
        {
            base.OnExitState();
            //当状态结束时，它会清除所有仍处于活动状态的 Hit Box
            Character.Animancer.EndHitSequence();
        }

        /************************************************************************************************************************/
    }
}
