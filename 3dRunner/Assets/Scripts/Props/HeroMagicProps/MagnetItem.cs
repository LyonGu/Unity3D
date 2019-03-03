
/***
 *
 *  Title: "3DRunner" 项目
 *         描述：道具组--> 磁铁吸引道具
 *
 *  Description:
 *        功能：
 *       
 *
 *  Date: 2019
 * 
 *  Version: 1.0
 *
 *  Modify Recorder:
 *     
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MagnetItem : BasePropItem{

    //最大吸引距离
    public int MaxMagnetLength = 8;

    //最小道具销毁距离
    public float MinPropDestroyLength = 0.5F;

    //英雄的方位
    private Transform _HeroTran = null;


	// Use this for initialization
	void Start () {
        _HeroTran = GameObject.FindGameObjectWithTag(Global.HeroTagName).transform;
        if(_HeroTran == null)
        {
            Debug.LogError(GetType() + "/Start()/_HeroTran 查找不到，请检查！");
        }
        //启动本道具与英雄距离检查协程
        InvokeRepeating("CheckLengthByHero", 2F, 0.2F);
	}

    // 检查与英雄的距离
    private void CheckLengthByHero()
    { 
        if(Global.HeroMagState == HeroMagicState.Magnet)
        {
            if (Vector3.Distance(this.transform.position, _HeroTran.position) < MaxMagnetLength)
            {
                StartCoroutine(UserMagnet(_HeroTran));
            }

        }
    }

    /// 磁铁吸引
    private IEnumerator UserMagnet(Transform target)
    {
        bool isLoop = true;
        while(isLoop)
        {
            this.transform.position = Vector3.Lerp(this.transform.position, target.position, Global.PlayerCurRunSpeed * Time.deltaTime);
            if (Vector3.Distance(this.transform.position, target.position) < MinPropDestroyLength)
            {
                isLoop = false;
                //音频处理
                //todo...
                yield return null;
            }
        }
        StopCoroutine("UserMagnet");
        yield return null;
    }


	// Update is called once per frame
	void Update () {
		
	}
}
