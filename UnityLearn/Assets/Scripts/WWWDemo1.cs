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

public class WWWDemo1 : MonoBehaviour{
    public GameObject goCube;                              //立方体
    public GameObject goSphere;                            //球体
    private Texture2D TxtDownloadTextures;                 //需要下载的贴图


    //下载本机贴图
    public void DownLoadTexturesByWWW(){
        StartCoroutine("StartDownLoadTexture");
    }

    //从互联网下载贴图
    public void DownLoadTexturesFromHTTP(){
        StartCoroutine("StartDownLoadTextureFromHTTP");
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
	
}


