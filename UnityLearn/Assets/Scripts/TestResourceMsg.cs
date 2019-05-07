
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

    private Texture _texObj;
    private Sprite _sprObj;
    private Sprite _sprObj1;
    private Material _materialObj;
    private Material _materialTexObj;
    private Shader _shaderObj;
    private AudioClip _audioObj;
    private AnimationClip _animObj;

    private GameObject _prefabObj;
    private GameObject _gameObj;

    //测试2，删除GameObject的时候，是否也会删除对应资源内存(texture,material...) ==>删除gameObject，对应的资源内存不会删除

    public GameObject _cube;
    public GameObject _cube1;
    public GameObject _sp;
    public GameObject _sp1;




    private ResourcesManager _resMgr;
    private SpriteAnimatinManger _spAltasMgr; //图集管理
    
    void Awake()
    {
        _resMgr = ResourcesManager.getInstance();
    }
	// Use this for initialization
	void Start () {

        //测试0: 加载同一个资源两次，内存里是否会有两份资源内存 ==> 不会，只会有一份资源内存
        //Texture tex = Resources.Load<Texture>("Textures/floor");
        //Texture tex1 = Resources.Load<Texture>("Textures/floor");


        //Texture tex = (Texture)_resMgr.getResouce(ResourceType.Texture, "floor", "TestResourceMsg");
        //_cube.GetComponent<Renderer>().material.mainTexture = tex;

        ////再次调用getResouce 从缓存里获取资源内存
        //Texture tex1 = (Texture)_resMgr.getResouce(ResourceType.Texture, "floor", "TestResourceMsg");
        //_cube1.GetComponent<Renderer>().material.mainTexture = tex1;

        //如果直接使用tex，需要手动添加引用计数管理
        //_cube1.GetComponent<Renderer>().material.mainTexture = tex;
        //_resMgr.addResourceRefCount(ResourceType.Texture, "floor", "TestResourceMsg");

        //图集测试 使用SpriteAnimatinManger

        /**/

        

        //测试1：加载单个texture，以及加载单个sprite（同一个资源既可以是texture也可以sprite，unity支持这样的操作转换）
        //_texObj = Resources.Load<Texture>("Textures/Floor");
        //_sprObj = Resources.Load<Sprite>("Textures/Floor");

        //不管里全局变量还是局部变量，不删除资源，会一直保存在内存中
        _sprObj1 = Resources.Load<Sprite>("Textures/login_select"); //不删除，会一直存在内存中
        //Sprite t_sprObj1 = Resources.Load<Sprite>("Textures/login_select"); //不删除，会一直存在内存中
        /*
         * 同一个资源既可以是texture也可以sprite
         * 
            1 Resources.UnloadAsset 删除texture，是否会删除sprite资源，==> 不会删除sprite资源
         * {
         *      是否会相关GameObject有影响 
             *      使用了texture的GameObject ==》有影响，丢失了贴图
             *      使用了sprite的GameObject  ==》有影响，GameObject会不显示
         * }
         *  2 Resources.UnloadAsset 删除sprite， 是否会删除texure资源 ==> 不会删除texture资源也不会删除自己
         *   {
         *      是否会相关GameObject有影响 
             *      使用了texture的GameObject ==》无影响
             *      使用了sprite的GameObject  ==》无影响
         * }
         */

        //贴图以及精灵测试
        //_cube.GetComponent<Renderer>().material.mainTexture = _texObj;
        //_sp.GetComponent<SpriteRenderer>().sprite = _sprObj;

       // _sp1.GetComponent<SpriteRenderer>().sprite = _sprObj1;


        //材质测试: 不带贴图，带贴图
        //_materialObj = Resources.Load<Material>("Materials/Blue");
        //_cube.GetComponent<Renderer>().material = _materialObj;

        //_materialTexObj = Resources.Load<Material>("Materials/Moon");
        //_cube.GetComponent<Renderer>().material = _materialTexObj;


        //shader 音频 动画
        //_shaderObj = Resources.Load<Shader>("Shaders/SimpleShader");
        //_audioObj = Resources.Load<AudioClip>("Audios/button");
        //_animObj = Resources.Load<AnimationClip>("Animations/avoid");

        //_materialObj.shader = _shaderObj;
        //_cube.GetComponent<Renderer>().material = _materialObj;

        //AudioSource audioSource = this.GetComponent<AudioSource>();
        //audioSource.clip = _audioObj;
        //audioSource.loop = true;
        //audioSource.Play();

        //Animation animation = this.GetComponent<Animation>();
        //animation.AddClip(_animObj, _animObj.name);

        //预设体
        //_prefabObj = Resources.Load<GameObject>("Prefabs/Cube");
       // _gameObj = Instantiate(_prefabObj);

        testResMsg();
        
    }

    //测试资源管理框架
    public void testResMsg()
    { 
    
    }
	
	// Update is called once per frame
	void Update () {
        if (Input.GetKeyDown(KeyCode.D))
        {
            //Destroy(_cube);
            //Destroy(_cube1);
            //_resMgr.removeResouce(ResourceType.Texture, "floor", "TestResourceMsg");
            //_resMgr.removeResoucesByView("TestResourceMsg");

            Debug.Log("释放资源============");

            //释放sprite
            //_sp1.GetComponent<SpriteRenderer>().sprite = null;//Destroy(_sp1);
            //Texture tex = _sprObj1.texture;
            //Resources.UnloadAsset(tex);
            //Resources.UnloadAsset(_sprObj1); //单纯是靠引用计数来删除的

            

            //释放材质
            //_cube.GetComponent<Renderer>().material = null;//Destroy(_cube);
            //Resources.UnloadAsset(_materialObj);


            //_cube.GetComponent<Renderer>().material = null;//Destroy(_cube)
            //Texture tex = _materialTexObj.mainTexture;
            //Resources.UnloadAsset(tex);
            //Resources.UnloadAsset(_materialTexObj); 

            //释放shader 音频 动画
            //Resources.UnloadAsset(_shaderObj); 
            //Resources.UnloadAsset(_audioObj);
            //Resources.UnloadAsset(_animObj);

            //释放预设体
            //_prefabObj = null;
            //Resources.UnloadUnusedAssets(); 
           
        }
	}
}
