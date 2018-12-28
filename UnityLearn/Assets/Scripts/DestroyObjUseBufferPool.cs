/***
 *
 *  Title: 
 *         第27章:  预加载与对象缓冲池技术
 *
 *  Description:
 *        功能：
 *            回收对象脚本
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

public class DestroyObjUseBufferPool : MonoBehaviour {
    public GameObject GoPoolManager;                       //池管理器
    private ObjectPoolManager _PoolManagerObj;             //对象池管理器
    private int _IntBulletID = 0;                          //子弹ID编号 

    void Start(){
        _PoolManagerObj = GoPoolManager.GetComponent<ObjectPoolManager>();
    }

    /// <summary>
    /// 游戏对象超出摄像机可视范围，则此对象进行“回收”。
    /// </summary>
    void OnBecameInvisible(){
        _PoolManagerObj.RecyleObj(_IntBulletID);
    }

    /// <summary>
    /// 接收本脚本所属对象的ID编号，用于回收使用。
    /// </summary>
    /// <param name="intBulleteNumber"></param>
    public void ReceiveBulletID(int intBulleteNumber){
        _IntBulletID = intBulleteNumber;
    }
}
