﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public bool m_bMain;
    public int m_nID;
    private int m_nTitleIns = 0;
    public HUDBloodType m_nBloodType = HUDBloodType.Blood_Red;
    public float m_fBloodPos = 1.0f; //血量进度
    public string m_szName;
    // Use this for initialization
    void Start ()
    {
        m_szName = name;
        RefreshTitle();
    }

    void RefreshTitle()
    {
        if( 0 == m_nTitleIns )
            m_nTitleIns = HUDTitleInfo.HUDTitleRender.Instance.RegisterTitle(transform, 1.8f, m_bMain); //构建了一个TitleInfo，返回对应的id

        float fOffsetY = GetHeadNameOffsetY();

        //从缓存中拿titleInfo
        HUDTitleInfo  title = HUDTitleInfo.HUDTitleRender.Instance.GetTitle(m_nTitleIns);
        title.Clear();

        title.SetOffsetY(fOffsetY);  //设置高度偏移量
        title.ShowTitle(true);
        // 血条
        HUDBloodType nBloodType = GetBloodType();
        if (nBloodType != HUDBloodType.Blood_None)
        {
            title.BeginTitle();
            title.PushBlood(nBloodType, curHpBarValue());
            title.EndTitle();

            // Test(title);
        }

        title.BeginTitle();
        title.PushTitle(m_szName, HUDTilteType.PlayerName, 0);
        // 威望
        {
            title.PushTitle("天下无双", HUDTilteType.PlayerPrestige, 1);
        }
        // 可反击标识(主角和平模式，并且可以反击）
        if (!m_bMain)
        {
            title.PushIcon(HUDTilteType.PKFlag, HudSetting.Instance.m_nPKFlagPic);
        }
        title.EndTitle();

        // 帮会名字
        string szFamily = "天下第一帮";
        if (!string.IsNullOrEmpty(szFamily))
        {
            title.BeginTitle();
            title.PushTitle(szFamily, HUDTilteType.PlayerCorp, 0);
            title.EndTitle();
        }

        // 称号
        {
            {
                string szDesign = "武林蒙主";
                int nFontType = 1;
                title.BeginTitle();
                title.PushTitle(szDesign, HUDTilteType.PlayerDesignation, nFontType);
                title.EndTitle();
            }
        }

        // 队长标记
        //if (isTeamLeader)
        {
            title.BeginTitle();
            title.PushIcon(HUDTilteType.HeadIcon, HudSetting.Instance.m_nTeamFlagPic);
            title.EndTitle();
        }
    }

    private void Test(HUDTitleInfo title)
    {
        //加载配置信息 assets_all.text 可以直接修改配置信息来调试看结果
        
        title.BeginTitle();
        title.PushBlood(HUDBloodType.Blood_Green, curHpBarValue());
        title.EndTitle();
        
        title.BeginTitle();
        //nSpriteID 就是配置里的NameID，位置的调整都在配置里HUDSetting里对应 HeadIcon
        title.PushIcon(HUDTilteType.HeadIcon, 12);
        title.EndTitle();
        
        title.BeginTitle();
        //PlayerTitle的设置在HUDSetting里
        title.PushTitle("dddddd", HUDTilteType.PlayerName,1);
        title.EndTitle();
    }

    public float curHpBarValue()
    {
        return m_fBloodPos;
    }

    HUDBloodType GetBloodType()
    {
        return m_nBloodType;
    }

    float GetHeadNameOffsetY()
    {
        return 0.5f;
    }

    void OnDestory()
    {
        if(m_nTitleIns != 0)
            HUDTitleInfo.HUDTitleRender.Instance.ReleaseTitle(m_nTitleIns);
        m_nTitleIns = 0;
    }

    // 功能：显示伤害数字
    public void ShowHurt(int nHurtHp)
    {
        // 有一定概率显示爆击
        bool bShowCT = Random.Range(1, 100) < 10;
        if(bShowCT)
        {
            if(m_bMain)
                HUDNumberRender.Instance.AddHudNumber(transform, HUDNumberRenderType.HUD_SHOW_CT_ATTACKED, nHurtHp, true, false, false);
            else
                HUDNumberRender.Instance.AddHudNumber(transform, HUDNumberRenderType.HUD_SHOW_CT_ATTACK, nHurtHp, true, false, false);
        }
        else
        {
            if (m_bMain)
                HUDNumberRender.Instance.AddHudNumber(transform, HUDNumberRenderType.HUD_SHOW_HP_HURT, nHurtHp, false, false, true);
            else
                HUDNumberRender.Instance.AddHudNumber(transform, HUDNumberRenderType.HUD_SHOW_COMMON_ATTACK, nHurtHp, false, false, true);
        }
    }

    // 功能：显示经验数字
    public void  ShowExp(int nExp)
    {
        HUDNumberRender.Instance.AddHudNumber(transform, HUDNumberRenderType.HUD_SHOW_EXP_ADD, nExp, true, true, false);
    }
	
	// Update is called once per frame
	void Update ()
    {		
	}

}
