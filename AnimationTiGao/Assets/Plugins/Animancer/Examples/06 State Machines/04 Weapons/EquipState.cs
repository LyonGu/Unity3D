// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2023 Kybernetik //

#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value.

using System;
using UnityEngine;

namespace Animancer.Examples.StateMachines
{
    /// <summary>A <see cref="CharacterState"/> which managed the currently equipped <see cref="CurrentWeapon"/>.</summary>
    /// <example><see href="https://kybernetik.com.au/animancer/docs/examples/fsm/weapons">Weapons</see></example>
    /// https://kybernetik.com.au/animancer/api/Animancer.Examples.StateMachines/EquipState
    /// 
    [AddComponentMenu(Strings.ExamplesMenuPrefix + "Weapons - Equip State")]
    [HelpURL(Strings.DocsURLs.ExampleAPIDocumentation + nameof(StateMachines) + "/" + nameof(EquipState))]
    public sealed class EquipState : CharacterState
    {
        /************************************************************************************************************************/

        private Action _OnUnequipEnd;

        public Weapon NextWeapon { get; set; }

        public Weapon CurrentWeapon => Character.Equipment.Weapon;

        /************************************************************************************************************************/

        private void Awake()
        {
            _OnUnequipEnd = OnUnequipEnd;
            NextWeapon = CurrentWeapon;

            // In this example we assign End Events to the weapon's animations each time they're used.
            // That will modify the transition's events each time.
            // In most situations, events on a transition should be set only once on startup.
            // Animancer would log a warning about that because it's a common source of bugs.
            // But in this example, we know what we're doing so we just disable the warning.
            /*
             *我们在每次使用武器时将结束事件分配给武器的动画。在大多数情况下，过渡事件应在启动时仅设置一次，
             * 因为任何更改都会累积修改过渡事件。Animancer 会记录一个警告，因为它是错误的常见来源，
             * 但在这个例子中，我们知道我们在做什么，所以我们只是禁用警告
             * 
             */
            OptionalWarning.LockedEvents.Disable();
        }

        /************************************************************************************************************************/

        public override bool CanEnterState =>
            !enabled &&
            NextWeapon != CurrentWeapon;

        /************************************************************************************************************************/

        private void OnEnable()
        {
            if (CurrentWeapon.UnequipAnimation.IsValid)
            {
                var state = Character.Animancer.Play(CurrentWeapon.UnequipAnimation);
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
            Character.Equipment.Weapon = NextWeapon;

            if (CurrentWeapon.EquipAnimation.IsValid)
            {
                var state = Character.Animancer.Play(CurrentWeapon.EquipAnimation);
                state.Events.OnEnd = Character.StateMachine.ForceSetDefaultState;
            }
            else
            {
                Character.StateMachine.ForceSetDefaultState();
            }
        }

        /************************************************************************************************************************/

        public override CharacterStatePriority Priority => CharacterStatePriority.Medium;

        /************************************************************************************************************************/
    }
}
