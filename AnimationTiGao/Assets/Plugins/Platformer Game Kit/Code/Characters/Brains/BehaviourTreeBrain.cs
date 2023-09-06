// Platformer Game Kit // https://kybernetik.com.au/platformer // Copyright 2021-2023 Kybernetik //

#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value.

using Animancer;
using PlatformerGameKit.BehaviourTrees;
using UnityEngine;

//这个类就是表现行为树
namespace PlatformerGameKit.Characters.Brains
{
    /// <summary>A <see cref="CharacterBrain"/> that uses a behaviour tree.</summary>
    /// <remarks>
    /// Documentation: <see href="https://kybernetik.com.au/platformer/docs/characters/brains/behaviour">Behaviour Tree Brain</see>
    /// </remarks>
    /// https://kybernetik.com.au/platformer/api/PlatformerGameKit.Characters.Brains/BehaviourTreeBrain
    /// 
    [AddComponentMenu(MenuPrefix + "Behaviour Tree Brain")]
    [HelpURL(APIDocumentation + nameof(BehaviourTreeBrain))]
    public class BehaviourTreeBrain : CharacterBrain
    {
        /************************************************************************************************************************/

        [SerializeReference] private IBehaviourNode _OnAwake;
        [SerializeReference] private IBehaviourNode _OnFixedUpdate;

        /************************************************************************************************************************/

#if UNITY_EDITOR
        /// <summary>[Editor-Only] Initializes default values for the fields.</summary>
        protected virtual void Reset()
        {
            //SetMovementForward 将Character.MovementDirectionX 设置为当前面向的方向
            _OnAwake = new SetMovementForward();
            _OnFixedUpdate = new Selector();
            ((IPolymorphicReset)_OnFixedUpdate).Reset();
        }
#endif

        /************************************************************************************************************************/

        /// <summary>Executes the <see cref="_OnAwake"/> behaviour tree.</summary>
        protected virtual void Awake()
        {
            //把Character作为参数存到Context中，然后在行为树节点里就可以直接引用  var character = Context<Character>.Current;
            using (new Context<Character>(Character))
                _OnAwake?.Execute();
        }

        /************************************************************************************************************************/

        /// <summary>Executes the <see cref="_OnFixedUpdate"/> behaviour tree.</summary>
        protected virtual void FixedUpdate()
        {
            using (new Context<Character>(Character))
                _OnFixedUpdate?.Execute();
        }

        /************************************************************************************************************************/
    }
}
