
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


    private string resourceRefKey = "TestResourceMsg";
    private string plistName = "guanyu";


    private ResourcesManager _resMgr;
    private SpriteAnimatinManger _spAltasMgr; //图集管理
    
    void Awake()
    {
        _resMgr = ResourcesManager.getInstance();
        _spAltasMgr = SpriteAnimatinManger.getInstance();
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
        //_sprObj1 = Resources.Load<Sprite>("Textures/login_select"); //不删除，会一直存在内存中
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

        //testResMsgFram();

        testAltasMsgFram();
        
    }

    //测试资源管理框架
    public void testResMsgFram()
    { 
        //texture
        Texture tex = (Texture)_resMgr.getResouce(ResourceType.Texture, "unitychan_tile3", resourceRefKey);

        //sprite 散图
        Sprite sp = (Sprite)_resMgr.getResouce(ResourceType.Sprite, "login_select", resourceRefKey);

        //material
        Material ma = (Material)_resMgr.getResouce(ResourceType.Material, "Blue", resourceRefKey);

        //material_tex
        Material ma_tex = (Material)_resMgr.getResouce(ResourceType.Material, "Moon", resourceRefKey, true, false);

        //shader
        Shader shader = (Shader)_resMgr.getResouce(ResourceType.Shader, "SimpleShader", resourceRefKey, false, true);

        //audioClip
        AudioClip audioClip = (AudioClip)_resMgr.getResouce(ResourceType.AudioClip, "button", resourceRefKey);

        //animationClip
        AnimationClip animationClip = (AnimationClip)_resMgr.getResouce(ResourceType.AnimationClip, "avoid", resourceRefKey);

        //prefab
        GameObject prefab = (GameObject)_resMgr.getResouce(ResourceType.Prefab, "Cube", resourceRefKey);

    }

    //图集资源测试
    void testAltasMsgFram()
    {
        
        //加载plist后，内存中会有一份texture，还会有所有的sprite
        _spAltasMgr.loadPlistResource(plistName);
        //Sprite sp = _spAltasMgr.getSingleSpriteResource(plistName, "guanyu_01_01_00");
        //_sp1.GetComponent<SpriteRenderer>().sprite = sp;
    }

    void releaseResourceByFram()
    {
        _resMgr.removeResouce(ResourceType.Texture, "unitychan_tile3", resourceRefKey);        //ok
        _resMgr.removeResouce(ResourceType.Sprite, "login_select", resourceRefKey);            //ok
        _resMgr.removeResouce(ResourceType.Material, "Blue", resourceRefKey);                  //ok
        _resMgr.removeResouce(ResourceType.Material, "Moon", resourceRefKey);                  //ok 
        _resMgr.removeResouce(ResourceType.Shader, "SimpleShader", resourceRefKey);            //ok 
        _resMgr.removeResouce(ResourceType.AudioClip, "button", resourceRefKey);               //ok 
        _resMgr.removeResouce(ResourceType.AnimationClip, "avoid", resourceRefKey);            //ok 
        _resMgr.removeResouce(ResourceType.Prefab, "Cube", resourceRefKey);                    //ok
    }

    void releaseAltasResourceByFram()
    {
        //图集没有被引用，直接调用_spAltasMgr.removePlistResource，会删除对应texture以及sprite
        //_spAltasMgr.removePlistResource(plistName);
        //_spAltasMgr.removeAllPlistResource();

        //图集被引用了，首先得删除引用 再删除plist
        //如果不删除引用，则只会删除图集里其他未使用的sprite，纹理和被引用的sprite不会被删除
        //_sp1.GetComponent<SpriteRenderer>().sprite = null;
        //_spAltasMgr.removePlistResource(plistName);

        //_spAltasMgr.removeAllPlistResource();
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

            //使用resourcesMsg框架测试
            //releaseResourceByFram();

            //使用spriteFramManerger框架测试
            releaseAltasResourceByFram();

           
        }
	}
}
