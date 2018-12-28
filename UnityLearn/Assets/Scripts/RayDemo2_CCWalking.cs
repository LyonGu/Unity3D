/***
 *
 *  Title: 
 *         第25章:  射线
 *
 *  Description:
 *        功能：
 *            使用“射线”技术，实现位置定位
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

public class RayDemo2_CCWalking : MonoBehaviour{
    private Vector3 VecGoalPosition;                       //移动的目标位置
    CharacterController CC;                                //角色控制器

    CharacterController CC1;

    void Start(){
        //得到角色控制器
        CC = gameObject.GetComponent<CharacterController>();
        CC1 = gameObject.GetComponent<CharacterController>();
    }
	void Update () {
        //确定移动位置
        if (Input.GetMouseButton(0)){
            //定义一个射线
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            //如果命中
            if (Physics.Raycast(ray, out hit)){
                VecGoalPosition = hit.point;
            }
        }
        //角色移动
        if (Vector3.Distance(VecGoalPosition, this.transform.position) >1F){
            //移动的步伐
            Vector3 step = Vector3.ClampMagnitude(VecGoalPosition - this.transform.position, 0.1f);
            //角色控制器的移动
            CC.Move(step);
        }
	}//UPDATE_END
}
