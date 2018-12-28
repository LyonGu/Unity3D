/***
 *
 *  Title: 
 *         第29章:  AssetBundle资源动态加载技术
 *                  演示AssetBundle基本加载
 *
 *  Description:
 *        功能：
 *            演示加载AssetBundle且显示资源，一共分两种情形。
 *            1： 非“对象预设”资源加载与显示。
 *            2： “对象预设”资源加载与显示。 
 *            
 *        说明： 
 *            1： 这里所谓的“基本加载”即在不使用自定义“AssetBundle框架”情况下加载“对象预设”。
 *            2： 这里AssetBundle资源，是提前已经打包好的。
 *
 *  Date: 2017
 * 
 *  Version: 1.0
 *
 *  Modify Recorder:
 *     
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AssetBundleLoadDemo:MonoBehaviour {
    //显示的方位
    public Transform TraShowPosition;
    //测试更改贴图
    public GameObject goCube_ChangeTxt;
    /*  URL与材质名称  */
    //第1组，测试显示“预设体”
    private string _ABURL_1;                                
    private string _AssetName_1;
    //第2组，测试改变贴图
    private string _ABURL_2;
    private string _AssetName_2;

    private void Awake(){        
        //对象预设资源路径
        _ABURL_1 = "file://" + Application.streamingAssetsPath + "/prefabab.ab";
        _AssetName_1 = "SpherePink";
        //对象贴图资源路径
        _ABURL_2 = "file://" + Application.streamingAssetsPath + "/texturesab";
        _AssetName_2 = "unitychan_tile3";
    }

    private void Start(){
        //实验1： 加载AB包，显示简单“预设体”
        //StartCoroutine(LoadPrefabsFromAB(_ABURL_1, _AssetName_1, TraShowPosition));

        //实验2： 加载AB包，显示贴图(材质、音频等)
        StartCoroutine(LoadNonObjFromAB(_ABURL_2, goCube_ChangeTxt,_AssetName_2));
    }

    /// <summary>
    /// (下载且)加载“预设”资源
    /// </summary>
    /// <param name="ABURL">AssetBundle URL</param>
    /// <param name="AssetName">资源名称</param>
    /// <param name="showPos">实例化克隆体显示方位</param>
    /// <returns></returns>
    IEnumerator LoadPrefabsFromAB(string ABURL,string assetaName="",Transform showPos=null)
    {
        //参数检查
        if (string.IsNullOrEmpty(ABURL))
            Debug.LogError(GetType()+ "/LoadPrefabsFromAB()/ 输入参数‘AssetBundle URL’为空，请检查！");
        using (WWW www=new WWW(ABURL)){
            yield return www;
            AssetBundle ab = www.assetBundle;
            if (ab!=null){
                if (assetaName == ""){
                    //实例化主资源
                    if (showPos!=null){
                        //确定显示方位
                        GameObject tmpClonePrefabs=(GameObject)Instantiate(ab.mainAsset);
                        tmpClonePrefabs.transform.position = showPos.transform.position;
                    }
                    else {
                        Instantiate(ab.mainAsset);
                    }
                }
                else {
                    //实例化指定资源
                    if (showPos != null){
                        //确定显示方位
                        GameObject tmpClonePrefabs = (GameObject)Instantiate(ab.LoadAsset(assetaName));
                        tmpClonePrefabs.transform.position = showPos.transform.position;
                    }
                    else{
                        Instantiate(ab.LoadAsset(assetaName));
                    }
                }
                //卸载资源（只卸载AssetBundle 包本身）
                ab.Unload(false);
            }
            else {
                Debug.LogError(GetType()+ "/LoadPrefabsFromAB()/WWW 下载出错，请检查 AssetBundle URL ："+ABURL+" 错误信息： "+www.error);
            }
        }
    }

    /// <summary>
    /// (下载且)加载"非GameObject"资源
    /// </summary>
    /// <param name="ABURL">AssetBundle URL</param>
    /// <param name="AssetName">资源名称</param>
    /// <returns></returns>
    IEnumerator LoadNonObjFromAB(string ABURL, GameObject goShowObj,string AssetName = "")
    {
        //参数检查
        if (string.IsNullOrEmpty(ABURL) || goShowObj==null)
        {
            Debug.LogError(GetType() + "/LoadTextureFromAB()/ 输入的参数为空，请检查！");
        }

        using (WWW www = new WWW(ABURL))
        {
            yield return www;
            AssetBundle ab = www.assetBundle;
            if (ab != null)
            {
                if (AssetName=="")
                {
                    goShowObj.GetComponent<Renderer>().material.mainTexture = (Texture)ab.mainAsset;
                }
                else {
                    goShowObj.GetComponent<Renderer>().material.mainTexture = (Texture)ab.LoadAsset(AssetName);
                }
                //卸载资源（只卸载AssetBundle 包本身）
                ab.Unload(false);
            }
            else
            {
                Debug.LogError(GetType() + "/LoadTextureFromAB()/WWW 下载出错，请检查 AssetBundle URL ：" + ABURL + " 错误信息： " + www.error);
            }
        }
    }

}//Class_end

