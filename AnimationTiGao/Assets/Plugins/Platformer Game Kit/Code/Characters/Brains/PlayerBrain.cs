// Platformer Game Kit // https://kybernetik.com.au/platformer // Copyright 2021-2023 Kybernetik //

#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value.

using Animancer.FSM;
using Animancer.Units;
using PlatformerGameKit.Characters.States;
using UnityEngine;
using static Animancer.Validate.Value;

//玩家输入控制
namespace PlatformerGameKit.Characters.Brains
{
    /// <summary>
    /// A <see cref="CharacterBrain"/> which manages the states of a player to be controlled by a
    /// <see cref="PlayerInputManager"/> or <see cref="PlayerInputSystem"/>.
    /// </summary>
    /// <remarks>
    /// Documentation: <see href="https://kybernetik.com.au/platformer/docs/characters/brains/player">Player Brain</see>
    /// </remarks>
    /// https://kybernetik.com.au/platformer/api/PlatformerGameKit.Characters.Brains/PlayerBrain
    /// 
    [AddComponentMenu(MenuPrefix + "Player Brain")]
    [HelpURL(APIDocumentation + nameof(PlayerBrain))]
    public class PlayerBrain : CharacterBrain
    {
        /************************************************************************************************************************/

        [SerializeField, Seconds(Rule = IsNotNegative)] private float _InputBufferTimeOut = 0.5f;

        [SerializeField] private CharacterState _Jump;
        [SerializeField] private CharacterState _PrimaryAttack;
        [SerializeField] private CharacterState _SecondaryAttack;

        private StateMachine<CharacterState>.InputBuffer _InputBuffer;

        /************************************************************************************************************************/

        protected virtual void Awake()
        {
            _InputBuffer = new StateMachine<CharacterState>.InputBuffer(Character.StateMachine);
        }

        /************************************************************************************************************************/

        protected virtual void Update()
        {
            _InputBuffer.Update();
        }

        /************************************************************************************************************************/

        public void TryIdle()
            => Character.StateMachine.TrySetDefaultState();

        /************************************************************************************************************************/

        public void Buffer(CharacterState state)
        {
            if (state == null)
                return;

            _InputBuffer.Buffer(state, _InputBufferTimeOut);
        }

        public void TryJump()
            => Buffer(_Jump);

        public void TryPrimaryAttack()
            => Buffer(_PrimaryAttack);

        public void TrySecondaryAttack()
            => Buffer(_SecondaryAttack);

        /************************************************************************************************************************/
    }
}
