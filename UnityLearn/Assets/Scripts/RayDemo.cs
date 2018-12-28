/***
 *
 *  Title: 
 *         第25章:  射线
 *
 *  Description:
 *        功能：
 *            学习“射线”基本原理与功能
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

public class RayDemo : MonoBehaviour {
    public Texture Texture_ShootingCursor;                 //射击瞄准星
    public GameObject G0_CubeOrigianl;                     //射击原型物体
    private Vector3 _VecRayPosion;                         //射线透射的坐标

	void Start () {
        //隐藏鼠标。
        Cursor.visible = false;
        //建立射击目标
        for (int j = 1; j <=5; j++){
            for (int i = 1; i <=5; i++){
                //克隆一个对象
                GameObject goClone = (GameObject)Instantiate(G0_CubeOrigianl);
                goClone.transform.position = new Vector3(G0_CubeOrigianl.transform.position.x +i,
                    G0_CubeOrigianl.transform.position.y+j, G0_CubeOrigianl.transform.position.z);
                goClone.SetActive(true);
            }            
        }
	}//Start_end

    void OnGUI()
    {

        // 左上角为原点
        Vector3 vecPos = Input.mousePosition;
        Rect rect = new Rect(vecPos.x - Texture_ShootingCursor.width /2, Screen.height - vecPos.y - Texture_ShootingCursor.height/2
            ,Texture_ShootingCursor.width, Texture_ShootingCursor.height);
        GUI.DrawTexture(rect, Texture_ShootingCursor);
    }
	
	void Update () {
        /* 射线的基本原理 */
        //定义一条从摄像机发射，沿着鼠标的方向无限延长的隐形射线。
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit)){
            //获取射线碰撞到碰撞体的方位
            _VecRayPosion = hit.point;
        }

        //如果鼠标点击左键，则发射子弹。
        if(Input.GetMouseButtonDown(0)){
            //创建子弹
            GameObject goBullet = GameObject.CreatePrimitive(PrimitiveType.Sphere); //创建一个gameobject
            //添加子弹的刚体
            goBullet.AddComponent<Rigidbody>();
            //子弹的位置
            goBullet.transform.position = Camera.main.transform.position;
            //给子弹加“力”
            goBullet.GetComponent<Rigidbody>().AddForce((_VecRayPosion - goBullet.transform.position) * 10F, 
                ForceMode.Impulse);  
            //添加脚本： 如果子弹超出射线机的范围，则进行销毁。
            goBullet.AddComponent<DestroyGameobject>();
        }
	}//Update_end

}//Class_end
