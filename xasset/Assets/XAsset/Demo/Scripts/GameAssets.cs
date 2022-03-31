using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using DXGame.structs;
using Game;
using libx;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using GameLog;

public class GameAssets : MonoBehaviour
{
	public Dropdown dropdown;
	public Image temp;
    public Slider hotUpdateTestSlider;
	private string[] _assets;
	private int _optionIndex;

	List<GameObject> _gos = new List<GameObject> ();
	List<AssetRequest> _requests = new List<AssetRequest> ();

    public RawImage urlRawImage;
    public RawImage localRawImage;

    public Text localTxt;
    public Transform TestRootTrsTransform;
	public void OnLoad ()
	{
		StartCoroutine (LoadAsset ());
	}

	AssetRequest LoadSprite (string path)
	{
		var request = Assets.LoadAsset (path, typeof(Sprite));
		_requests.Add (request);
		return request;
	}

    AssetRequest LoadSpriteAsync(string path)
    {
        var request = Assets.LoadAssetAsync(path, typeof(Sprite));
        _requests.Add(request);
        return request;
    }

    AssetRequest LoadGameObjectAsync(string path)
	{
		path = Assets.GetAssetPathByName(path);
		var request = Assets.LoadAssetAsync(path, typeof(GameObject));
		_requests.Add(request);
		return request;
	}


    AssetRequest LoadSceneAsync(string path)
    {
	    var request = Assets.LoadSceneAsync(path, false);
        _requests.Add(request);
        return request;
    }

    AssetRequest LoadGameObject(string path)
    {
	    path = Assets.GetAssetPathByName(path);
		var request = Assets.LoadAsset(path, typeof(GameObject));
		_requests.Add(request);
		return request;
	}

	public void OnLoadAll ()
	{ 
		StartCoroutine (LoadAll (_assets.Length));
	}

    public void OnLoadAllAsync()
    {
        StartCoroutine(LoadAllAsync(_assets.Length));
    }

    public void OnLoadScenelAsync()
    {
        string path = _assets[_optionIndex];
        if (string.IsNullOrEmpty(path)) return;
        var ext = Path.GetExtension(path);
        if (ext.Equals(".unity", StringComparison.OrdinalIgnoreCase))
                StartCoroutine(LoadSceneAsync(path));
    }

    IEnumerator LoadAll (int size)
	{
		var count = 0; 
		List<AssetRequest> list = new List<AssetRequest> ();
		for (int i = _optionIndex; i < _assets.Length; i++) {
			var asset = _assets [i];
			var ext = Path.GetExtension (asset);
			if (count >= size) {
				_optionIndex = i; 
				break;
			}
			if (ext.Equals (".png", StringComparison.OrdinalIgnoreCase)) {
				var request = LoadSprite (asset);
				request.completed += OnCompleted;  
				list.Add (request); 
				count++;
			}
		}
		yield return new WaitUntil (() => list.TrueForAll (o => {
			return o.isDone;
		}));
	}

    private bool _isLoadAllAsync = false;
    IEnumerator LoadAllAsync(int size)
    {
        _isLoadAllAsync = true;
        var count = 0;
        List<AssetRequest> list = new List<AssetRequest>();
        for (int i = _optionIndex; i < _assets.Length; i++)
        {
            var assetPath = _assets[i];
            var ext = Path.GetExtension(assetPath);
            if (count >= size)
            {
                _optionIndex = i;
                break;
            }
            if (ext.Equals(".png", StringComparison.OrdinalIgnoreCase) && assetPath.IndexOf("Demo/UI/") > -1)
            {
                var request = LoadSpriteAsync(assetPath);
                request.completed += OnCompleted;
                list.Add(request);
                count++;
            }
        }
        yield return new WaitUntil(() => list.TrueForAll(o =>
        {
            return o.isDone;
        }));
    }

    private void OnCompleted (AssetRequest request)
	{
        if (request == null)
            return;
		if (!string.IsNullOrEmpty (request.error)) {
			request.Release ();
			return;
		}
        if (request.asset == null)
            return;
		var go = Instantiate (temp.gameObject, temp.transform.parent);
		go.SetActive (true);
		go.name = request.asset.name;
		var image = go.GetComponent<Image> ();
		image.sprite = request.asset as Sprite;
		_gos.Add (go);
	}

	private IEnumerator LoadAsset ()
	{
		if (_assets == null || _assets.Length == 0) {
			yield break;
		}
        //根据当前下拉框选择的值去选取AB路径
        var path = _assets [_optionIndex];
        //获取拓展名
        var ext = Path.GetExtension (path);

        if (ext.Equals (".png", StringComparison.OrdinalIgnoreCase) && path.IndexOf("Demo/UI/")>-1) {
            //拿着这个路径去加载精灵图片
            var request = LoadSprite (path);
			yield return request;
			if (!string.IsNullOrEmpty (request.error)) {
				request.Release ();
				yield break;
			}
            //实例化
            var go = Instantiate (temp.gameObject, temp.transform.parent);
			go.SetActive (true);
			go.name = request.asset.name;
			var image = go.GetComponent<Image> ();
            //设置从AB加载出来的精灵图片
            image.sprite = request.asset as Sprite; 
			_gos.Add (go);
		}
	}

	public void OnUnload ()
	{
		_optionIndex = 0;
		StartCoroutine (UnloadAssets ());
	}

	private IEnumerator UnloadAssets ()
	{
		foreach (var image in _gos) {
			DestroyImmediate (image);
		}
		_gos.Clear ();
        
		foreach (var request in _requests) {
            //减少引用计数
            request.Release ();
		}

		_requests.Clear ();
		yield return null; 
	}

	private void Awake()
	{
		var gameRoot = Resources.Load<GameObject>("GameRoot");
		var rootObj = Instantiate(gameRoot);
		rootObj.name = "GameRoot";
		DontDestroyOnLoad(rootObj);
	}

	// Use this for initialization
	void Start ()
	{
        LogUtils.InitSettings();

        dropdown.ClearOptions ();
		_assets = Assets.GetAllAssetPaths ();
		foreach (var item in _assets) {
			var ext = Path.GetExtension(item);
			dropdown.options.Add (new Dropdown.OptionData (item));
		}

		dropdown.onValueChanged.AddListener (OnDropdown);


        Test();

        LogUtils.ColorLog(LogColor.Green,$"assts==========={_assets.Length}  {Application.persistentDataPath}");
        LogUtils.Warn($"assts==========={_assets.Length}  {Application.persistentDataPath}");
        LogUtils.Error($"assts==========={_assets.Length}  {Application.persistentDataPath}");
	}
    #region 测试下一些常用接口

    async Task RunAwaitSecondsTestAsync()
    {
	    LogUtils.Log("RunAwaitSecondsTestAsync Waiting 1 second...");
	    await new WaitForSeconds(1.0f);
	    AssetsMgr.PoolGetGameObject("FootmanHP", (gObj) =>
        {
	        gObj.SetActive(true);
	        gObj.name = "AssetsMgr_HotTestAyncPoolGet";
        }, TestRootTrsTransform,11);
    }
    
    private void Test()
    {
		//同步加载
        var abRequest = LoadGameObject("FootmanHP");
        _requests.Add(abRequest);
        var goSync = Instantiate(abRequest.asset) as GameObject;
        goSync.SetActive(true);
        goSync.name = "HotTestSync";
        
        AssetsMgr.Load<GameObject>("FootmanHP", (obj) =>
        {
	        var goSync1 = Instantiate(obj);
	        goSync1.SetActive(true);
	        goSync1.name = "AssetsMgr_HotTestSync";
        });
        
        AssetsMgr.LoadAsyn<GameObject>("FootmanHP", (obj) =>
        {
	        var goSync1 = Instantiate(obj);
	        goSync1.SetActive(true);
	        goSync1.name = "AssetsMgr_HotTestAync";
        });


        AssetsMgr.CreatePoolGameObject("FootmanHP", 10, () =>
        {
	        LogUtils.Log("AssetsMgr.CreatePoolGameObject Done=====FootmanHP");
			
	     
//	        RunAwaitSecondsTestAsync();
	        AssetsMgr.PoolGetGameObject("FootmanHP", (gObj) =>
	        {
		        gObj.SetActive(true);
		        gObj.name = "AssetsMgr_HotTestAyncPoolGet";
	        }, TestRootTrsTransform,11);
        });
        
//        DXQueue<int> testQueue = new DXQueue<int>(10); 
//        testQueue.Enqueue(1);
//        testQueue.Enqueue(2);
//        testQueue.Enqueue(3);
//        testQueue.Enqueue(4);
//
//        for (int i = 0; i < testQueue.Count; i++)
//        {
//	        int value = testQueue.Dequeue();
//	        LogUtils.Log($"DXQueue  value==========={value}");
//	        i--;
//        }

        //异步加载
        var abRequestAsync = LoadGameObjectAsync("FootmanHP");
        _requests.Add(abRequestAsync);
        abRequestAsync.completed += (AssetRequest request) =>
        {
            if (!string.IsNullOrEmpty(request.error))
            {
                request.Release();
                return;
            }
            var go = Instantiate(request.asset) as GameObject;
            go.SetActive(true);
            go.name = "HotTestAsync";
            go.transform.position = new Vector3(2, 0, 0);

        };

        //加载进度
        /*
            从Bundle中加载始终返回的是BundleAssetRequest或者BundleAssetRequestAsync
            1 同步加载  AssetRequest上progress始终为1 （BundleAssetRequest）
            2 异步加载  AssetRequest上progress ==》 BundleAssetRequestAsync
        */

        // animationClip 嵌入到gameObject上 OK
        // altas 测下 ==》OK  图集不打AB，对应的散图打成一个AB

        //打了一个资源模型 含有fbx texture material mesh 都通过了


        //场景加载 OK
        //var scene = Assets.LoadSceneAsync(gameScene, false);

        //网络加载 内部使用的是UnityWebRequest封装的
        /*
           if (path.StartsWith("http://", StringComparison.Ordinal) ||
                path.StartsWith("https://", StringComparison.Ordinal) ||
                path.StartsWith("file://", StringComparison.Ordinal) ||
                path.StartsWith("ftp://", StringComparison.Ordinal) ||
                path.StartsWith("jar:file://", StringComparison.Ordinal))
         */

        //网络资源  测试OK
        string url = "https://ss0.baidu.com/94o3dSag_xI4khGko9WTAnF6hhy/zhidao/pic/item/8326cffc1e178a82b13fb3d1f703738da977e844.jpg";
        var trequest = Assets.LoadAssetAsync(url, typeof(Texture2D));
        trequest.completed += (AssetRequest request) =>
        {
            var tex = request.asset as Texture2D;
            urlRawImage.texture = tex;
        };
        
        


        //本地资源 
        string path = Updater.GetStreamingAssetsPath() + "/hotUpdateTemp.jpg";
        var filerequest = Assets.LoadAssetAsync(path, typeof(Texture2D));
        filerequest.completed += (AssetRequest request) =>
        {
            var tex = request.asset as Texture2D;
            localRawImage.texture = tex;
        };

        path = Updater.GetStreamingAssetsPath() + "/Title.txt";
  
        var textrequest = Assets.LoadAssetAsync(path, typeof(TextAsset));
        textrequest.completed += (AssetRequest request) =>
        { 
            string str = request.text;
            localTxt.text = str;
            Debug.Log($"Title.txt 内容是=={str}");
        };


        //读取lua.txt
        //var luarequest = Assets.LoadAsset("Assets/Games/Lua/TestBuildBundle.lua.txt", typeof(TextAsset));
        //TextAsset asset = luarequest.asset as TextAsset;
        //byte[] luaBytes = asset.bytes;
        //string luastr1 = System.Text.Encoding.UTF8.GetString(luaBytes);
        //string luastr = asset.text;
        //Debug.Log($"TestBuildBundle.lua.txt 内容是=={luastr1}");
        //

//            StartCoroutine(TestLoadLua());
    }

      //向服务器请求版本信息
//        private IEnumerator TestLoadLua()
//        {
//            
//
//            //把服务器版本文件下载到本地，版本文件里记录的所有文件列表
//            string remoteVerPath = Assets.baseURL + "/Lua/Main.lua";
//            string _savePathDir = string.Format("{0}/DLC/Lua/", Application.persistentDataPath);
//            if (!Directory.Exists(_savePathDir))
//	            Directory.CreateDirectory(_savePathDir);
//           
//            string savaPath = _savePathDir + "Main.lua";
//            if(File.Exists(savaPath))
//	            File.Delete(savaPath);
//            var luarequest = UnityWebRequest.Get(remoteVerPath);//加载资源服务器文件
//            luarequest.downloadHandler = new DownloadHandlerFile(savaPath); //设置本地文件存储路径
//            yield return luarequest.SendWebRequest();
//            var error = luarequest.error;
//            if(!string.IsNullOrEmpty(error))
//	            Debug.LogError($"下载lua文件失败 error=== {error}");
//            var textrequest = Assets.LoadAssetAsync("file://" + savaPath, typeof(TextAsset));
//            textrequest.completed += (AssetRequest request) =>
//            {
//	            string str = request.text;
//	            Debug.Log($"Main.lua 内容是=={str}");
//	            textrequest.Release();
//	            
//	            //测试lua 文件
//	           LuaManager.StartLua();
//
//            };
//
//            luarequest.Dispose();
//
//
//        }
        

    private void Update()
    {
        if (!_isLoadAllAsync) return;
        if (_isLoadAllAsync)
        {
            int count = _requests.Count;
            if (count == 0)
            {
                hotUpdateTestSlider.value = 0;
                return;
            }
            //根据数量来标识进度  ==> 用数量来表现感觉更好
            int doneCount = 0;
            for (int i = 0; i < count; i++)
            {
                var request = _requests[i];
                if (request.isDone)
                    doneCount++;
            }
            float pro = (float)doneCount / count;
            ///Debug.Log($"curProgress == {pro} {doneCount} {count}");
            hotUpdateTestSlider.value = pro;

            //根据request的progr来标识进度

            //float rPro = 1.0f / count;
            //float curProgress = 0f;
            //for (int i = 0; i < count; i++)
            //{
            //    var request = _requests[i];
            //    //if (request.isDone)
            //    //    doneCount++;
            //    curProgress += request.progress * rPro;
            //}
            //Debug.Log($"curProgress == {curProgress}");
            //hotUpdateTestSlider.value = curProgress;
        }
    }
    #endregion

    private void OnDropdown (int index)
	{
		_optionIndex = index;
	}
}