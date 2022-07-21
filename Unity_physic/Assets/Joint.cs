using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Joint : MonoBehaviour
{
    private HingeJoint _joint;
    private JointMotor _motor;

    // Use this for initialization
    void Start () {

        Debug.Log("Joint===========Start");
        _joint = GetComponent<HingeJoint>();
        _motor = _joint.motor;

    }
	
    // Update is called once per frame
    void Update () {

        if (Input.GetMouseButtonDown(0))
        {
            _joint.useMotor = true;   
            _motor.force = 100;
            _motor.freeSpin = false ;
            _motor.targetVelocity = 100; 
            _joint.motor = _motor;    //这句代码一定不能掉了，因为运行到这一句之前HingeJoint类实例中的_joint.motor变量还一直没有赋值

            // _joint.useSpring = true;
            // var spring = _joint.spring;
            // spring.targetPosition = 90;
            // _joint.spring = spring;
            //
            // ConstantForce constantForce =  this.gameObject.GetComponent<ConstantForce>();
            // if(constantForce == null)
            //     constantForce = this.gameObject.AddComponent<ConstantForce>();
            //
            // // constantForce
            // constantForce.force = new Vector3(0,0,1000);
        }
    }
}
