/***
 *
 *  Title: 
 *         第28章:  网络基础
 *                 
 *
 *  Description:
 *        功能：
 *            WWW 下载类的学习 
 *
 *  Date: 2017
 * 
 *  Version: 1.0
 *
 *  Modify Recorder:
 *     
 */
using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using UnityEngine.UI;

public class WWWDemo1 : MonoBehaviour{
    public GameObject goCube;                              //立方体
    public GameObject goSphere;                            //球体
    public RawImage rawImage;
    private Texture2D TxtDownloadTextures;                 //需要下载的贴图


    //下载本机贴图
    public void DownLoadTexturesByWWW(){
        StartCoroutine("StartDownLoadTexture");

        Sprite t;
        RawImage ts;
    }

    private void DownHttpCompleteHandle(AsyncOperation t)
    {
        if (t.isDone)
        {
            var Operation = (UnityWebRequestAsyncOperation)t;
            var request = Operation.webRequest;
            var myTexture = DownloadHandlerTexture.GetContent(request);
            rawImage.texture = myTexture;
        }
    }
    //Action<AsyncOperation> completed
    //从互联网下载贴图
    public void DownLoadTexturesFromHTTP(){

        //WWW www = new WWW("http://imageserver.uniregistry.com/catimg/computersinternetdownloads/c3.jpg");
        //while (!www.isDone) { }
        //goSphere.GetComponent<Renderer>().material.mainTexture = www.texture;
        //StartCoroutine("StartDownLoadTextureFromHTTP");

        //StartCoroutine(GetTexture());

        string url = "http://imageserver.uniregistry.com/catimg/computersinternetdownloads/c3.jpg";
        UnityWebRequest uwr = UnityWebRequestTexture.GetTexture(url);
        {
            uwr.SendWebRequest().completed += delegate (AsyncOperation t)
            {
                if (t.isDone)
                {
                    Debug.Log("GetURLImage done===========");
                    var myTexture = DownloadHandlerTexture.GetContent(uwr);
                    rawImage.texture = myTexture;
                }
            };
        }
        

    }

    //下载本机资源
    IEnumerator StartDownLoadTexture(){
        //定义本机资源
        WWW loadloadTexture = 
            new WWW("file://" + Application.dataPath + "/Resources/Textures/DarkFloor.jpg");
        //等待下载
        yield return loadloadTexture;
        //下载的贴图直接赋值指定的游戏对象
        goCube.GetComponent<Renderer>().material.mainTexture = loadloadTexture.texture;
    }

    //从互联网下载资源
    IEnumerator StartDownLoadTextureFromHTTP(){
        //定义本机资源（注意： 如果WWW 对应URL链接资源失效，请自行更换一个有效地址即可，不影响本示例演示效果）
        WWW loadloadTexture =
            new WWW("http://a2.qpic.cn/psb?/V13LFf3X1JlDOB/yWI26TtGrHrDAVDeH*okj5i3U8zBeUAlTf6hR8BzbS0!/b/dAoAAAAAAAAA&bo=gAKaA1cEQAYFCCU!&rf=viewer_4");

        //等待下载
        yield return loadloadTexture;
        //下载的贴图直接赋值指定的游戏对象
        goSphere.GetComponent<Renderer>().material.mainTexture = loadloadTexture.texture;
    }


    IEnumerator GetTexture()
    {
        UnityWebRequest www = UnityWebRequestTexture.GetTexture("http://imageserver.uniregistry.com/catimg/computersinternetdownloads/c3.jpg");
        yield return www.SendWebRequest();

        Texture myTexture = DownloadHandlerTexture.GetContent(www);
        rawImage.texture = myTexture;
    }
}


