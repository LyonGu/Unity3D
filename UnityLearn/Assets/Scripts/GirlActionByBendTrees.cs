using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GirlActionByBendTrees : MonoBehaviour {

    public float animSpeed = 1.5f;
    public float forwardSpeed = 1.5f;
    public float backSpeed = 1.5f;
    public float rotateSpeed = 1.5f;

    private Vector3 velocity;
    private Animator animObj;




	// Use this for initialization
	void Start () {
		animObj = this.gameObject.GetComponent<Animator>();
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    void FixedUpdate()
    {
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        animObj.SetFloat("Speed",v);
        animObj.SetFloat("Direction", h);
        animObj.speed = animSpeed;
        velocity = new Vector3(0, 0, v);
        velocity = transform.TransformDirection(velocity); //转到世界坐标

        //确定前进或者后退
        if (v > 0.1)
        {
            velocity *= forwardSpeed; //前进移动速度
        }
        else if(v <=-0.1)
        {
            velocity *= backSpeed;
        }
        transform.localPosition += velocity * Time.fixedDeltaTime;
        transform.Rotate(0, h * rotateSpeed, 0);
        
    }
}
