using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.UI;
using UGameObject = UnityEngine.GameObject;

public class UIAnimationHxp : MonoBehaviour
{

    #region 配置数据结构定义
    public enum PlayState : byte
    {
        NoDoing,
        Doing,
        Done,
    }

    [Serializable]
    //动画类型
    public enum AnimationType : byte
    {
        MoveTo,
        MoveBy,
        ScaleTo,
        ScaleBy,
        RotateTo,
        RotateBy,
        Fade,
        Color,
        GradientColor,
        DoPath,
        MaterialFloat,
        MaterialColor,
    }

    [Serializable]
    //动画控制类型
    public enum UIAnimationControlType : byte
    {
        AnimationCurve = 100,
        Unset = Ease.Unset,
        Linear = Ease.Linear,
        InSine = Ease.InSine,
        OutSine = Ease.OutSine,
        InOutSine = Ease.InOutSine,
        InQuad = Ease.InQuad,
        OutQuad = Ease.OutQuad,
        InOutQuad = Ease.InOutQuad,
        InCubic = Ease.InCubic,
        OutCubic = Ease.OutCubic,
        InOutCubic = Ease.InOutCubic,
        InQuart = Ease.InQuart,
        OutQuart = Ease.OutQuart,
        InOutQuart = Ease.InOutQuart,
        InQuint = Ease.InQuint,
        OutQuint = Ease.OutQuint,
        InOutQuint = Ease.InOutQuint,
        InExpo = Ease.InExpo,
        OutExpo = Ease.OutExpo,
        InOutExpo = Ease.InOutExpo,
        InCirc = Ease.InCirc,
        OutCirc = Ease.OutCirc,
        InOutCirc = Ease.InOutCirc,
        InElastic = Ease.InElastic,
        OutElastic = Ease.OutElastic,
        InOutElastic = Ease.InOutElastic,
        InBack = Ease.InBack,
        OutBack = Ease.OutBack,
        InOutBack = Ease.InOutBack,
        InBounce = Ease.InBounce,
        OutBounce = Ease.OutBounce,
        InOutBounce = Ease.InOutBounce,
        Flash = Ease.Flash,
        InFlash = Ease.InFlash,
        OutFlash = Ease.OutFlash,
        InOutFlash = Ease.InOutFlash,

    }

    [Serializable]
    public class AniUnitData
    {
        public int animationId;
        public AnimationType animationType = AnimationType.MoveTo;
        public UIAnimationControlType uIAnimationControlType = UIAnimationControlType.Linear;
        public AnimationCurve animationCurve;
        public UGameObject obj;
        public float duration;
        public Vector3 startValue;
        public Vector3 endValue;
        public Color startColor;
        public Color endColor;
        public Gradient gradientColor;
        public float startAlpha;
        public float endAlpha;
        public List<Vector3> pathList;
        public float delayTime = 0;
        public bool isLoop;
        public LoopType loopType;
        public string propertyName;
        public UGameObject particleSystemObj;
    }

    [Serializable]
    public class SequenceAniUnitData
    {
        public int animationId;

    }


    public class RecycleMaterialData
    {
        public Image imgCom;
        public Material mat;
    }


    #endregion

    public List<AniUnitData> animationDataConfig = new List<AniUnitData>();
    public List<SequenceAniUnitData> SequenceAniList = new List<SequenceAniUnitData>();
    public Dictionary<int, int> SequenceAniDic = new Dictionary<int, int>();
    private Dictionary<int, AniUnitData> animationDic = new Dictionary<int, AniUnitData>();
    private Dictionary<int, RecycleMaterialData> recycleMaterialDataDic = new Dictionary<int, RecycleMaterialData>();

    

    private int endtotalAniCount = 0;
    private int realyAniCount = 0;  // 真是的动作总数: 循环的需要剔除, 路径动画但是路径点为空的需要剔除
    private bool isInit = false;
    private int ANIMID = 100;
    private Action allCompleteHandle = null;
    private PlayState playState = PlayState.NoDoing;

    public bool isAutoPlay = false;
    public int debugAnimationId = 0;
    public float maxAnimationTime = 5;  //动作最大执行时间 单位秒

    public int GetMaxAnimId()
    {
        int maxANIMID = ANIMID;
        if (animationDataConfig.Count > 0)
        {
            foreach (var item in animationDataConfig)
            {
                if (item.animationId > maxANIMID)
                {
                    maxANIMID = item.animationId;
                }
            }
        }
        return maxANIMID;
    }

    private void InitlogicData()
    {
        if (!isInit)
        {
            isInit = true;

            //统计动作总数
            if (animationDataConfig.Count > 0)
            {
                foreach (var item in animationDataConfig)
                {
                    UGameObject uGameObj = item.obj;
                    bool isLoop = item.isLoop;
                    if (uGameObj != null && !isLoop)
                    {
                        if (item.animationType == AnimationType.DoPath)
                        {
                            if (item.pathList.Count > 0)
                            {
                                realyAniCount++;
                            }
                        }
                        else if(item.animationType == AnimationType.MaterialFloat || item.animationType == AnimationType.MaterialColor)
                        {
                            //材质相关的需要检查对应shader里是否存在对应属性，不存在时运动不会执行
                            Material mat = uGameObj.GetComponent<Image>().material;
                            bool isExsit = this.CheckPropertyIsExsit(mat, item.propertyName);
                            if (isExsit)
                            {
                                realyAniCount++;
                            }
                        }
                        else
                        {
                            realyAniCount++;
                        }
                    }
                }
            }

            if (SequenceAniList.Count > 0)
            {
                foreach (var item in SequenceAniList)
                {
                    if (!SequenceAniDic.ContainsKey(item.animationId))
                    {
                        SequenceAniDic.Add(item.animationId, item.animationId);
                    }
                }
            }

        }
    }

    private void AddRecycleMaterialData(RecycleMaterialData data)
    {
        int id = data.mat.GetInstanceID();
        if (!recycleMaterialDataDic.ContainsKey(id))
        {
            recycleMaterialDataDic.Add(data.imgCom.GetInstanceID(), data);
        }
        else
        {
            recycleMaterialDataDic[id] = data;
        }
    }

    private Material GetRecyCleMaterial(int id)
    {
        if (recycleMaterialDataDic.ContainsKey(id))
        {
            return recycleMaterialDataDic[id].mat;
        }
        return null;
    }

    private void RemoveRecycleMaterialData(RecycleMaterialData data)
    {
        int id = data.mat.GetInstanceID();
        if (recycleMaterialDataDic.ContainsKey(id))
        {
            data.imgCom.material = null;
            Resources.UnloadAsset(data.mat);
            recycleMaterialDataDic.Remove(id);
        }
    }

    private void RealseRecycleMaterialData(RecycleMaterialData data)
    {
        data.imgCom.material = null;
        Destroy(data.mat);
    }

    private void RemoveAllRecycleMaterialData()
    {
        if (recycleMaterialDataDic.Count > 0)
        {
            foreach (var item in recycleMaterialDataDic)
            {
                this.RealseRecycleMaterialData(item.Value);
            }
            recycleMaterialDataDic.Clear();
        }
    }

    private void Awake()
    {
        playState = PlayState.NoDoing;
        InitlogicData();
        InitAnimMapData();
    }

    private void Start()
    {
        if (isAutoPlay)
            Play();
    }

    private void OnEnable()
    {
        playState = PlayState.NoDoing;
        InitlogicData();
        InitAnimMapData();
    }
    private void InitAnimMapData()
    {
        if (animationDic.Count == 0)
        {
            if (animationDataConfig.Count > 0)
            {
                foreach (var item in animationDataConfig)
                {
                    if (!animationDic.ContainsKey(item.animationId))
                    {
                        animationDic.Add(item.animationId, item);
                    }
                    else
                    {
                        Debug.LogError($"InitAnimMapData 配置数据里 动作ID {item.animationId} 有重复，请检查配置信息");
                    }
                }
            }
        }
    }

    public bool CheckDatas(out string msg)
    {
        bool result = true;
        msg = string.Empty;
        var checkDic = new Dictionary<int, bool>();
        if (animationDataConfig.Count > 0)
        {
            foreach (var item in animationDataConfig)
            {
                if (item.obj == null)
                {
                    result = false;
                    msg = $"动作Id为: {item.animationId} 的配置数据里 [目标对象] 为空，请检查配置信息!";
                    break;
                }
                if (!checkDic.ContainsKey(item.animationId))
                {
                    checkDic.Add(item.animationId, true);
                }
                else
                {
                    result = false;
                    msg = $"动作Id {item.animationId} 有重复，请检查配置信息!";
                    break;
                }

                //类型检查
                if (item.animationType == AnimationType.DoPath)
                {
                    if (item.pathList.Count == 0)
                    {
                        result = false;
                        msg = $"动作Id {item.animationId} 路点数据为空，请检查配置信息!";
                        break;
                    }
                }
                else if (item.animationType == AnimationType.MaterialFloat || item.animationType == AnimationType.MaterialColor)
                {
                    Material mat = item.obj.GetComponent<Renderer>().material;
                    bool isExsit = this.CheckPropertyIsExsit(mat, item.propertyName);
                    if (!isExsit)
                    {
                        result = false;
                        msg = $"动作ID：{item.animationId} 目标物体 {item.obj.name} 使用的shader中不包含属性名 {item.propertyName}======";
                        break;
                    }
                }
            }
        }
        return result;
    }

    private void AnimationEnd(int animId)
    {
        Debug.Log($"animId: {animId} 执行完毕=====");
        endtotalAniCount++;
        if (endtotalAniCount >= realyAniCount)
        {
            Debug.Log($"所有动作执行完毕===== {endtotalAniCount} -- {realyAniCount}");
            endtotalAniCount = 0;
            playState = PlayState.Done;
            allCompleteHandle?.Invoke();
            allCompleteHandle = null;
        }

    }

    //测试用
    public void PlayById(int animID)
    {
        if (animID <= 0)
            return;
        playState = PlayState.NoDoing;
        foreach (var item in animationDataConfig)
        {
            if (item.animationId == animID)
            {
                this.Play(item);
                break;
            }
        }

    }

    private Tween Play(AniUnitData data, bool isBack = false)
    {
        Tween twe = null;
        UGameObject uGameObj = data.obj;
        if (uGameObj == null)
        {
            Debug.LogError($"配置里有错误数据 动作ID:{data.animationId}");
            return twe;
        }
        playState = PlayState.Doing;
        AnimationType animationT = data.animationType;
        switch (animationT)
        {
            case AnimationType.MoveTo:
                twe = this._DOMoveTo(data, isBack);
                break;
            case AnimationType.MoveBy:
                twe = this._DOMoveBy(data, isBack);
                break;
            case AnimationType.ScaleTo:
                twe = this._DOScaleTo(data, isBack);
                break;
            case AnimationType.ScaleBy:
                twe = this._DOScaleBy(data, isBack);
                break;
            case AnimationType.RotateTo:
                twe = this._DORotateTo(data, isBack);
                break;
            case AnimationType.RotateBy:
                twe = this._DORotateBy(data, isBack);
                break;
            case AnimationType.Fade:
                twe = this._DOFade(data, isBack);
                break;
            case AnimationType.Color:
                twe = this._DOColor(data, isBack);
                break;
            case AnimationType.GradientColor:
                twe = this._DoGradientColor(data, isBack);
                break;
            case AnimationType.DoPath:
                if (data.pathList.Count > 0)
                {
                    twe = this._DOPath(data, isBack);
                }
                else
                {
                    Debug.LogError($"动作ID：{data.animationId}路点数据为空======");
                }
                   
                break;
            case AnimationType.MaterialFloat:
                Material mat = uGameObj.GetComponent<Image>().material;
                bool isExsit = this.CheckPropertyIsExsit(mat, data.propertyName);
                if (isExsit)
                {
                    twe = this._DOMaterialFloat(data, isBack);
                }
                else
                {
                    Debug.LogError($"动作ID：{data.animationId} 目标物体 {uGameObj.name} 使用的shader中不包含属性名 {data.propertyName}======");
                }
                break;
            case AnimationType.MaterialColor:
                Material mat1 = uGameObj.GetComponent<Image>().material;
                bool isExsit1 = this.CheckPropertyIsExsit(mat1, data.propertyName);
                if (isExsit1)
                {
                    twe = this._DOMaterialColor(data, isBack);
                }
                else
                {
                    Debug.LogError($"动作ID：{data.animationId} 目标物体 {uGameObj.name}  使用的shader中不包含属性名 {data.propertyName}======");
                }
                break;

        }
        var particleSystemObj = data.particleSystemObj;
        if (particleSystemObj != null)
        {
            particleSystemObj.SetActive(false);
            particleSystemObj.SetActive(true);
        }
        return twe;
    }
    public void Play(Action allComplete = null)
    {

        if (animationDataConfig.Count > 0)
        {
            if (playState == PlayState.Doing)
                return;
            if (allComplete != null)
                allCompleteHandle = allComplete;
            foreach (var item in animationDataConfig)
            {
                UGameObject uGameObj = item.obj;
                if (uGameObj == null)
                {
                    Debug.LogError($"配置里有错误数据 动作ID:{item.animationId}");
                    continue;
                }

                //如果在SequenceAniList里就不单独执行
                if (!SequenceAniDic.ContainsKey(item.animationId))
                    this.Play(item);
            }


            //顺序动作
            if (SequenceAniList.Count > 0)
            {
                Sequence seq = DOTween.Sequence();
                foreach (var item in SequenceAniList)
                {
                    int animationId = item.animationId;
                    if (animationDic.ContainsKey(animationId))
                    {
                        AniUnitData aData = animationDic[animationId];
                        var tween = this.Play(aData);
                        if(tween!=null)
                            seq.Append(tween);
                    }
                }
                seq.Play();
            }

        }
        else
        {
            if (allComplete != null)
                allComplete();
        }
    }


    public void PlayBack(Action allComplete = null)
    {

        if (animationDataConfig.Count > 0)
        {
            if (playState == PlayState.Doing)
                return;
            if (allComplete != null)
                allCompleteHandle = allComplete;
            foreach (var item in animationDataConfig)
            {
                UGameObject uGameObj = item.obj;
                if (uGameObj == null)
                {
                    Debug.LogError($"配置里有错误数据 动作ID:{item.animationId}");
                    continue;
                }

                //如果在SequenceAniList里就不单独执行
                if (!SequenceAniDic.ContainsKey(item.animationId))
                    this.Play(item, true);
            }


            //顺序动作
            if (SequenceAniList.Count > 0)
            {
                Sequence seq = DOTween.Sequence();
                foreach (var item in SequenceAniList)
                {
                    int animationId = item.animationId;
                    if (animationDic.ContainsKey(animationId))
                    {
                        AniUnitData aData = animationDic[animationId];
                        var tween = this.Play(aData, true);
                        seq.Append(tween);
                    }
                }
                seq.Play();
            }

        }
        else
        {
            if (allComplete != null)
                allComplete();
        }
    }

    private Tween _DOMoveTo(AniUnitData item, bool isBack = false)
    {
        Tween twe = null;
        UGameObject uGameObj = item.obj;
        if (!isBack)
        {
            return this._DOMoveTo(uGameObj, item.endValue, item.duration, () => { AnimationEnd(item.animationId); }, item.uIAnimationControlType, item.startValue, false, item.isLoop, item.delayTime, item.animationCurve, item.loopType);
        }
        return this._DOMoveTo(uGameObj, item.startValue, item.duration, () => { AnimationEnd(item.animationId); }, item.uIAnimationControlType, item.endValue, false, item.isLoop, item.delayTime, item.animationCurve, item.loopType);
    }

    private Tween _DOMoveBy(AniUnitData item, bool isBack = false)
    {
        Tween twe = null;
        UGameObject uGameObj = item.obj;
        if (!isBack)
            return this._DOMoveBy(uGameObj, item.endValue, item.duration, () => { AnimationEnd(item.animationId); }, item.uIAnimationControlType, false, item.isLoop, item.delayTime, item.animationCurve, item.loopType);
        return this._DOMoveBy(uGameObj, -item.endValue, item.duration, () => { AnimationEnd(item.animationId); }, item.uIAnimationControlType, false, item.isLoop, item.delayTime, item.animationCurve, item.loopType);
    }

    private Tween _DOScaleTo(AniUnitData item, bool isBack = false)
    {
        Tween twe = null;
        UGameObject uGameObj = item.obj;
        if (!isBack)
            return this._DOScaleTo(uGameObj, item.endValue, item.duration, () => { AnimationEnd(item.animationId); }, item.uIAnimationControlType, item.startValue, item.isLoop, item.delayTime, item.animationCurve, item.loopType);
        return this._DOScaleTo(uGameObj, item.startValue, item.duration, () => { AnimationEnd(item.animationId); }, item.uIAnimationControlType, item.endValue, item.isLoop, item.delayTime, item.animationCurve, item.loopType);
    }

    private Tween _DOScaleBy(AniUnitData item, bool isBack = false)
    {
        Tween twe = null;
        UGameObject uGameObj = item.obj;
        if (!isBack)
            return this._DOScaleBy(uGameObj, item.endValue, item.duration, () => { AnimationEnd(item.animationId); }, item.uIAnimationControlType, item.isLoop, item.delayTime, item.animationCurve, item.loopType);
        return this._DOScaleBy(uGameObj, -item.endValue, item.duration, () => { AnimationEnd(item.animationId); }, item.uIAnimationControlType, item.isLoop, item.delayTime, item.animationCurve, item.loopType);
    }

    private Tween _DORotateTo(AniUnitData item, bool isBack = false)
    {
        Tween twe = null;
        UGameObject uGameObj = item.obj;
        if (!isBack)
            return this._DORotateTo(uGameObj, item.endValue, item.duration, () => { AnimationEnd(item.animationId); }, item.uIAnimationControlType, item.startValue, item.isLoop, item.delayTime, item.animationCurve, item.loopType);
        return this._DORotateTo(uGameObj, item.startValue, item.duration, () => { AnimationEnd(item.animationId); }, item.uIAnimationControlType, item.endValue, item.isLoop, item.delayTime, item.animationCurve, item.loopType);
    }

    private Tween _DORotateBy(AniUnitData item, bool isBack = false)
    {
        Tween twe = null;
        UGameObject uGameObj = item.obj;
        if (!isBack)
            return this._DORotateBy(uGameObj, item.endValue, item.duration, () => { AnimationEnd(item.animationId); }, item.uIAnimationControlType, item.isLoop, item.delayTime, item.animationCurve, item.loopType);
        return this._DORotateBy(uGameObj, -item.endValue, item.duration, () => { AnimationEnd(item.animationId); }, item.uIAnimationControlType, item.isLoop, item.delayTime, item.animationCurve, item.loopType);
    }

    private Tween _DOFade(AniUnitData item, bool isBack = false)
    {
        Tween twe = null;
        UGameObject uGameObj = item.obj;
        if (!isBack)
            return this._DOFade(uGameObj, item.endAlpha, item.duration, () => { AnimationEnd(item.animationId); }, item.uIAnimationControlType, item.startAlpha, item.isLoop, item.delayTime, item.animationCurve, item.loopType);
        return this._DOFade(uGameObj, item.startAlpha, item.duration, () => { AnimationEnd(item.animationId); }, item.uIAnimationControlType, item.endAlpha, item.isLoop, item.delayTime, item.animationCurve, item.loopType);
    }

    private Tween _DOColor(AniUnitData item, bool isBack = false)
    {
        Tween twe = null;
        UGameObject uGameObj = item.obj;
        if (!isBack)
            return this._DOColor(uGameObj, item.endColor, item.duration, () => { AnimationEnd(item.animationId); }, item.uIAnimationControlType, item.startColor, item.isLoop, item.delayTime, item.animationCurve, item.loopType);
        return this._DOColor(uGameObj, item.startColor, item.duration, () => { AnimationEnd(item.animationId); }, item.uIAnimationControlType, item.endColor, item.isLoop, item.delayTime, item.animationCurve, item.loopType);
    }

    private T[] Revers<T>( T[] datas)
    {
        T t;
        int len = datas.Length;
        for (int i = 0; i < len / 2; i++)
        {
            t = datas[i];
            datas[i] = datas[len - i - 1];
            datas[len - i - 1] = t;
        }
        return datas;
    }
    private Tween _DoGradientColor(AniUnitData item, bool isBack = false)
    {
 
        UGameObject uGameObj = item.obj;
        if (!isBack)
            return this._DOGradientColor(uGameObj, item.gradientColor, item.duration, () => { AnimationEnd(item.animationId); }, item.uIAnimationControlType, item.isLoop, item.delayTime, item.animationCurve, item.loopType);
        else
        {
            Gradient newGradient = new Gradient();
            newGradient.SetKeys((GradientColorKey[])item.gradientColor.colorKeys.Clone(), (GradientAlphaKey[])item.gradientColor.alphaKeys.Clone());
            newGradient.colorKeys = this.Revers<GradientColorKey>(newGradient.colorKeys);
            newGradient.alphaKeys = this.Revers<GradientAlphaKey>(newGradient.alphaKeys);
            return this._DOGradientColor(uGameObj, newGradient, item.duration, () => { AnimationEnd(item.animationId); }, item.uIAnimationControlType, item.isLoop, item.delayTime, item.animationCurve, item.loopType);

        }

    }

    private Tween _DOPath(AniUnitData item, bool isBack = false)
    {
        
        Tween twe = null;
        if (item.pathList.Count == 0)
            return twe;
        UGameObject uGameObj = item.obj;
        if (!isBack)
        {
           return this._DOPath(uGameObj, item.pathList, item.duration, () => { AnimationEnd(item.animationId); }, item.uIAnimationControlType, item.startValue, item.isLoop, item.delayTime, item.animationCurve, item.loopType);
        }
        else
        {
            
            List<Vector3> newPathPoints = new List<Vector3>();
            int count = item.pathList.Count;
            for (int i = count -1; i >= 0; i--)
            {
                newPathPoints.Add(item.pathList[i]);
            }
            var startPathPoint = item.pathList[count-1];
            newPathPoints.RemoveAt(0);
            newPathPoints.Add(item.startValue);
            return this._DOPath(uGameObj, newPathPoints, item.duration, () => { AnimationEnd(item.animationId); }, item.uIAnimationControlType, startPathPoint, item.isLoop, item.delayTime, item.animationCurve, item.loopType);
        }

    }

    private Tween _DOMaterialFloat(AniUnitData item, bool isBack = false)
    {

        UGameObject uGameObj = item.obj;
        if (!isBack)
        {
            return this._DOMaterialFloat(uGameObj, item.endAlpha, item.duration, item.propertyName, () => { AnimationEnd(item.animationId); }, item.uIAnimationControlType, item.startAlpha, item.isLoop, item.delayTime, item.animationCurve, item.loopType);
        }
        else
        {
            return this._DOMaterialFloat(uGameObj, item.startAlpha, item.duration, item.propertyName,() => { AnimationEnd(item.animationId); }, item.uIAnimationControlType, item.endAlpha, item.isLoop, item.delayTime, item.animationCurve, item.loopType);
        }
    }

    private Tween _DOMaterialColor(AniUnitData item, bool isBack = false)
    {

        UGameObject uGameObj = item.obj;
        if (!isBack)
        {
            return this._DOMaterialColor(uGameObj, item.endColor, item.duration, item.propertyName, () => { AnimationEnd(item.animationId); }, item.uIAnimationControlType, item.startColor, item.isLoop, item.delayTime, item.animationCurve, item.loopType);
        }
        else
        {
            return this._DOMaterialColor(uGameObj, item.startColor, item.duration, item.propertyName, () => { AnimationEnd(item.animationId); }, item.uIAnimationControlType, item.endColor, item.isLoop, item.delayTime, item.animationCurve, item.loopType);
        }

    }


    #region 原子接口

    private Tween _DOMoveTo(GameObject obj, Vector3 endPos, float time , Action endCallBack = null, UIAnimationControlType controltype = UIAnimationControlType.Linear,Vector3 startPos = default(Vector3), bool isWorld = false, bool isLoop = false, float delayT = 0, AnimationCurve animationCurve = null, LoopType loopType = LoopType.Restart)
    {
        
        Transform tran = obj.transform;
        Tween twe = null;
        if (isWorld)
        {
            //世界坐标移动
            if (startPos != null)
            {
                tran.position = startPos;
            }
            twe = tran.DOMove(endPos, time);
        }
        else
        {
            if (startPos != null)
            {
                tran.localPosition = startPos;
            }
            //局部坐标移动
            twe = tran.DOLocalMove(endPos, time);
        }
        if (controltype != UIAnimationControlType.AnimationCurve)
        {
            twe.SetEase((Ease)controltype);
        }
        else
        {
            if (animationCurve != null)
            {
                twe.SetEase(animationCurve);
            }
        }

        if(isLoop)
            twe.SetLoops(-1, loopType);
        if (delayT > 0)
        {
            twe.SetDelay(delayT);
        }

        if (endCallBack != null)
        {
            twe.OnComplete(() => {
                endCallBack();
            });
        }
        
        return twe;
    }

    private Tween _DOMoveBy(GameObject obj, Vector3 offsetPos, float time, Action endCallBack = null, UIAnimationControlType controltype = UIAnimationControlType.Linear, bool isWorld = false, bool isLoop = false, float delayT = 0, AnimationCurve animationCurve = null, LoopType loopType = LoopType.Restart)
    {

        Transform tran = obj.transform;
        Tween twe = null;
        if (isWorld)
        {
            //世界坐标移动
            twe = tran.DOMove(offsetPos, time).SetRelative(true);
        }
        else
        {
            //局部坐标移动
            twe = tran.DOLocalMove(offsetPos, time).SetRelative(true);
        }
        if (controltype != UIAnimationControlType.AnimationCurve)
        {
            twe.SetEase((Ease)controltype);
        }
        else
        {
            if (animationCurve != null)
            {
                twe.SetEase(animationCurve);
            }
        }

        if (isLoop)
            twe.SetLoops(-1, loopType);
        if (delayT > 0)
        {
            twe.SetDelay(delayT);
        }

        if (endCallBack != null)
        {
            twe.OnComplete(() =>
            {
                endCallBack();
            });
        }
        
        return twe;
    }

    private Tween _DOScaleTo(GameObject obj, Vector3 endScale, float time, Action endCallBack = null, UIAnimationControlType controltype = UIAnimationControlType.Linear, Vector3 startScale = default(Vector3), bool isLoop = false, float delayT = 0, AnimationCurve animationCurve = null, LoopType loopType = LoopType.Restart)
    {

        Transform tran = obj.transform;
        Tween twe = null;
        if (startScale != null)
        {
            tran.localScale = startScale;
        }
        twe = tran.DOScale(endScale, time); //修改的就是localScale
        if (controltype != UIAnimationControlType.AnimationCurve)
        {
            twe.SetEase((Ease)controltype);
        }
        else
        {
            if (animationCurve != null)
            {
                twe.SetEase(animationCurve);
            }
        }

        if (isLoop)
            twe.SetLoops(-1, loopType);
        if (delayT > 0)
        {
            twe.SetDelay(delayT);
        }

        if (endCallBack != null)
        {
            twe.OnComplete(() =>
            {
                endCallBack();
            });
        }
        
        return twe;
    }

    private Tween _DOScaleBy(GameObject obj, Vector3 endScale, float time, Action endCallBack = null, UIAnimationControlType controltype = UIAnimationControlType.Linear, bool isLoop = false, float delayT = 0, AnimationCurve animationCurve = null, LoopType loopType = LoopType.Restart)
    {

        Transform tran = obj.transform;
        Tween twe = null;
        
        twe = tran.DOScale(endScale, time).SetRelative(true); //修改的就是localScale
        if (controltype != UIAnimationControlType.AnimationCurve)
        {
            twe.SetEase((Ease)controltype);
        }
        else
        {
            if (animationCurve != null)
            {
                twe.SetEase(animationCurve);
            }
        }

        if (isLoop)
            twe.SetLoops(-1, loopType);
        if (delayT > 0)
        {
            twe.SetDelay(delayT);
        }

        if (endCallBack != null)
        {
            twe.OnComplete(() =>
            {
                endCallBack();
            });
        }
        
        return twe;
    }

    private Tween _DORotateTo(GameObject obj, Vector3 endEulerAngles, float time, Action endCallBack = null, UIAnimationControlType controltype = UIAnimationControlType.Linear, Vector3 startEulerAngles = default(Vector3), bool isLoop = false, float delayT = 0, AnimationCurve animationCurve = null, LoopType loopType = LoopType.Restart)
    {

        Transform tran = obj.transform;
       
        Tween twe = null;
        if (startEulerAngles != null)
        {
            tran.eulerAngles = startEulerAngles;
        }
        twe = tran.DORotate(endEulerAngles, time);

        if (controltype != UIAnimationControlType.AnimationCurve)
        {
            twe.SetEase((Ease)controltype);
        }
        else
        {
            if (animationCurve != null)
            {
                twe.SetEase(animationCurve);
            }
        }

        if (isLoop)
            twe.SetLoops(-1, loopType);
        if (delayT > 0)
        {
            twe.SetDelay(delayT);
        }

        if (endCallBack != null)
        {
            twe.OnComplete(() =>
            {
                endCallBack();
            });
        }
        
        return twe;
    }
    private Tween _DORotateBy(GameObject obj, Vector3 endEulerAngles, float time, Action endCallBack = null, UIAnimationControlType controltype = UIAnimationControlType.Linear, bool isLoop = false, float delayT = 0, AnimationCurve animationCurve = null, LoopType loopType = LoopType.Restart)
    {

        Transform tran = obj.transform;
        Tween twe = null;
        twe = tran.DORotate(endEulerAngles, time).SetRelative(true);

        if (controltype != UIAnimationControlType.AnimationCurve)
        {
            twe.SetEase((Ease)controltype);
        }
        else
        {
            if (animationCurve != null)
            {
                twe.SetEase(animationCurve);
            }
        }

        if (isLoop)
            twe.SetLoops(-1, loopType);
        if (delayT > 0)
        {
            twe.SetDelay(delayT);
        }

        if (endCallBack != null)
        {
            twe.OnComplete(() =>
            {
                endCallBack();
            });
        }
        
        return twe;
    }


    private Tween _DOFade(GameObject obj, float endAlpha, float time, Action endCallBack = null, UIAnimationControlType controltype = UIAnimationControlType.Linear, float startAlpha = 0, bool isLoop = false, float delayT = 0, AnimationCurve animationCurve = null, LoopType loopType = LoopType.Restart)
    {
        CanvasGroup canvasGroup = obj.GetComponent<CanvasGroup>();;
        if (canvasGroup == null)
        {
            canvasGroup = obj.AddComponent<CanvasGroup>();
        }


        Tween twe = null;
        if (startAlpha != null)
        {
            canvasGroup.alpha = startAlpha;
        }
        twe = canvasGroup.DOFade(endAlpha, time);

        if (controltype != UIAnimationControlType.AnimationCurve)
        {
            twe.SetEase((Ease)controltype);
        }
        else
        {
            if (animationCurve != null)
            {
                twe.SetEase(animationCurve);
            }
        }

        if (isLoop)
            twe.SetLoops(-1, loopType);
        if (delayT > 0)
        {
            twe.SetDelay(delayT);
        }

        if (endCallBack != null)
        {
            twe.OnComplete(() =>
            {
                endCallBack();
            });
        }
        
        return twe;
    }

    private Tween _DOColor(GameObject obj, Color endColor, float time, Action endCallBack = null, UIAnimationControlType controltype = UIAnimationControlType.Linear, Color startColor = default(Color), bool isLoop = false, float delayT = 0, AnimationCurve animationCurve = null, LoopType loopType = LoopType.Restart)
    {
        Image imgCom = obj.GetComponent<Image>();
        Tween twe = null;
        if (imgCom == null)
        {
            endtotalAniCount++;
            Debug.LogError($"{obj.name} 上没有Image组件");
            return twe;
        }

        if (startColor != null)
        {
            imgCom.color = startColor;
        }
        twe = imgCom.DOColor(endColor, time);

        if (controltype != UIAnimationControlType.AnimationCurve)
        {
            twe.SetEase((Ease)controltype);
        }
        else
        {
            if (animationCurve != null)
            {
                twe.SetEase(animationCurve);
            }
        }

        if (isLoop)
            twe.SetLoops(-1, loopType);
        if (delayT > 0)
        {
            twe.SetDelay(delayT);
        }

        if (endCallBack != null)
        {
            twe.OnComplete(() =>
            {
                endCallBack();
            });
        }
        
        return twe;
    }

    private Tween _DOGradientColor(GameObject obj, Gradient gradient, float time, Action endCallBack = null, UIAnimationControlType controltype = UIAnimationControlType.Linear, bool isLoop = false, float delayT = 0, AnimationCurve animationCurve = null, LoopType loopType = LoopType.Restart)
    {
        Image imgCom = obj.GetComponent<Image>();
        Tween twe = null;
        if (imgCom == null)
        {
            endtotalAniCount++;
            Debug.LogError($"{obj.name} 上没有Image组件");
            return twe;
        }

        twe = imgCom.DOGradientColor(gradient, time);

        if (controltype != UIAnimationControlType.AnimationCurve)
        {
            twe.SetEase((Ease)controltype);
        }
        else
        {
            if (animationCurve != null)
            {
                twe.SetEase(animationCurve);
            }
        }

        if (isLoop)
            twe.SetLoops(-1, loopType);
        if (delayT > 0)
        {
            twe.SetDelay(delayT);
        }

        if (endCallBack != null)
        {
            twe.OnComplete(() =>
            {
                endCallBack();
            });
        }
        
        return twe;
    }

    private Tween _DOPath(GameObject obj, List<Vector3> pathPoints, float time, Action endCallBack = null, UIAnimationControlType controltype = UIAnimationControlType.Linear, Vector3 startPos = default(Vector3), bool isLoop = false, float delayT = 0, AnimationCurve animationCurve = null, LoopType loopType = LoopType.Restart)
    {
        Transform transCom = obj.GetComponent<Transform>();
        Tween twe = null;
        if (startPos != null)
        {
            transCom.localPosition = startPos;
        }
        twe = transCom.DOLocalPath(pathPoints.ToArray(), time, pathType: PathType.CatmullRom, gizmoColor: Color.red);

        if (controltype != UIAnimationControlType.AnimationCurve)
        {
            twe.SetEase((Ease)controltype);
        }
        else
        {
            if (animationCurve != null)
            {
                twe.SetEase(animationCurve);
            }
        }

        if (isLoop)
            twe.SetLoops(-1, loopType);
        if (delayT > 0)
        {
            twe.SetDelay(delayT);
        }

        if (endCallBack != null)
        {
            twe.OnComplete(() =>
            {
                endCallBack();
            });
        }

        return twe;
    }

    //尽量少用
    private Tween _DOMaterialFloat(GameObject obj, float endValue, float time, string property, Action endCallBack = null, UIAnimationControlType controltype = UIAnimationControlType.Linear, float startValue = 0, bool isLoop = false, float delayT = 0, AnimationCurve animationCurve = null, LoopType loopType = LoopType.Restart)
    {
        Tween twe = null;
        Image imgCom = obj.GetComponent<Image>();
        if (imgCom == null)
        {
            endtotalAniCount++;
            Debug.LogError($"{obj.name} 上没有Image组件");
            return twe;
        }

        Material material = this.GetRecyCleMaterial(imgCom.GetInstanceID());
        if (material == null)
        {
            material = new Material(imgCom.material);
            RecycleMaterialData recycleMaterialData = new RecycleMaterialData()
            {
                imgCom = imgCom,
                mat = material

            };
            this.AddRecycleMaterialData(recycleMaterialData);
        }
        imgCom.material = material;

        if (startValue != 0)
        {
            material.SetFloat(property, startValue);
        }
        material.DOFloat(endValue, property, time);
        if (controltype != UIAnimationControlType.AnimationCurve)
        {
            twe.SetEase((Ease)controltype);
        }
        else
        {
            if (animationCurve != null)
            {
                twe.SetEase(animationCurve);
            }
        }

        if (isLoop)
            twe.SetLoops(-1, loopType);
        if (delayT > 0)
        {
            twe.SetDelay(delayT);
        }

        if (endCallBack != null)
        {
            twe.OnComplete(() =>
            {
                endCallBack();
            });
        }


        return twe;
    }

    private Tween _DOMaterialColor(GameObject obj, Color endColor, float time, string property, Action endCallBack = null, UIAnimationControlType controltype = UIAnimationControlType.Linear, Color startColor = default(Color), bool isLoop = false, float delayT = 0, AnimationCurve animationCurve = null, LoopType loopType = LoopType.Restart)
    {
        Tween twe = null;
        Image imgCom = obj.GetComponent<Image>();
        if (imgCom == null)
        {
            endtotalAniCount++;
            Debug.LogError($"{obj.name} 上没有Image组件");
            return twe;
        }

        Material material = this.GetRecyCleMaterial(imgCom.GetInstanceID());
        if (material == null)
        {
            material = new Material(imgCom.material);
            RecycleMaterialData recycleMaterialData = new RecycleMaterialData()
            {
                imgCom = imgCom,
                mat = material

            };
            this.AddRecycleMaterialData(recycleMaterialData);
        }
        imgCom.material = material;

        if (startColor != null)
        {
            material.SetColor(property, startColor);
        }
        material.DOColor(endColor, property, time);
        if (controltype != UIAnimationControlType.AnimationCurve)
        {
            twe.SetEase((Ease)controltype);
        }
        else
        {
            if (animationCurve != null)
            {
                twe.SetEase(animationCurve);
            }
        }

        if (isLoop)
            twe.SetLoops(-1, loopType);
        if (delayT > 0)
        {
            twe.SetDelay(delayT);
        }

        if (endCallBack != null)
        {
            twe.OnComplete(() =>
            {
                endCallBack();
            });
        }


        return twe;
    }

    #endregion

    #region 暴露给Lua接口
    public static void Play(Tween tween)
        {
            tween.Play();
        }

        public static void Stop(Tween tween)
        {
            tween.Kill();
        }

        public static void StopAll()
        {
            DOTween.KillAll();
        }

        public static void Pause(Tween tween)
        {
            tween.Pause();
        }
        public static void Resume(Tween tween)
        {
            if (!tween.IsPlaying())
                tween.Play();
        }

        public static void Restart(Tween tween)
        {
            tween.Restart();
        }

    /// <param name="obj"> 目标对象</param>
    /// <param name="endPos">目标位置</param>
    /// <param name="time">动作时间</param>
    /// <param name="endCallBack">结束回调</param>
    /// <param name="controltype">控制类型</param>
    /// <param name="startPos">起始位置</param>
    /// <param name="isWorld">是否是世界坐标移动</param>
    /// <param name="isLoop">是否循环播放</param>
    /// <param name="delayT">延迟时间</param>
    /// <returns></returns>
    public static Tween DOMoveTo(GameObject obj, Vector3 endPos, float time, bool isAutokill = true, Action endCallBack = null, UIAnimationControlType controltype = UIAnimationControlType.Linear, Vector3 startPos = default(Vector3), bool isWorld = false, bool isLoop = false, float delayT = 0, AnimationCurve animationCurve = null, LoopType loopType = LoopType.Restart)
    {

        Transform tran = obj.transform;
        Tween twe = null;
        if (isWorld)
        {
            //世界坐标移动
            if (startPos != null)
            {
                tran.position = startPos;
            }
            twe = tran.DOMove(endPos, time);
        }
        else
        {
            if (startPos != null)
            {
                tran.localPosition = startPos;
            }
            //局部坐标移动
            twe = tran.DOLocalMove(endPos, time);
        }
        if (controltype != UIAnimationControlType.AnimationCurve)
        {
            twe.SetEase((Ease)controltype);
        }
        else
        {
            if (animationCurve != null)
            {
                twe.SetEase(animationCurve);
            }
        }

        if (isLoop)
            twe.SetLoops(-1, loopType);
        if (delayT > 0)
        {
            twe.SetDelay(delayT);
        }

        if (endCallBack != null)
        {
            twe.OnComplete(() =>
            {
                endCallBack();
            });
        }
        twe.SetAutoKill(isAutokill);
        return twe;
    }

    /// <param name="obj"> 目标对象</param>
    /// <param name="endPos">目标位置</param>
    /// <param name="time">动作时间</param>
    /// <param name="endCallBack">结束回调</param>
    /// <param name="controltype">控制类型</param>
    /// <param name="offsetPos">相对位移</param>
    /// <param name="isWorld">是否是世界坐标移动</param>
    /// <param name="isLoop">是否循环播放</param>
    /// <param name="delayT">延迟时间</param>
    /// <returns></returns>
    public static Tween DOMoveBy(GameObject obj, Vector3 offsetPos, float time, bool isAutokill = true, Action endCallBack = null, UIAnimationControlType controltype = UIAnimationControlType.Linear, bool isWorld = false, bool isLoop = false, float delayT = 0, AnimationCurve animationCurve = null, LoopType loopType = LoopType.Restart)
    {

        Transform tran = obj.transform;
        Tween twe = null;
        if (isWorld)
        {
            //世界坐标移动
            twe = tran.DOMove(offsetPos, time).SetRelative(true);
        }
        else
        {
            //局部坐标移动
            twe = tran.DOMove(offsetPos, time).SetRelative(true);
        }
        if (controltype != UIAnimationControlType.AnimationCurve)
        {
            twe.SetEase((Ease)controltype);
        }
        else
        {
            if (animationCurve != null)
            {
                twe.SetEase(animationCurve);
            }
        }

        if (isLoop)
            twe.SetLoops(-1, loopType);
        if (delayT > 0)
        {
            twe.SetDelay(delayT);
        }

        if (endCallBack != null)
        {
            twe.OnComplete(() =>
            {
                endCallBack();
            });
        }
        twe.SetAutoKill(isAutokill);
        return twe;
    }


    /// <param name="obj"> 目标对象</param>
    /// <param name="endScale">目标大小</param>
    /// <param name="time">动作时间</param>
    /// <param name="endCallBack">结束回调</param>
    /// <param name="controltype">控制类型</param>
    /// <param name="startScale">起始大小</param>
    /// <param name="isLoop">是否循环播放</param>
    /// <param name="delayT">延迟时间</param>
    /// <returns></returns>
    public static Tween DOScaleTo(GameObject obj, Vector3 endScale, float time, bool isAutokill = true, Action endCallBack = null, UIAnimationControlType controltype = UIAnimationControlType.Linear, Vector3 startScale = default(Vector3), bool isLoop = false, float delayT = 0, AnimationCurve animationCurve = null, LoopType loopType = LoopType.Restart)
    {

        Transform tran = obj.transform;
        Tween twe = null;
        if (startScale != null)
        {
            tran.localScale = startScale;
        }
        twe = tran.DOScale(endScale, time); //修改的就是localScale
        if (controltype != UIAnimationControlType.AnimationCurve)
        {
            twe.SetEase((Ease)controltype);
        }
        else
        {
            if (animationCurve != null)
            {
                twe.SetEase(animationCurve);
            }
        }

        if (isLoop)
            twe.SetLoops(-1, loopType);
        if (delayT > 0)
        {
            twe.SetDelay(delayT);
        }

        if (endCallBack != null)
        {
            twe.OnComplete(() =>
            {
                endCallBack();
            });
        }
        twe.SetAutoKill(isAutokill);
        return twe;
    }

    /// <param name="obj"> 目标对象</param>
    /// <param name="endScale">目标大小</param>
    /// <param name="time">动作时间</param>
    /// <param name="endCallBack">结束回调</param>
    /// <param name="controltype">控制类型</param>
    /// <param name="isLoop">是否循环播放</param>
    /// <param name="delayT">延迟时间</param>
    /// <returns></returns>
    public static Tween DOScaleBy(GameObject obj, Vector3 endScale, float time, bool isAutokill = true, Action endCallBack = null, UIAnimationControlType controltype = UIAnimationControlType.Linear, bool isLoop = false, float delayT = 0, AnimationCurve animationCurve = null, LoopType loopType = LoopType.Restart)
    {

        Transform tran = obj.transform;
        Tween twe = null;

        twe = tran.DOScale(endScale, time).SetRelative(true); //修改的就是localScale
        if (controltype != UIAnimationControlType.AnimationCurve)
        {
            twe.SetEase((Ease)controltype);
        }
        else
        {
            if (animationCurve != null)
            {
                twe.SetEase(animationCurve);
            }
        }

        if (isLoop)
            twe.SetLoops(-1, loopType);
        if (delayT > 0)
        {
            twe.SetDelay(delayT);
        }

        if (endCallBack != null)
        {
            twe.OnComplete(() =>
            {
                endCallBack();
            });
        }
        twe.SetAutoKill(isAutokill);
        return twe;
    }

    /// <param name="obj"> 目标对象</param>
    /// <param name="endEulerAngles">目标欧拉角</param>
    /// <param name="time">动作时间</param>
    /// <param name="endCallBack">结束回调</param>
    /// <param name="controltype">控制类型</param>
    /// <param name="startEulerAngles">起始欧拉角</param>
    /// <param name="isLoop">是否循环播放</param>
    /// <param name="delayT">延迟时间</param>
    /// <returns></returns>
    public static Tween DORotateTo(GameObject obj, Vector3 endEulerAngles, float time, bool isAutokill = true, Action endCallBack = null, UIAnimationControlType controltype = UIAnimationControlType.Linear, Vector3 startEulerAngles = default(Vector3), bool isLoop = false, float delayT = 0, AnimationCurve animationCurve = null, LoopType loopType = LoopType.Restart)
    {

        Transform tran = obj.transform;

        Tween twe = null;
        if (startEulerAngles != null)
        {
            tran.eulerAngles = startEulerAngles;
        }
        twe = tran.DORotate(endEulerAngles, time);

        if (controltype != UIAnimationControlType.AnimationCurve)
        {
            twe.SetEase((Ease)controltype);
        }
        else
        {
            if (animationCurve != null)
            {
                twe.SetEase(animationCurve);
            }
        }

        if (isLoop)
            twe.SetLoops(-1, loopType);
        if (delayT > 0)
        {
            twe.SetDelay(delayT);
        }

        if (endCallBack != null)
        {
            twe.OnComplete(() =>
            {
                endCallBack();
            });
        }
        twe.SetAutoKill(isAutokill);
        return twe;
    }

    // <param name="obj"> 目标对象</param>
    /// <param name="endEulerAngles">目标欧拉角</param>
    /// <param name="time">动作时间</param>
    /// <param name="endCallBack">结束回调</param>
    /// <param name="controltype">控制类型</param>
    /// <param name="isLoop">是否循环播放</param>
    /// <param name="delayT">延迟时间</param>
    /// <returns></returns>
    public static Tween DORotateBy(GameObject obj, Vector3 endEulerAngles, float time, bool isAutokill = true, Action endCallBack = null, UIAnimationControlType controltype = UIAnimationControlType.Linear, bool isLoop = false, float delayT = 0, AnimationCurve animationCurve = null, LoopType loopType = LoopType.Restart)
    {

        Transform tran = obj.transform;
        Tween twe = null;
        twe = tran.DORotate(endEulerAngles, time).SetRelative(true);

        if (controltype != UIAnimationControlType.AnimationCurve)
        {
            twe.SetEase((Ease)controltype);
        }
        else
        {
            if (animationCurve != null)
            {
                twe.SetEase(animationCurve);
            }
        }

        if (isLoop)
            twe.SetLoops(-1, loopType);
        if (delayT > 0)
        {
            twe.SetDelay(delayT);
        }

        if (endCallBack != null)
        {
            twe.OnComplete(() =>
            {
                endCallBack();
            });
        }
        twe.SetAutoKill(isAutokill);
        return twe;
    }


    public static Tween DOFade(GameObject obj, float endAlpha, float time, bool isAutokill = true, Action endCallBack = null, UIAnimationControlType controltype = UIAnimationControlType.Linear, float startAlpha = 0, bool isLoop = false, float delayT = 0, AnimationCurve animationCurve = null, LoopType loopType = LoopType.Restart)
    {
        CanvasGroup canvasGroup = obj.GetComponent<CanvasGroup>(); ;
        if (canvasGroup == null)
        {
            canvasGroup = obj.AddComponent<CanvasGroup>();
        }


        Tween twe = null;
        if (startAlpha != null)
        {
            canvasGroup.alpha = startAlpha;
        }
        twe = canvasGroup.DOFade(endAlpha, time);

        if (controltype != UIAnimationControlType.AnimationCurve)
        {
            twe.SetEase((Ease)controltype);
        }
        else
        {
            if (animationCurve != null)
            {
                twe.SetEase(animationCurve);
            }
        }

        if (isLoop)
            twe.SetLoops(-1, loopType);
        if (delayT > 0)
        {
            twe.SetDelay(delayT);
        }

        if (endCallBack != null)
        {
            twe.OnComplete(() =>
            {
                endCallBack();
            });
        }
        twe.SetAutoKill(isAutokill);
        return twe;
    }

    public static Tween DOColor(GameObject obj, Color endColor, float time, bool isAutokill = true, Action endCallBack = null, UIAnimationControlType controltype = UIAnimationControlType.Linear, Color startColor = default(Color), bool isLoop = false, float delayT = 0, AnimationCurve animationCurve = null, LoopType loopType = LoopType.Restart)
    {
        Image imgCom = obj.GetComponent<Image>();
        Tween twe = null;
        if (imgCom == null)
            return twe;

        if (startColor != null)
        {
            imgCom.color = startColor;
        }
        twe = imgCom.DOColor(endColor, time);

        if (controltype != UIAnimationControlType.AnimationCurve)
        {
            twe.SetEase((Ease)controltype);
        }
        else
        {
            if (animationCurve != null)
            {
                twe.SetEase(animationCurve);
            }
        }

        if (isLoop)
            twe.SetLoops(-1, loopType);
        if (delayT > 0)
        {
            twe.SetDelay(delayT);
        }

        if (endCallBack != null)
        {
            twe.OnComplete(() =>
            {
                endCallBack();
            });
        }
        twe.SetAutoKill(isAutokill);
        return twe;
    }

    public static Tween DOGradientColor(GameObject obj, Gradient gradient, float time, bool isAutokill = true, Action endCallBack = null, UIAnimationControlType controltype = UIAnimationControlType.Linear, bool isLoop = false, float delayT = 0, AnimationCurve animationCurve = null, LoopType loopType = LoopType.Restart)
    {
        Image imgCom = obj.GetComponent<Image>();
        Tween twe = null;
        if (imgCom == null)
            return twe;

        twe = imgCom.DOGradientColor(gradient, time);

        if (controltype != UIAnimationControlType.AnimationCurve)
        {
            twe.SetEase((Ease)controltype);
        }
        else
        {
            if (animationCurve != null)
            {
                twe.SetEase(animationCurve);
            }
        }

        if (isLoop)
            twe.SetLoops(-1, loopType);
        if (delayT > 0)
        {
            twe.SetDelay(delayT);
        }

        if (endCallBack != null)
        {
            twe.OnComplete(() =>
            {
                endCallBack();
            });
        }
        twe.SetAutoKill(isAutokill);
        return twe;
    }

    public static Tween DOPath(GameObject obj, List<Vector3> pathPoints, float time, Action endCallBack = null, bool isAutokill = true, UIAnimationControlType controltype = UIAnimationControlType.Linear, bool isLoop = false, float delayT = 0, bool isWorld = false , AnimationCurve animationCurve = null, LoopType loopType = LoopType.Restart)
    {
        Transform transCom = obj.GetComponent<Transform>();
        Tween twe = null;
        if (transCom == null)
            return twe;
        if(isWorld)
        {
            twe = transCom.DOPath(pathPoints.ToArray(), time, pathType: PathType.CatmullRom, gizmoColor: Color.clear);
        }
        else
        {
            twe = transCom.DOLocalPath(pathPoints.ToArray(), time, pathType: PathType.CatmullRom, gizmoColor: Color.clear);
        }

        if (controltype != UIAnimationControlType.AnimationCurve)
        {
            twe.SetEase((Ease)controltype);
        }
        else
        {
            if (animationCurve != null)
            {
                twe.SetEase(animationCurve);
            }
        }

        if (isLoop)
            twe.SetLoops(-1, loopType);
        if (delayT > 0)
        {
            twe.SetDelay(delayT);
        }

        if (endCallBack != null)
        {
            twe.OnComplete(() =>
            {
                endCallBack();
            });
        }
        twe.SetAutoKill(isAutokill);
        return twe;
    }
    
    
    
    #endregion


    #region 退出相关操作
    private void OnDestroy()
    {
    
        animationDataConfig.Clear();
        SequenceAniList.Clear();
        SequenceAniDic.Clear();
        animationDic.Clear();
        this.RemoveAllRecycleMaterialData();
        animationDataConfig = null;
        SequenceAniList = null;
        SequenceAniDic = null;
        animationDic = null;
        recycleMaterialDataDic = null;
    }

    //private void OnDisable()
    //{
    //    this.RemoveAllRecycleMaterialData();
    //}


    #endregion


#if UNITY_EDITOR

    //检查shader中是否有对应属性
    private bool CheckPropertyIsExsit(Material mat, string propertyName)
    {
        if (string.IsNullOrEmpty(propertyName))
            return false;
        var shader = mat.shader;
        int count = ShaderUtil.GetPropertyCount(shader);
        for (int i = 0; i < count; i++)
        {
            string name = ShaderUtil.GetPropertyName(shader, i);
            if (string.Equals(name, propertyName))
            {
                return true;
            }
        }
        return false;
    }
#endif

}



