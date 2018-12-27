/***
 *
 *  Title: 
 *         第21章 导航寻路
 *         
 *  Description:
 *        功能：
 *            寻路障碍物控制脚本
 *           
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
using UnityEngine.AI;                       //导入AI命名空间

public class NavmeshObstacleControl : MonoBehaviour
{
    private NavMeshObstacle _navMeshObs;    //路径障碍组件

    void Start(){
        _navMeshObs = this.GetComponent<NavMeshObstacle>();
    }

    void Update(){
        if (Input.GetButtonDown("Fire1")){  //允许通过               
            if (_navMeshObs){
                _navMeshObs.enabled = false;               
                this.GetComponent<Renderer>().material.color = Color.green;
            }
        }

        if (Input.GetButtonUp("Fire1")){    //禁止通过               
            if (_navMeshObs){
                _navMeshObs.enabled = true;
                this.GetComponent<Renderer>().material.color = Color.red;
            }
        }
    }
}

