using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Particle_demo1 : MonoBehaviour {

    //基本粒子系统脚本控制
    public ParticleSystem ps;

    //添加了扩展粒子组件
    //public ParticleEmitter psExtend;

    //复合粒子系统
    public GameObject psCom;


	// Use this for initialization
	void Start () {
        ps.Stop();
        //psExtend.emit = false;
        psCom.SetActive(false);
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    void onTriggerEnter(Collider col)
    {
        //col.gameObject.GetComponent<Collider>().name.Equals("dddddd");
        if(col.name.Equals("dddddd"))
        {
            ps.Play();
        }

        //psExtend.emit = true;
        psCom.SetActive(true);
    }

    void onTriggerExit(Collider col)
    {
        //col.gameObject.GetComponent<Collider>().name.Equals("dddddd");
        if (col.name.Equals("dddddd"))
        {
            ps.Stop();
        }
        //psExtend.emit = false;
        psCom.SetActive(false);
    }
}
