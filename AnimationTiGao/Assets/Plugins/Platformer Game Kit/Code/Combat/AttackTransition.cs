// Platformer Game Kit // https://kybernetik.com.au/platformer // Copyright 2021-2023 Kybernetik //

using Animancer;
using System;
using UnityEngine;
//AttackTransition里提前绘制了很多攻击碰撞区域
namespace PlatformerGameKit
{
    /// <summary>A <see cref="ClipTransition"/> with <see cref="HitData"/>.</summary>
    /// <remarks>
    /// Documentation: <see href="https://kybernetik.com.au/platformer/docs/combat/melee">Melee Attacks</see>
    /// </remarks>
    /// https://kybernetik.com.au/platformer/api/PlatformerGameKit/AttackTransition
    /// 
    [Serializable]
    public partial class AttackTransition : ClipTransition, ICopyable<AttackTransition>
    {
        /************************************************************************************************************************/

        [SerializeField]
        private HitData[] _Hits;

        /// <summary>The details of the hit boxes involved in this attack.</summary>
        public HitData[] Hits
        {
            get => _Hits;
            set
            {
                if (_HasInitializedEvents)
                    throw new InvalidOperationException(
                        $"Modifying the {nameof(AttackTransition)}.{nameof(Hits)} after the transition has already been used" +
                        $" is not supported because its initialisation modifies the underlying" +
                        $" {nameof(AnimancerEvent)}.{nameof(AnimancerEvent.Sequence)} in a way that can't be easily undone.");

                _Hits = value;
            }
        }

        private bool _HasInitializedEvents;

        /************************************************************************************************************************/

        /// <inheritdoc/> 启动的时候就会调用
        public override void Apply(AnimancerState state)
        {
            if (!_HasInitializedEvents)
            {
                _HasInitializedEvents = true;
                //还会初始化碰撞区域
                HitData.InitializeEvents(Hits, SerializedEvents.Events, Clip.length);
            }

            base.Apply(state);
        }

        /************************************************************************************************************************/

        /// <inheritdoc/>
        public virtual void CopyFrom(AttackTransition copyFrom)
        {
            CopyFrom((ClipTransition)copyFrom);

            if (copyFrom == null)
            {
                _Hits = default;
                return;
            }

            AnimancerUtilities.CopyExactArray(copyFrom._Hits, ref _Hits);
        }

        /************************************************************************************************************************/
    }
}
