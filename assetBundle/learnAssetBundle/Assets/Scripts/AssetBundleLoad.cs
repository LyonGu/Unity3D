using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AssetBundleLoad : MonoBehaviour {

    public GameObject goCubeChangeTexture1;
    public GameObject goCubeChangeTexture2;

    private string _URL1;
    private string _assetName1;
    private string _assetName2;

    private Coroutine _IELoadNoeObjectFramAB;


    void Awake()
    { 
        //PC端前要加file:://
        //要加载的AB包，一个包里可以放多个资源
        _URL1 = "file://" + Application.streamingAssetsPath + "/texture1"; // 

        //AB包内部资源名称，就是原始资源的名称
        _assetName1 = "unitychan_tile3";
        _assetName2 = "unitychan_tile6";
    }
	// Use this for initialization
	void Start () {
        _IELoadNoeObjectFramAB = StartCoroutine(LoadNoeObjectFramAB(_URL1, goCubeChangeTexture1, _assetName1));
        this.Invoke("testABLoad",2.0f);
        
	}

    void testABLoad()
    {
        StopCoroutine(_IELoadNoeObjectFramAB);
        StartCoroutine(LoadNoeObjectFramAB(_URL1, goCubeChangeTexture2, _assetName2));
    }
    

    //加载 “非GameObject”资源
    /// <summary>
    /// 加载“非GameObject”资源
    /// </summary>
    /// <param name="ABURL">AB包URL</param>
    /// <param name="goShowObj">操作且显示的对象</param>
    /// <param name="assetName">加载资源的名称</param>
    /// <returns></returns>
    IEnumerator LoadNoeObjectFramAB(string ABURL, GameObject goShowObj, string assetName)
    { 
        //参数检查
        if (string.IsNullOrEmpty(ABURL) || goShowObj == null)
        {
            Debug.LogError(GetType() + "/LoadNonObjectFromAB()/输入的参数不合法，请检查");
        }

        using (WWW www = new WWW(ABURL))
        {
            yield return www;
            AssetBundle ab = www.assetBundle;
            if (ab != null)
            {
                if (assetName == "")
                {
                    goShowObj.GetComponent<Renderer>().material.mainTexture = (Texture)ab.mainAsset;
                }
                else
                {
                    goShowObj.GetComponent<Renderer>().material.mainTexture = (Texture)ab.LoadAsset(assetName);
                }

                //卸载资源(只卸载AB包本身)
                ab.Unload(false);
            }
            else
            {
                Debug.LogError(GetType() + "/LoadNonObjectFromAB()/WWW 下载错误，请检查 URL: " + ABURL + " 错误信息:" + www.error);
            }
        }
    }
}
