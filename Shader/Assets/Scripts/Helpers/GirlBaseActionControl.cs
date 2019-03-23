using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GirlBaseActionControl : MonoBehaviour {

    public float floWalkingSpeed = 1F;    //走路速度
    public float floRatitionSpeed = 1F;   //旋转速度
    private Animator AnimaObj;

	// Use this for initialization
	void Start () {
        AnimaObj = this.GetComponent<Animator>();
	}
	
	// Update is called once per frame
	void Update () {

        if(Input.GetKey(KeyCode.W))
        {
            AnimaObj.SetFloat("Speed", 1);
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
            this.transform.Translate(Vector3.forward * floWalkingSpeed);
        }
        else if (Input.GetKey(KeyCode.S))
        {
            AnimaObj.SetFloat("Speed", 1);
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
            this.transform.Translate(Vector3.back * floWalkingSpeed);
        }
        else
        {
            AnimaObj.SetBool("IsRun", false);
            AnimaObj.SetBool("IsWalk", false);
        }

        if (Input.GetKey(KeyCode.A))
        {
            this.transform.Rotate(Vector3.down * floRatitionSpeed);
        }
        else if (Input.GetKey(KeyCode.D))
        {
            this.transform.Rotate(Vector3.up * floRatitionSpeed);
        }


	}
}
