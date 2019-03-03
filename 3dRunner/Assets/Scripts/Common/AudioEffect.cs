/***
 *
 *  Title: "不夜城跑酷" 项目
 *         道具组-->英雄音效脚本
 *
 *  Description:
 *        功能：[本脚本的主要功能描述] 
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
using kernal;

public class AudioEffect:BasePropItem
{
    //销毁自身延迟时间
    public string AudioEffectName = null;

    private void Awake()
    {
        base.m_PropTriggerHandle = PlayAudioEffect;
    }

    private void Start()
    {
        //设置音效音量
        AudioManager.SetAudioEffectVolumns(1F);
    }

    /// <summary>
    /// 播放道具音效
    /// </summary>
    private void PlayAudioEffect()
    {
        //Debug.Log(GetType() + "/播放道具音频");
        if (!string.IsNullOrEmpty(AudioEffectName))
            AudioManager.PlayAudioEffectB(AudioEffectName);
        else
            Debug.LogError(GetType() + "/PlayAudioEffect()/道具音效文件不存在，请检查！");
    }  
}//Class_end