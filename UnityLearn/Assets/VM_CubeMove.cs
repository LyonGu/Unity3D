using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VM_CubeMove : MonoBehaviour {

    public float moveSpeed = 0.5f;

    private Rigidbody myRigidbody;
    // Use this for initialization
    void Start () {
        myRigidbody = gameObject.GetComponent<Rigidbody>();

    }
	
	// Update is called once per frame
	void Update () {
        //if(Input.GetKey(KeyCode.S))
        // 获得输入的H轴和V轴，也就是横轴和纵轴，也就是W、S键或是A、D键的状态。
        float input_h = Input.GetAxisRaw("Horizontal");
        float input_v = Input.GetAxisRaw("Vertical");

        // 输入是一个-1~+1之间的浮点数，把它转化成方向向量
        Vector3 vec = new Vector3(input_h, 0, input_v);

        // 当W键和D键同时按下时，vec会比单按W键要长一些，你可以想想为什么。
        // 所以这里要把输入归一化，无论怎么按键，vec长度都要一致。
        vec = vec.normalized;

        // 乘以moveSpeed可以让调整vec的长度
        vec = vec * moveSpeed;

        // 把vec赋值给刚体的速度，就可以让刚体运动起来了
        myRigidbody.velocity = vec;
    }
}
