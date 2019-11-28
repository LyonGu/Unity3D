using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

public class GirlBaseActionControl : MonoBehaviour {

    public float floWalkingSpeed = 1F;    //走路速度
    public float floRatitionSpeed = 1F;   //旋转速度
    private Animator AnimaObj;
    private Rigidbody myRigidbody;
    public PlayableDirector playableDirector;
    private Vector3 oldPosition;
    private bool isRun = false;


    // Use this for initialization
    void Start() {
        AnimaObj = this.GetComponent<Animator>();
        myRigidbody = this.GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update() {

        
        if (Input.GetKey(KeyCode.W))
        {
            ResetPlaybleState();
            UpdateAnimator();
            UpdatePosAndDir(new Vector3(0, 0, 0));
        }
        else if (Input.GetKey(KeyCode.S))
        {
            ResetPlaybleState();
            UpdateAnimator();
            UpdatePosAndDir(new Vector3(0, 180, 0));
        }
        else if (Input.GetKey(KeyCode.A))
        {
            ResetPlaybleState();
            UpdateAnimator();
            UpdatePosAndDir(new Vector3(0, 270, 0));
        }
        else if (Input.GetKey(KeyCode.D))
        {
            ResetPlaybleState();
            UpdateAnimator();
            UpdatePosAndDir(new Vector3(0, 90, 0));
        }
        else
        {
            UpdateAnimator(true);
        }
        if (playableDirector != null && playableDirector.state != PlayState.Playing)
        {
            oldPosition = transform.localPosition;
            transform.localPosition = new Vector3(oldPosition.x, 0, oldPosition.z);
        }

    }

    void UpdateAnimator(bool isReset = false)
    {
        isRun = false;
        if (isReset)
        {
            AnimaObj.SetBool("IsRun", false);
            AnimaObj.SetBool("IsWalk", false);
            return;
        }
        AnimaObj.SetFloat("Speed",1);
        if (Input.GetKey(KeyCode.LeftShift))
        {
            isRun = true;
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
        float walkSpedd = floWalkingSpeed;
        if (isRun)
        {
            walkSpedd *= 3.0f;
        }
            
        this.transform.Translate(Vector3.forward * walkSpedd * Time.deltaTime); //默认Space.Self
        //oldPosition = transform.localPosition;
        //transform.localPosition = new Vector3(oldPosition.x, 0, oldPosition.z);
        //transform.localPosition = 
    }

    void ResetPlaybleState()
    {
        if (playableDirector != null && playableDirector.state == PlayState.Playing)
        {
            playableDirector.Stop();
        }
    }


}
