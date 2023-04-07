using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimatorTest : MonoBehaviour
{

    public Animator animator;

    private int JUMP00BFullHash = Animator.StringToHash("Base Layer.JUMP00B");
    private int JUMP01BFullHash = Animator.StringToHash("Base Layer.JUMP01B");
    // Start is called before the first frame update
    void Start()
    {
        AnimationClip[] ans = animator.runtimeAnimatorController.animationClips;
        for (int i = 0; i < ans.Length; i++)
        {
            var an = ans[i];
            Debug.Log($"animationClip {an.name}  {an.length}");
        }
        
        //必须是AnimatorOverrideController才行
        // var runtimeAnamator = animator.runtimeAnimatorController as AnimatorOverrideController;
        // var JUMP00B = runtimeAnamator["JUMP00B"]; // <originClip, overrideClip> ==> 拿到的是overrideClip  key为资源的名字
        // if (JUMP00B)
        // {
        // 	Debug.Log($"JUMP00B ClipTime is========{JUMP00B.length}");
        // }
        // animator.layer

        // Vector3 dir1 = transform.right;
        var propertyBlock = new MaterialPropertyBlock();
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyUp(KeyCode.A))
        {
            animator.Play("POSE03");
        }

        var animatorStateInfo = animator.GetCurrentAnimatorStateInfo(0);
        
        
        // ** 如果状态中有过渡，处于过渡状态中GetCurrentAnimatorStateInfo返回的结果仍然是切换前的状态
        //用hash比较需要完整的路径hash 才能得出当前状态
        int hash = animatorStateInfo.fullPathHash;
        int hash1 = animatorStateInfo.nameHash;
        if (hash == JUMP00BFullHash)
        {
            Debug.Log($"{Time.frameCount} curstate is JUMP00B==========");
        }
        
        if (hash == JUMP01BFullHash)
        {
            Debug.Log($"{Time.frameCount} curstate is JUMP01B==========");
        }

        // var transitionInfo = animator.GetAnimatorTransitionInfo(0);
        // Debug.Log($"{Time.frameCount} transitionInfo time =========={transitionInfo.duration} {transitionInfo.durationUnit}");


        //用IsName比较能得出当前状态 ok
        // if (animatorStateInfo.IsName("JUMP00B"))
        // {
        //     Debug.Log($"{Time.frameCount} curstate is JUMP00B==========");
        // }
        //
        // if (animatorStateInfo.IsName("JUMP01B"))
        // {
        //     Debug.Log($"{Time.frameCount} curstate is JUMP01B==========");
        // }


    }
}
