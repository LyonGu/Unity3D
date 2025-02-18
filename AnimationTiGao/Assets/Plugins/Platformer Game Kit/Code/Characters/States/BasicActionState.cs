// Platformer Game Kit // https://kybernetik.com.au/platformer // Copyright 2021-2023 Kybernetik //

#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value.

using Animancer;
using UnityEngine;
//播放动画然后返回空闲状态的一般操作  普通攻击
namespace PlatformerGameKit.Characters.States
{
    /// <summary>A <see cref="CharacterState"/> that plays an animation then returns to idle.</summary>
    /// <remarks>
    /// Documentation: <see href="https://kybernetik.com.au/platformer/docs/characters/states/attack/basic">Basic Action</see>
    /// </remarks>
    /// https://kybernetik.com.au/platformer/api/PlatformerGameKit.Characters.States/BasicActionState
    /// 
    [AddComponentMenu(MenuPrefix + "Basic Action State")]
    [HelpURL(APIDocumentation + nameof(BasicActionState))]
    public class BasicActionState : AttackState
    {
        /************************************************************************************************************************/

        [SerializeReference] private ITransitionWithEvents _Animation;

        /************************************************************************************************************************/

        protected virtual void Awake()
        {
            _Animation.Events.OnEnd += Character.StateMachine.ForceSetDefaultState;
        }

        /************************************************************************************************************************/

        public override void OnEnterState()
        {
            base.OnEnterState();
            Character.Animancer.Play(_Animation);
        }

        /************************************************************************************************************************/
    }
}
