/***
 *
 *  Title: 
 *         第27章:  预加载与对象缓冲池技术
 *
 *  Description:
 *        功能：
 *            销毁离开摄像机范围的子弹
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

public class DestroyObjNoBufferPool : MonoBehaviour{

    //游戏对象超出摄像机范围，则执行以下代码
    void OnBecameInvisible(){
        Destroy(this.gameObject);
    }
}//Class_end
