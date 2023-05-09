using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class DoTweenTest : MonoBehaviour
{

    public Transform StartTrans;

    public Transform EndTrans;

    public Transform TargetTrans;
    // Start is called before the first frame update
    void Start()
    {
       
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            TargetTrans.position = StartTrans.position;
            Debug.Log("JumpStart==============");
            Tween tween = TargetTrans.DOJump(EndTrans.position, 5, 1, 2.0f, false);
            tween.onComplete = () =>
            {
                Debug.Log("JumpEnd==============");
            };
            tween.SetAutoKill(true); //onComplete后自动销毁
        }
    }
}
