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
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyUp(KeyCode.A))
        {
            animator.Play("POSE03");
        }

        var animatorStateInfo = animator.GetCurrentAnimatorStateInfo(0);
        
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
