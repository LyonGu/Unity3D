// Animancer // https://kybernetik.com.au/animancer // Copyright 2021 Kybernetik //

#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value.

using Animancer.Examples.StateMachines.Brains;
using System;
using UnityEngine;

namespace Animancer.Examples.StateMachines.Weapons
{
    //执行这个状态的EquipAnimation and UnequipAnimation 其他动作无法执行
    /// <summary>A <see cref="CharacterState"/> which managed the currently equipped <see cref="Weapon"/>.</summary>
    /// <example><see href="https://kybernetik.com.au/animancer/docs/examples/fsm/weapons">Weapons</see></example>
    /// https://kybernetik.com.au/animancer/api/Animancer.Examples.StateMachines.Weapons/EquipState
    /// 
    [AddComponentMenu(Strings.ExamplesMenuPrefix + "Weapons - Equip State")]
    [HelpURL(Strings.DocsURLs.ExampleAPIDocumentation + nameof(StateMachines) + "." + nameof(Weapons) + "/" + nameof(EquipState))]
    public sealed class EquipState : CharacterState
    {
        /************************************************************************************************************************/

        [SerializeField] private Transform _WeaponHolder;
        [SerializeField] private Weapon _Weapon; //当前持有的武器

        private Weapon _EquippingWeapon; //当前正在换的武器
        private Action _OnUnequipEnd; //卸下装备结束回调

        /************************************************************************************************************************/

        // Called by UI Buttons.
        public Weapon Weapon
        {
            get => _Weapon;
            set
            {
                if (enabled)
                    return;

                _EquippingWeapon = value;
                //如果无法进入这个状态，直接_EquippingWeapon
                if (!Character.StateMachine.TrySetState(this))
                    _EquippingWeapon = _Weapon;
            }
        }

        /************************************************************************************************************************/

        private void Awake()
        {
            _EquippingWeapon = _Weapon;
            _OnUnequipEnd = OnUnequipEnd;
            AttachWeapon();
        }

        /************************************************************************************************************************/

        // This state can only be entered by setting the Weapon property.
        public override bool CanEnterState => _Weapon != _EquippingWeapon;

        /************************************************************************************************************************/

        /// <summary>
        /// Start at the beginning of the sequence by default, but if the previous attack hasn't faded out yet then
        /// perform the next attack instead.
        /// </summary>
        private void OnEnable()
        {
            if (_Weapon.UnequipAnimation.IsValid)
            {
                //如果有卸下装备动画，就执行动画，动画结束执行回调处理
                var state = Character.Animancer.Play(_Weapon.UnequipAnimation);
                state.Events.OnEnd = _OnUnequipEnd;
            }
            else
            {
                OnUnequipEnd();
            }
        }

        /************************************************************************************************************************/

        private void OnUnequipEnd()
        {
            //卸下之前的武器
            DetachWeapon();
            //把正在换装的武器赋值给当前持有武器变量
            _Weapon = _EquippingWeapon;
            AttachWeapon();

            if (_Weapon.EquipAnimation.IsValid)
            {
                //有换上武器动画，执行动画，动画结束后恢复到默认动作
                var state = Character.Animancer.Play(_Weapon.EquipAnimation);
                state.Events.OnEnd = Character.StateMachine.ForceSetDefaultState;
            }
            else
            {
                //没有动画直接进入默认动作
                Character.StateMachine.ForceSetState(Character.Idle);
            }
        }

        /************************************************************************************************************************/

        private void AttachWeapon()
        {
            if (_Weapon == null)
                return;

            if (_WeaponHolder != null)
            {
                var transform = _Weapon.transform;
                transform.parent = _WeaponHolder;
                transform.localPosition = default;
                transform.localRotation = Quaternion.identity;
                transform.localScale = Vector3.one;
            }

            _Weapon.gameObject.SetActive(true);
        }

        private void DetachWeapon()
        {
            if (_Weapon == null)
                return;

            // It might be more appropriate to reparent inactive weapons to the inventory system if you have one.
            // Or you could even attach them to specific bones on the character and leave them active.
            _Weapon.transform.parent = transform;
            _Weapon.gameObject.SetActive(false);
        }

        /************************************************************************************************************************/

        private void FixedUpdate()
        {
            Character.Rigidbody.velocity = default;
        }

        /************************************************************************************************************************/
        // 设置为false，表示这个状态不能别打断，例如换武器的时候不能执行攻击操作
        public override bool CanExitState => false;

        /************************************************************************************************************************/
    }
}
