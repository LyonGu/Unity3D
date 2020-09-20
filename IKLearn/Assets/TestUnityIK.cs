using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestUnityIK : MonoBehaviour
{
    public Transform lookAtTarget;
    public Transform leftHandTarget;
    public Transform rightHandTarget;
    public Transform leftFootTarget;
    public Transform rightFootTarget;

    private Animator _animator;
    // Start is called before the first frame update
    void Start()
    {
        _animator = this.GetComponent<Animator>();
    }

    // Update is called once per frame
    //void Update()
    //{

    //}

    /*
     IK可以设置5个部位：头、左右手、左右脚
     */
    void OnAnimatorIK(int layerIndex)
    {
        if (_animator != null)
        {
           
            if (lookAtTarget != null)
            {
                //仅仅是头部跟着变动
                _animator.SetLookAtWeight(1);

                //身体也会跟着转, 弧度变动更大
                //_animator.SetLookAtWeight(1, 1, 1, 1);
                _animator.SetLookAtPosition(lookAtTarget.position);
            }

            if (leftHandTarget != null)
            {
                _animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, 1); //设置位置权重
                _animator.SetIKPosition(AvatarIKGoal.LeftHand, leftHandTarget.position);


                _animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, 1); //设置旋转权重
                _animator.SetIKRotation(AvatarIKGoal.LeftHand, leftHandTarget.rotation);
            }

            if (rightHandTarget != null)
            {
                _animator.SetIKPositionWeight(AvatarIKGoal.RightHand, 1); //设置位置权重
                _animator.SetIKPosition(AvatarIKGoal.RightHand, rightHandTarget.position);


                _animator.SetIKRotationWeight(AvatarIKGoal.RightHand, 1); //设置旋转权重
                _animator.SetIKRotation(AvatarIKGoal.RightHand, rightHandTarget.rotation);
            }

            if (leftFootTarget != null)
            {
                _animator.SetIKPositionWeight(AvatarIKGoal.LeftFoot, 1); //设置位置权重
                _animator.SetIKPosition(AvatarIKGoal.LeftFoot, leftFootTarget.position);


                _animator.SetIKRotationWeight(AvatarIKGoal.LeftFoot, 1); //设置旋转权重
                _animator.SetIKRotation(AvatarIKGoal.LeftFoot, leftFootTarget.rotation);
            }

            if (rightFootTarget != null)
            {
                _animator.SetIKPositionWeight(AvatarIKGoal.RightFoot, 1); //设置位置权重
                _animator.SetIKPosition(AvatarIKGoal.RightFoot, rightFootTarget.position);


                _animator.SetIKRotationWeight(AvatarIKGoal.RightFoot, 1); //设置旋转权重
                _animator.SetIKRotation(AvatarIKGoal.RightFoot, rightFootTarget.rotation);
            }
        }
    }
}
