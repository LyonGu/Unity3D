/***
 *
 *  Title: 
 *         第21章 导航寻路
 *         
 *  Description:
 *        功能：
 *            基本导航控制脚本
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

public class BaseNavigation : MonoBehaviour{
    //寻路目标
    public Transform TraFindDestination;      
    //寻路组件
    private UnityEngine.AI.NavMeshAgent _Agent;                          


    void Start(){
        _Agent = this.GetComponent<UnityEngine.AI.NavMeshAgent>();
    }//Start_end


    void Update(){
        //设置寻路
        if (_Agent && TraFindDestination)
        {
            _Agent.SetDestination(TraFindDestination.transform.position);
        }
    }//Update_end
}

