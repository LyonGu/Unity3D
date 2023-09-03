// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2023 Kybernetik //

#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value.

using UnityEngine;

namespace Animancer.Examples.StateMachines
{
    /// <summary>
    /// Takes the root motion from the <see cref="Animator"/> attached to the same <see cref="GameObject"/> and applies
    /// it to a <see cref="Rigidbody"/> on a different object.
    /// </summary>
    /// <example><see href="https://kybernetik.com.au/animancer/docs/examples/fsm/weapons">Weapons</see></example>
    /// https://kybernetik.com.au/animancer/api/Animancer.Examples.StateMachines/RootMotionRedirect
    /// 
    [AddComponentMenu(Strings.ExamplesMenuPrefix + "Brains - Root Motion Redirect")]
    [HelpURL(Strings.DocsURLs.ExampleAPIDocumentation + nameof(StateMachines) + "/" + nameof(RootMotionRedirect))]
    public sealed class RootMotionRedirect : MonoBehaviour
    {
        /************************************************************************************************************************/

        [SerializeField] private Rigidbody _Rigidbody;
        [SerializeField] private Animator _Animator;

        /************************************************************************************************************************/
        
        //用于处理动画移动以修改根运动的回调。 该回调在处理完状态机和动画后 （但在 OnAnimatorIK 之前）的每个帧中调用。
        //该方法会禁用Animator的rootMotion，需要手动调用 _Animator.ApplyBuiltinRootMotion();
        private void OnAnimatorMove()
        {
            if (_Animator.applyRootMotion)
            {
                _Rigidbody.MovePosition(_Rigidbody.position + _Animator.deltaPosition);
                _Rigidbody.MoveRotation(_Rigidbody.rotation * _Animator.deltaRotation);
                // _Animator.ApplyBuiltinRootMotion();
            }
        }

        /************************************************************************************************************************/
    }
}
