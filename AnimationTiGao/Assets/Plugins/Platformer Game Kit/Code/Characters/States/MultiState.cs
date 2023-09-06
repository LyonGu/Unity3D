// Platformer Game Kit // https://kybernetik.com.au/platformer // Copyright 2021-2023 Kybernetik //

#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value.

using Animancer;
using UnityEngine;
//一个“状态选择器”，只需从其他状态列表中进行选择。
namespace PlatformerGameKit.Characters.States
{
    /// <summary>A <see cref="CharacterState"/> which redirects to other states.</summary>
    /// <remarks>
    /// Documentation: <see href="https://kybernetik.com.au/platformer/docs/characters/states/multi-state">Multi-State</see>
    /// </remarks>
    /// https://kybernetik.com.au/platformer/api/PlatformerGameKit.Characters.States/MultiState
    ///
    /*
     * 其 DefaultExecutionOrder 设置为 -1000，以便它在其他状态之前的每一帧运行其 FixedUpdate 方法，
     * 这样当它改变远离某个状态时，该状态的 FixedUpdate 方法不会在该帧运行。
     *
     *
     * MultiState 包含其他状态的列表。尝试进入多状态，只是尝试一一进入其他每个状态
     */
    [AddComponentMenu(MenuPrefix + "Multi State")]
    [HelpURL(APIDocumentation + nameof(MultiState))]
    [DefaultExecutionOrder(DefaultExecutionOrder)]
    public class MultiState : CharacterState
    {
        /************************************************************************************************************************/

        /// <summary>Run before other states in case <see cref="FixedUpdate"/> changes the state.</summary>
        public const int DefaultExecutionOrder = -1000;

        /************************************************************************************************************************/

        [SerializeField]
        [Tooltip("While in one of the States, should it try to enter them again in order every " + nameof(FixedUpdate) + "?")]
        private bool _AutoInternalTransitions;
        public bool AutoInternalTransitions => _AutoInternalTransitions;

        [SerializeField]
        [Tooltip("The other states that this one will try to enter in order")]
        private CharacterState[] _States;
        public CharacterState[] States => _States;

        private CharacterState _CurrentState;

        /************************************************************************************************************************/
        //返回第一个可以进入的状态 Character.StateMachine.CanSetState
        public override bool CanEnterState => Character.StateMachine.CanSetState(_States);

        public override bool CanExitState => true;

        /************************************************************************************************************************/

        public override void OnEnterState()
        {
            if (Character.StateMachine.TrySetState(_States))
            {
                //如果成功进入一个状态，当_AutoInternalTransitions为false时，只会选择一个，不会自动检测下一个可以进入的状态
                if (_AutoInternalTransitions)
                {
                    //存下当前状态，如果内部自动转换（一个接着一个状态进入）
                    _CurrentState = Character.StateMachine.CurrentState;
                    enabled = true;
                }
            }
            else
            {
                //状态列表中，没有一个状态可以进入
                var text = ObjectPool.AcquireStringBuilder()
                    .AppendLine($"{nameof(MultiState)} failed to enter any of its {nameof(States)}:");

                for (int i = 0; i < _States.Length; i++)
                {
                    text.Append("    [")
                        .Append(i)
                        .Append("] ")
                        .AppendLine(_States[i].ToString());
                }

                Debug.LogError(text.ReleaseToString(), this);
            }
        }

        public override void OnExitState() { }

        /************************************************************************************************************************/
        //优先于其他状态的FixedUpdate 先执行
        protected virtual void FixedUpdate()
        {
            if (_CurrentState != Character.StateMachine.CurrentState)
            {
                //当此状态跟当前状态机的当前状态不同时，禁用
                enabled = false;
                return;
            }
            //返回第一个可以进入的状态
            var newState = Character.StateMachine.CanSetState(_States);
            if (_CurrentState != newState && newState != null)
            {
                //如果这个状态列表里还有其他状态可以执行 并且跟当前状态不同 就执行对应状态
                _CurrentState = newState;
                Character.StateMachine.ForceSetState(newState);
            }
        }

        /************************************************************************************************************************/
    }
}
