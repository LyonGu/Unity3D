
/***
 *
 *  Title: "Guardian" 项目
 *         描述：
 *
 *  Description:
 *        功能：
 *       
 *
 *  Date: 2019
 * 
 *  Version: 1.0
 *
 *  Modify Recorder:
 *     
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TestResourceMsg : MonoBehaviour {


    //测试2，删除GameObject的时候，是否也会删除对应资源内存(texture,material...) ==>删除gameObject，对应的资源内存不会删除

    public GameObject _cube;
    public GameObject _cube1;


    private ResourcesManager _resMgr;
    
    void Awake()
    {
        _resMgr = ResourcesManager.getInstance();
    }
	// Use this for initialization
	void Start () {
        Texture tex = (Texture)_resMgr.getResouce(ResourceType.Texture, "floor", "TestResourceMsg");
        _cube.GetComponent<Renderer>().material.mainTexture = tex;

        //再次调用getResouce 从缓存里获取资源内存
        //Texture tex1 = (Texture)_resMgr.getResouce(ResourceType.Texture, "floor", "TestResourceMsg");
        //_cube1.GetComponent<Renderer>().material.mainTexture = tex1;

        //如果直接使用tex，需要手动添加引用计数管理
        _cube1.GetComponent<Renderer>().material.mainTexture = tex;
        _resMgr.addResourceRefCount(ResourceType.Texture, "floor", "TestResourceMsg");
	}
	
	// Update is called once per frame
	void Update () {
        if (Input.GetKeyDown(KeyCode.D))
        {
            //Destroy(_cube);
            //Destroy(_cube1);
            //_resMgr.removeResouce(ResourceType.Texture, "floor", "TestResourceMsg");
            _resMgr.removeResoucesByView("TestResourceMsg");
        }
	}
}
