
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ResourceMemeroyMsg : MonoBehaviour {

    //非gameObject资源测试
    private Texture _texObj;
    private Sprite _sprObj;
    private Material _materialObj;
    private Material _materialTexObj;
    private Shader _shaderObj;
    private AudioClip _audioObj;
    private AnimationClip _animObj;
    private Mesh _meshObj;


    //gameObject资源: 预设体prefab
    private GameObject _prefabObj;
    private GameObject _gameObj;




	// Use this for initialization
	void Start () {

        //_texObj  = Resources.Load<Texture>("Textures/Floor");
        //_sprObj = Resources.Load<Sprite>("Textures/login_select");  //加载sprite会把对应的纹理也加入内存
        //_materialObj = Resources.Load<Material>("Materials/Blue"); //不带贴图
        //_materialTexObj = Resources.Load<Material>("Materials/Moon"); //带贴图 会把对应的贴图也加入内存
        //_shaderObj = Resources.Load<Shader>("Shaders/SimpleShader");
        //_audioObj = Resources.Load<AudioClip>("Audios/button");
        //_animObj = Resources.Load<AnimationClip>("Animations/avoid");
        //_meshObj = Resources.Load<Mesh>("Mesh/_meshObj"); //尽量少加载mesh，删除不了

        //模型todo
        //虽然是局部变量，但是资源已经加载到内存，需要释放
        Texture _tmptexObj = Resources.Load<Texture>("Textures/Floor");
        Sprite _tmpsprObj = Resources.Load<Sprite>("Textures/login_select");  //加载sprite会把对应的纹理也加入内存
        Material _tmpmaterialObj = Resources.Load<Material>("Materials/Blue"); //不带贴图
        Material _tmpmaterialTexObj = Resources.Load<Material>("Materials/Moon"); //带贴图 会把对应的贴图也加入内存
        Shader _tmpshaderObj = Resources.Load<Shader>("Shaders/SimpleShader");
        AudioClip _tmpaudioObj = Resources.Load<AudioClip>("Audios/button");
        AnimationClip _tmpanimObj = Resources.Load<AnimationClip>("Animations/avoid");
        Mesh _tmpmeshObj = Resources.Load<Mesh>("Mesh/_meshObj"); //尽量少加载mesh，删除不了

        _prefabObj = Resources.Load<GameObject>("Prefabs/Cube");
        _gameObj = Instantiate(_prefabObj);

	}
	
	// Update is called once per frame
	void Update () {
        if (Input.GetKeyDown(KeyCode.D))
        {
            //释放非gameObject资源

            //Resources.UnloadAsset(_texObj); //通过profile查看是否成功 ==>ok
            //Resources.UnloadAsset(_sprObj); //通过profile查看是否成功 ==>ok  **** 只能删除sprite，但是对应的纹理不会删除,需要手动调用Resources.UnloadUnusedAssets();
            //Resources.UnloadAsset(_materialObj); //通过profile查看是否成功 ==>ok
            //Resources.UnloadAsset(_materialTexObj); //通过profile查看是否成功 ==>no 需要手动调用Resources.UnloadUnusedAssets();
            //Resources.UnloadAsset(_shaderObj);     //通过profile查看是否成功 ==>ok
            //Resources.UnloadAsset(_audioObj); //通过profile查看是否成功 ==>ok
            //Resources.UnloadAsset(_animObj); //通过profile查看是否成功 ==>ok
            //Resources.UnloadAsset(_meshObj); //通过profile查看是否成功 ==>no  删除不了？？？


            //释放gameobject资源（prefab）
            _prefabObj = null; //一定要加这句
            Destroy(_gameObj); //释放克隆体内存


            //测试下直接能否删除
            /*
             1 用全局变量缓存非gameObject资源 即前面的_texObj   ==》可以删除
             2 用函数内局部变量缓存非gameObject资源 即前面的tmpTexObj ==》直接调用Resources.UnloadUnusedAssets()可以删除
             */
            Resources.UnloadUnusedAssets();
        }
	}
}
