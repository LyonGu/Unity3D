using System.Collections;
using System.Collections.Generic;
using System.Resources;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class GameMain : MonoBehaviour
{
    // Start is called before the first frame update
    private GameObject myGameObject1;
    private GameObject myGameObject2;
    void Start()
    {

        LoadTest1();
        LoadTest2();

    }

    private void LoadTest1()
    {
        //方法一: Addressables.LoadAssetAsync 加载指定地址的asset, 然后在异步回调里实例化对象
        Addressables.LoadAssetAsync<GameObject>("RedCube").Completed += OnLoadDone1;

        //方法二: Addressables.InstantiateAsync 这个实例化指定地址的asset，并且添加到场景中
        Addressables.InstantiateAsync("BlurCube").Completed += OnLoadDone2;

    }

    private void LoadTest2()
    {
        //Sub-assets 加载子asset  
        //加载一个图集中的所有sprite
        Addressables.LoadAssetAsync<IList<Sprite>>("UICommon").Completed += OnLoadDone3;
    }

    private void OnLoadDone1(UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle<GameObject> obj)
    {
        //只是返回一个资源，需要自己实例化
        myGameObject1 = GameObject.Instantiate(obj.Result);
    }

    private void OnLoadDone2(UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle<GameObject> obj)
    {
        //返回的就是实例化对象
        myGameObject2 = obj.Result;
        myGameObject2.transform.Translate(transform.right * 2,Space.World);
    }

    private void OnLoadDone3(UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle<IList<Sprite>> obj)
    {

        //IList<Sprite> list = obj.Result;
        //foreach (var item in list)
        //{
        //    //Debug.Log($"sprite name is {item.name}");
        //}

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
