using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GirlBaseActionControl : MonoBehaviour {

    public float floWalkingSpeed = 1F;    //走路速度
    public float floRatitionSpeed = 1F;   //旋转速度
    private Animator AnimaObj;
    private Rigidbody myRigidbody;

    // Use this for initialization
    void Start() {
        AnimaObj = this.GetComponent<Animator>();
        myRigidbody = this.GetComponent<Rigidbody>();

    }

    // Update is called once per frame
    void Update() {

        if (Input.GetKey(KeyCode.W))
        {
            UpdateAnimator();
            UpdatePosAndDir(new Vector3(0, 0, 0));
        }
        else if (Input.GetKey(KeyCode.S))
        {
            UpdateAnimator();
            UpdatePosAndDir(new Vector3(0, 180, 0));
        }
        else if (Input.GetKey(KeyCode.A))
        {
            UpdateAnimator();
            UpdatePosAndDir(new Vector3(0, 270, 0));
        }
        else if (Input.GetKey(KeyCode.D))
        {
            UpdateAnimator();
            UpdatePosAndDir(new Vector3(0, 90, 0));
        }
        else
        {
            UpdateAnimator(true);
        }

    }

    void UpdateAnimator(bool isReset = false)
    {
        if (isReset)
        {
            AnimaObj.SetBool("IsRun", false);
            AnimaObj.SetBool("IsWalk", false);
            return;
        }
        AnimaObj.SetFloat("SpWeed", 1);
        if (Input.GetKey(KeyCode.LeftShift))
        {
            AnimaObj.SetBool("IsRun", true);
            AnimaObj.SetBool("IsWalk", false);
        }
        else
        {
            AnimaObj.SetBool("IsRun", false);
            AnimaObj.SetBool("IsWalk", true);
        }
    }

    void UpdatePosAndDir(Vector3 vec3)
    {
        transform.eulerAngles = vec3;
        this.transform.Translate(Vector3.forward * floWalkingSpeed * Time.deltaTime); //默认Space.Self
    }

}
