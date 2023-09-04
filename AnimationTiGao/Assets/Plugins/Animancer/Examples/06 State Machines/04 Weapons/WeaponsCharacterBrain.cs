// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2023 Kybernetik //

#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value.

using Animancer.FSM;
using Animancer.Units;
using System;
using UnityEngine;

namespace Animancer.Examples.StateMachines
{
    /// <summary>Uses player input to control a <see cref="Character"/>.</summary>
    /// <example><see href="https://kybernetik.com.au/animancer/docs/examples/fsm/weapons">Weapons</see></example>
    /// https://kybernetik.com.au/animancer/api/Animancer.Examples.StateMachines/WeaponsCharacterBrain
    /// 
    [AddComponentMenu(Strings.ExamplesMenuPrefix + "Weapons - Weapons Character Brain")]
    [HelpURL(Strings.DocsURLs.ExampleAPIDocumentation + nameof(StateMachines) + "/" + nameof(WeaponsCharacterBrain))]
    public sealed class WeaponsCharacterBrain : MonoBehaviour
    {
        /************************************************************************************************************************/

        [SerializeField] private Character _Character;
        [SerializeField] private CharacterState _Move;
        [SerializeField] private CharacterState _Attack;
        
        //在0.5s内会不断尝试进入状态，成功进入就不尝试了
        [SerializeField, Seconds] private float _InputTimeOut = 0.5f;
        [SerializeField] private EquipState _Equip;
        [SerializeField] private Weapon[] _Weapons;

        private StateMachine<CharacterState>.InputBuffer _InputBuffer;

        /************************************************************************************************************************/

        private void Awake()
        {
            //创建一个状态缓冲
            _InputBuffer = new StateMachine<CharacterState>.InputBuffer(_Character.StateMachine);
        }

        /************************************************************************************************************************/

        private void Update()
        {
            UpdateMovement();
            UpdateEquip();
            UpdateAction();

            _InputBuffer.Update();
        }

        /************************************************************************************************************************/

        private void UpdateMovement()// This method is identical to the one in MovingCharacterBrain.
        {
            var input = ExampleInput.WASD;
            if (input != default)
            {
                // Get the camera's forward and right vectors and flatten them onto the XZ plane.
                var camera = Camera.main.transform;

                var forward = camera.forward;
                forward.y = 0;
                forward.Normalize();

                var right = camera.right;
                right.y = 0;
                right.Normalize();

                // Build the movement vector by multiplying the input by those axes.
                _Character.Parameters.MovementDirection =
                   right * input.x +
                   forward * input.y;

                // Enter the locomotion state if we aren't already in it.
                _Character.StateMachine.TrySetState(_Move);
            }
            else
            {
                _Character.Parameters.MovementDirection = default;
                _Character.StateMachine.TrySetDefaultState();
            }

            // Indicate whether the character wants to run or not.
            _Character.Parameters.WantsToRun = ExampleInput.LeftShiftHold;
        }

        /************************************************************************************************************************/

        private void UpdateEquip()
        {
            if (ExampleInput.RightMouseDown)
            {
                var equippedWeaponIndex = Array.IndexOf(_Weapons, _Character.Equipment.Weapon);

                equippedWeaponIndex++;
                if (equippedWeaponIndex >= _Weapons.Length)
                    equippedWeaponIndex = 0;

                _Equip.NextWeapon = _Weapons[equippedWeaponIndex];
                
                //使用缓冲 切换到装备状态, 并不会立即执行，等到_InputTimeOut结束才执行
                //Doesn't actually attempt to enter the state until <see cref="Update(float)"/> is called.
                _InputBuffer.Buffer(_Equip, _InputTimeOut);
            }
        }

        /************************************************************************************************************************/

        private void UpdateAction()
        {
            if (ExampleInput.LeftMouseDown)
            {
                //使用缓冲 切换到攻击状态
                _InputBuffer.Buffer(_Attack, _InputTimeOut);
            }
        }

        /************************************************************************************************************************/
    }
}
