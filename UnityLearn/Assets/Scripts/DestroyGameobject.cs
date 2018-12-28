/***
 *
 *  Title: 
 *         第25章:  射线
 *
 *  Description:
 *        功能：
 *            销毁游戏对象。
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

public class DestroyGameobject : MonoBehaviour {

    //游戏对象超出摄像机范围，则执行以下代码
    void OnBecameInvisible(){
        Destroy(this.gameObject);
    }
}//Class_end
