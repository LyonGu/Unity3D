using System.Collections;
using System.Collections.Generic;
using UnityEditor.Animations;
using UnityEngine;

public class GamePlayAnimator : MonoBehaviour
{


    public Animator animator;
    // Start is called before the first frame update
    void Start()
    {
        AnimatorController animatorController = animator.runtimeAnimatorController as AnimatorController;
        AnimatorStateMachine stateMachine = animatorController.layers[0].stateMachine;
        string[] animatorState = new string[stateMachine.states.Length];
        for (int i = 0; i < stateMachine.states.Length; i++)
        {
            animatorState[i] = stateMachine.states[i].state.name;
            Debug.Log($"state name {animatorState[i]}");
        }
        //return animatorState;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
