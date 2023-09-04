// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2023 Kybernetik //

#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value.

using System;
using UnityEngine;

namespace Animancer.Examples.Basics
{
    /// <summary>Plays an animation to demonstrate the basic usage of Animancer.</summary>
    /// <remarks>
    /// If you actually want to only play one animation on an object and don't need any of the other features of
    /// Animancer, you can use the <see cref="SoloAnimation"/> component to do so without needing an extra script.
    /// </remarks>
    /// <example><see href="https://kybernetik.com.au/animancer/docs/examples/basics/quick-play">Quick Play</see></example>
    /// https://kybernetik.com.au/animancer/api/Animancer.Examples.Basics/PlayAnimationOnEnable
    /// 
    [AddComponentMenu(Strings.ExamplesMenuPrefix + "Basics - Play Animation On Enable")]
    [HelpURL(Strings.DocsURLs.ExampleAPIDocumentation + nameof(Basics) + "/" + nameof(PlayAnimationOnEnable))]
    public sealed class PlayAnimationOnEnable : MonoBehaviour
    {
        /************************************************************************************************************************/

        [SerializeField] private AnimancerComponent _Animancer;
        [SerializeField] private AnimationClip _Animation;
        [SerializeField] private AnimationClip _Animation1;
        /************************************************************************************************************************/

        private AnimancerState _animancerState;
        private void OnEnable()
        {
            AnimancerState state = _Animancer.Play(_Animation1);
            _animancerState = state;
            
            //OnEnd事件 不管动作是否循环，只要时间超过动画长度就会每帧调用
            // state.Events.OnEnd = PlayEnd;

            //循环动画，每次都会触发
            state.Events.Add(0.4f, PlayEnd);
           
        }

        private void PlayEnd()
        {
            Debug.Log($"{Time.frameCount} PlayEnd=================");
            // if (_animancerState != null)
            //     _animancerState.Events.OnEnd = null;
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.A))
            {
                _Animancer.Play(_Animation1);
            }
            
            if (Input.GetKeyDown(KeyCode.S))
            {
                _Animancer.Play(_Animation);
            }
        }

        /************************************************************************************************************************/
    }
}
