/***
 *
 *  Title: 
 *         第27章:  预加载与对象缓冲池技术
 *
 *  Description:
 *        功能：
 *            学习“对象缓冲池”技术
 *            利用缓冲池实现射击代码
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
using System.Collections.Generic;   

public class ShottingUseBufferPool : MonoBehaviour{
    public Texture Texture_ShootingCursor;                 //射击瞄准星
    public GameObject G0_CubeOrigianl;                     //射击原型物体
    public Transform Tran_TargetWallParentPosition;        //靶墙数组父对象
    public Transform Tran_BulletParentPosition;            //子弹数组父对象
    private Vector3 _VecRayPosion;                         //射线透射的坐标

    public GameObject GoPoolManager;                       //池管理器  
    public GameObject GoBulletPrefabsOriginal;             //子弹原型(预设) 
    private ObjectPoolManager boPoolManager;               //池管理器对象
    private GameObject goCloneBullete;                     //克隆的子弹

    /// <summary>
    /// 初始化场景
    /// </summary>
    void Start(){
        //隐藏鼠标。
        Cursor.visible = false;

        //取得池管理器
        boPoolManager = GoPoolManager.GetComponent<ObjectPoolManager>();    //获取脚本组件直接用文件名

        //建立射击目标靶墙
        for (int j = 1; j <= 5; j++)
        {
            for (int i = 1; i <= 5; i++)
            {
                GameObject goClone = (GameObject)Instantiate(G0_CubeOrigianl);
                goClone.transform.position = new Vector3(G0_CubeOrigianl.transform.position.x + i,
                    G0_CubeOrigianl.transform.position.y + j, G0_CubeOrigianl.transform.position.z);
                //确定子弹的父对象
                goClone.transform.parent = Tran_TargetWallParentPosition;
            }
        }
    }//Start_end

    /// <summary>
    /// 绘制射击光标
    /// </summary>
    void OnGUI(){
        Vector3 vecPos = Input.mousePosition;
        GUI.DrawTexture(new Rect(vecPos.x - Texture_ShootingCursor.width / 2,
            Screen.height - vecPos.y - Texture_ShootingCursor.height / 2, Texture_ShootingCursor.width,
            Texture_ShootingCursor.height), Texture_ShootingCursor);
    }//OnGUI_end

    /// <summary>
    /// 射击逻辑处理
    /// </summary>
    void Update(){
        //射线处理
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit))
        {
            //获取射线碰撞到碰撞体的方位
            _VecRayPosion = hit.point;
        }

        //如果鼠标点击左键，则发射子弹。
        if (Input.GetMouseButtonDown(0)){
            //创建子弹
            KeyValuePair<int, GameObject> kvObj = boPoolManager.PickObj();
            if (kvObj.Value != null){
                goCloneBullete = kvObj.Value;
                goCloneBullete.SendMessage("ReceiveBulletID", kvObj.Key);
            }
            //添加子弹刚体
            if (!goCloneBullete.GetComponent<Rigidbody>()){
                goCloneBullete.AddComponent<Rigidbody>();
            }
            //子弹的位置
            goCloneBullete.transform.position = new Vector3(Camera.main.transform.position.x, Camera.main.transform.position.y, Camera.main.transform.position.z + 0.3F);
            //给子弹加“力”
            goCloneBullete.GetComponent<Rigidbody>().AddForce((_VecRayPosion - goCloneBullete.transform.position) * 10F, ForceMode.Impulse);  
        }
    }//Update_end  

}//Class_end
