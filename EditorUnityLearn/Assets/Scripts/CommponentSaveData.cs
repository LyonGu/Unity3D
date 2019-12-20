using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.ParticleSystem;


public enum CompoentState : byte
{
    Born,
    Dead
}


// 组件类型
public enum CompoentType : byte
{
    ParticleSystem,

    // todo
}

public enum PropertyType : byte
{
    //ParticleSystem

    //测试用
    Particle_MaxParticles,
    Particle_Duration,
    Particle_Looping,
    Particle_StartColor,

    //正式用
    Particle_RateOverTime,
    Particle_RateOverDistance,
    Particle_Brust,

}


public class BaseData<T>
{
    public PropertyType property;
    public T value;

    public BaseData(PropertyType prop, T v)
    {
        value = v;
        property = prop;
    }
}


[Serializable]
public class OtherData
{
    //这种类型 直接清空
    public PropertyType property;
}


[Serializable]
public class CommponentData
{
    public CompoentType compType;
    public List<BaseData<int>> iList = new List<BaseData<int>>();
    public List<BaseData<float>> fList = new List<BaseData<float>>();
    public List<BaseData<bool>> bList = new List<BaseData<bool>>();
    public List<BaseData<Vector3>> v3List = new List<BaseData<Vector3>>();
    public List<BaseData<Vector2>> v2List = new List<BaseData<Vector2>>();
    public List<BaseData<Color>> cList = new List<BaseData<Color>>();
    public List<OtherData> oList = new List<OtherData>();
}

[Serializable]
public class ShowCommponentData
{
    public CompoentType compType;
    public List<PropertyType> properList;

}


public class CommponentSaveData : MonoBehaviour
{
    public CompoentState state = CompoentState.Born;
    public GameObject targetGameObject;

    public List<ShowCommponentData> compList;

    [HideInInspector]
    public List<CommponentData> BornDatas;

    [HideInInspector]
    public List<CommponentData> DeadDatas;

    //去重处理
    public Dictionary<CompoentType,List<PropertyType>> PDict = new Dictionary<CompoentType, List<PropertyType>>();
    public Dictionary<CompoentType, bool> CDict = new Dictionary<CompoentType, bool>();




    public void SaveToDatas()
    {
        switch (state)
        {
            case CompoentState.Born:
                SaveDatas(BornDatas);
                break;
            case CompoentState.Dead:
                SaveDatas(DeadDatas);
                break;
        }
    }

    void SaveDatas(List<CommponentData> dataList)
    {

        if (dataList.Count > 0)
        {
            dataList.Clear();
        }

        CDict.Clear();
        PDict.Clear();
        /*
            1 得到所有组件
            2 得到组件对应的所有属性
         */
        if (compList.Count > 0)
        {
            for (int i = 0; i < compList.Count; ++i)
            {
                ShowCommponentData data = compList[i];
                CompoentType comType = data.compType;
                switch (comType)
                {
                    case CompoentType.ParticleSystem:
                        if (!CDict.ContainsKey(comType))
                        {
                            CDict[comType] = false;
                            PDict[comType] = new List<PropertyType>();
                        }

                        if (!CDict[comType])
                        {
                            CDict[comType] = true;
                            CommponentData cd = new CommponentData();
                            cd.compType = comType;
                            dataList.Add(cd);

                            SaveParticleSystemData(data, cd);
                        }
                     
                        break;
                }

            }
        }
    }

    /*
       数据存储 
    */

    void SaveParticleSystemData(ShowCommponentData showData, CommponentData cd)
    {

        GameObject gObj = targetGameObject;
        ParticleSystem particle = gObj.GetComponent<ParticleSystem>();
        MainModule main = particle.main;
        EmissionModule emission = particle.emission;
        for (int j = 0; j < showData.properList.Count; j++)
        {
            PropertyType pty = showData.properList[j];

            if (!PDict[CompoentType.ParticleSystem].Contains(pty))
            {
                PDict[CompoentType.ParticleSystem].Add(pty);
                switch (pty)
                {
                    case PropertyType.Particle_MaxParticles:
                        cd.iList.Add(new BaseData<int>(pty, main.maxParticles));
                        break;
                    case PropertyType.Particle_Duration:
                        cd.fList.Add(new BaseData<float>(pty, main.duration));
                        break;
                    case PropertyType.Particle_Looping:
                        cd.bList.Add(new BaseData<bool>(pty, main.loop));
                        break;
                    case PropertyType.Particle_StartColor:
                        cd.cList.Add(new BaseData<Color>(pty, main.startColor.color));
                        break;
                    case PropertyType.Particle_RateOverTime:
                        cd.fList.Add(new BaseData<float>(pty, emission.rateOverTime.constant));
                        break;
                    case PropertyType.Particle_RateOverDistance:
                        cd.fList.Add(new BaseData<float>(pty, emission.rateOverDistance.constant));
                        break;
                    case PropertyType.Particle_Brust:
                        break;
                }
            }
            
        }
    }

    /************************************************************************/

    public void SetState(CompoentState tarState)
    {
        switch (tarState)
        {
            case CompoentState.Born:
                UpdateDatas(BornDatas);
                break;
            case CompoentState.Dead:
                UpdateDatas(DeadDatas);
                break;
        }
    }


    void UpdateDatas(List<CommponentData> datas)
    {
        if (datas.Count > 0)
        {
            for (int i = 0; i < datas.Count; ++i)
            {
                CommponentData data = datas[i];
                CompoentType comtype = data.compType;
                switch (comtype)
                {
                    case CompoentType.ParticleSystem:
                        ParticleSystemData(data);
                        break;
                }
            }
        }
    }
    // 粒子系统组件
    void ParticleSystemData(CommponentData data)
    {
        GameObject gObj = targetGameObject;
        ParticleSystem particle = gObj.GetComponent<ParticleSystem>();
        MainModule main = particle.main;
        EmissionModule emission = particle.emission;
        //遍历所有的集合

        int count = data.iList.Count;
        if (count > 0)
        {
            for (int i = 0; i < count; i++)
            {
                BaseData<int> d = data.iList[i];
                PropertyType property = d.property;
                switch (property)
                {
                    case PropertyType.Particle_MaxParticles:
                        main.maxParticles = d.value;
                        break;
                }
            }
        }

        count = data.fList.Count;
        if (count > 0)
        {
            for (int i = 0; i < count; i++)
            {
                BaseData<float> d = data.fList[i];
                float value = (float)d.value;
                PropertyType property = d.property;
                switch (property)
                {
                    case PropertyType.Particle_Duration:
                        main.duration = value;
                        break;
                    case PropertyType.Particle_RateOverTime:
                        emission.rateOverTime = value;
                        break;
                    case PropertyType.Particle_RateOverDistance:
                        emission.rateOverDistance = value;
                        break;
                }
            }
        }

        count = data.bList.Count;
        if (count > 0)
        {
            for (int i = 0; i < count; i++)
            {
                BaseData<bool> d = data.bList[i];
                PropertyType property = d.property;
                switch (property)
                {
                    case PropertyType.Particle_Looping:
                        main.loop = d.value;
                        break;
                }
            }
        }

        count = data.cList.Count;
        if (count > 0)
        {
            for (int i = 0; i < count; i++)
            {
                BaseData<Color> d = data.cList[i];
                PropertyType property = d.property;
                switch (property)
                {
                    case PropertyType.Particle_StartColor:
                        main.startColor = d.value;
                        break;
                }
            }
        }

        count = data.oList.Count;
        if (count > 0)
        {
            for (int i = 0; i < count; i++)
            {
                OtherData d = data.oList[i];
                PropertyType property = d.property;
                switch (property)
                {
                    case PropertyType.Particle_Brust:
                        Burst[] brust = new Burst[0];
                        emission.SetBursts(brust);
                        break;
                }
            }
        }

        // vector3 vector2 todo....
    }

    
}